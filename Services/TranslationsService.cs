using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Translations.Models;
using Translations.Common.Enums;
using Translations.Common.Utilities;
using Translations.Common.Constants;
using Newtonsoft.Json.Linq;

namespace Translations.Services;

public class TranslationsService
{
    //private IEnumerable<LocalizedText> foundResults; TODO: Still pondering if we need this.
    private readonly IMongoCollection<LocalizedText> _translationsCollection;

    public TranslationsService(IOptions<TranslationsDatabaseSettings> translationsDbSettings)
    {
        var mongoClient = new MongoClient(
            translationsDbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            translationsDbSettings.Value.DatabaseName);

        _translationsCollection = mongoDatabase.GetCollection<LocalizedText>(
            translationsDbSettings.Value.TranslationsCollectionName);
    }

    public async Task<List<LocalizedText>> GetAsync() =>
        await _translationsCollection.Find(_ => true).ToListAsync();

    public async Task<LocalizedText?> GetAsync(string id) =>
        await _translationsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<LocalizedText>> GetAsync(string? text, string? gameFranchise, string? gameName)
    {
        URLParameters urlParams = new(text, gameFranchise, gameName);

        var localizedTexts = await _translationsCollection.Find(FilterDefinition<LocalizedText>.Empty).ToListAsync();

        return MatchLocalizedTextEntries(localizedTexts, urlParams);
    }

    /**
    * Handles matching text from the file.
    **/
    public async Task<LocalizedText.Results> ProcessFileAsync(IFormFile file, string? gameName, string? gameFranchise, IEnumerable<LocalizedText> localizedTextCollection)
    {
        List<LocalizedText> foundLocalizedTexts = [];
        List<LocalizedText> notFoundLocalizedTexts = [];

        using(var reader = new StreamReader(file.OpenReadStream()))
        {
            while(!reader.EndOfStream)
            {
                var text = await reader.ReadLineAsync();

                if(text == null || text == "{" || text == "}") { continue; }
                
                var parsedTextEntry = RegexTools.ParseTextEntry(text);
                URLParameters urlParams = new(parsedTextEntry.Value, gameFranchise, gameName);

                var localizedText = MatchLocalizedTextEntries(localizedTextCollection, urlParams).FirstOrDefault();

                if(localizedText == null) 
                { 
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    LocalizedText localizedTextEntry = new(){
                        Key = parsedTextEntry.Key,
                        Text = parsedTextEntry.Value,
                        Language = Language.GetLanguageCodeEnum(fileName),
                        GameName = gameName,
                        GameFranchise = gameFranchise
                    };
                    
                    notFoundLocalizedTexts.Add(localizedTextEntry);

                    continue; 
                }

                foundLocalizedTexts.Add(localizedText);
            }
        }

        return new LocalizedText.Results(foundLocalizedTexts, notFoundLocalizedTexts);
    }

    public async Task GenerateJSONDocumentsAsync(IEnumerable<LocalizedText> localizedTextResults, IEnumerable<LocalizedText> localizedTextCollection)
    {
        // Get all texts FOR EACH languages.
        var languages = Enum.GetValues(typeof(Language.Codes)).Cast<Language.Codes>();
        Language.Codes[] skipLanguages = [
            Language.Codes.Unknown, 
            Language.Codes.en_GB, 
            Language.Codes.en_US];

        foreach(var language in languages)
        {
            if(skipLanguages.Contains(language)) { continue; }

            var filename = language.ToString()+".json";
            Console.WriteLine(filename);
            var locTextDictionary = new Dictionary<string, string?>();

            foreach(var localizedTextResult in localizedTextResults)
            {
                if(localizedTextResult.Text == null) { continue; }

                // TODO
                var localizedTextEntry = MatchLocalizedTextEntries(
                    localizedTextCollection, new URLParameters(){
                        Text = localizedTextResult.Text,
                        GameFranchise = localizedTextResult.GameFranchise,
                        GameName = localizedTextResult.GameName
                    }).Where(doc => doc.Language == language).FirstOrDefault(); // TODO Probably do this in ProcessFileAsync OR cache / store result from that function and use it here??

                if(localizedTextEntry == null) 
                { 
                    Console.WriteLine("Nothing found");
                    continue; 
                }

                Console.WriteLine(localizedTextEntry.Text);

                locTextDictionary.Add(localizedTextEntry.Key, localizedTextEntry.Text);
            }

            string jsonString = RegexTools.ParseUnicodeString(
                JsonSerializer.Serialize(locTextDictionary));
            string formattedJsonString = JToken.Parse(jsonString)
                .ToString(Newtonsoft.Json.Formatting.Indented);

            // TODO: Only use for development. This will get removed.
            var path = @"C:\Users\corte\Downloads\" + filename;
            await File.WriteAllTextAsync(path, formattedJsonString);
        }
    }

    public async Task CreateAsync(LocalizedText newLocalizedText) =>
        await _translationsCollection.InsertOneAsync(newLocalizedText);

    public async Task UpdateAsync(string id, LocalizedText updatedLocalizedText) =>
        await _translationsCollection.ReplaceOneAsync(x => x.Id == id, updatedLocalizedText);

    public async Task RemoveAsync(string id) =>
        await _translationsCollection.DeleteOneAsync(x => x.Id == id);

    private static IEnumerable<LocalizedText> MatchLocalizedTextEntries(IEnumerable<LocalizedText> localizedTextEntries, URLParameters urlParams)
    {
        var isMatch = RegexTools.IsMatch;

        return localizedTextEntries.Where(doc =>
            isMatch(doc.Text, urlParams.Text, [RegexPatterns.SpecialCharExceptBraces,RegexPatterns.ComplexStringPattern]) &&
            (string.IsNullOrEmpty(urlParams.GameFranchise) || 
                isMatch(doc.GameFranchise, urlParams.GameFranchise, [RegexPatterns.SpecialCharactersPattern])) && 
            (string.IsNullOrEmpty(urlParams.GameName) || 
                isMatch(doc.GameName, urlParams.GameName, [RegexPatterns.SpecialCharactersPattern])));
    }

    // TODO: Not sure if this is the best way but I'll go with it.
    private struct URLParameters(string? text, string? gameFranchise, string? gameName)
    {
        public string? Text { get; set; } = text;
        public string? GameName { get; set; } = gameName;
        public string? GameFranchise { get; set; } = gameFranchise;
    }
}
