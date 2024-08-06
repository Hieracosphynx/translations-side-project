using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Translations.Models;
using Translations.Common.Enums;
using Translations.Common.Utilities;
using Translations.Common.Constants;
using System.IO.Compression;
using System.Text;

namespace Translations.Services;

public class TranslationsService
{
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

    public async Task<byte[]> GenerateJSONDocumentsAsync(IEnumerable<LocalizedText> localizedTextResults, IEnumerable<LocalizedText> localizedTextCollection)
    {
        // Get all texts FOR EACH languages.
        var languages = Enum.GetValues(typeof(Language.Codes)).Cast<Language.Codes>();
        Language.Codes[] skipLanguages = [
            Language.Codes.Unknown, 
            Language.Codes.en_GB, 
            Language.Codes.en_US];

        var jsonFiles = new List<(string filename, string content)>();
        foreach(var language in languages)
        {
            if(skipLanguages.Contains(language)) { continue; }

            var foundTextDict = new Dictionary<string, string>();
            var notFoundTextDict = new Dictionary<string, string>();
            var languageString = language.ToString();
            var filename = Path.ChangeExtension(languageString, ".json");
            var notFoundFilename = Path.ChangeExtension(languageString + "_not_found", ".json");

            foreach(var localizedTextResult in localizedTextResults)
            {
                if(localizedTextResult.Text is null) { continue; }

                var localizedTextEntry = localizedTextCollection.Where(doc => doc.Key == localizedTextResult.Key && doc.Language == language).FirstOrDefault(); // TODO Probably do this in ProcessFileAsync OR cache / store result from that function and use it here??
                var key = localizedTextResult.Key;

                if(localizedTextEntry is null) 
                { 
                    notFoundTextDict.Add(key, localizedTextResult.Text);
                    continue; 
                }

                localizedTextEntry.Text ??= "";
                
                foundTextDict.Add(key, localizedTextEntry.Text);
            }

            var formattedJsonString = Tools.FormatDictionaryToJson(notFoundTextDict);
            if(notFoundTextDict.Count > 0)
            {
                jsonFiles.Add((notFoundFilename, formattedJsonString));
            }

            if(foundTextDict.Count == 0) { continue; }

            formattedJsonString = Tools.FormatDictionaryToJson(foundTextDict);
            jsonFiles.Add((filename, formattedJsonString));
        }

        // Creates all json files and put into a zip file.
        using(var memoryStream = new MemoryStream())
        {
            using(var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach(var (filename, content) in jsonFiles)
                {
                    var zipEntry = archive.CreateEntry(filename);
                    using(var zipStream = zipEntry.Open())
                    using(var writer = new StreamWriter(zipStream, Encoding.UTF8))
                    {
                        await writer.WriteAsync(content);
                    }
                }
            }

            memoryStream.Position = 0;
            return memoryStream.ToArray();
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
