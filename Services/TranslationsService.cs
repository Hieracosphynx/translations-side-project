using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Translations.Models;
using Translations.Common.Enums;
using Translations.Common.Utilities;
using Translations.Common.Constants;
using Translations.Common.Types;
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

    public async Task<IEnumerable<LocalizedText>> GetAsync(SearchTextContext textContext)
    {
        var localizedTexts = await _translationsCollection.Find(FilterDefinition<LocalizedText>.Empty).ToListAsync();

        return MatchLocalizedTextEntries(localizedTexts, textContext);
    }

    /**
    * Handles matching text from the file.
    **/
    public async Task<LocalizedText.Results> ProcessFileAsync(SearchFileContext fileContext, IEnumerable<LocalizedText> localizedTextCollection)
    {
        var (gameFranchise, gameName, jsonFile) = fileContext;

        List<LocalizedText> foundLocalizedTexts = [];
        List<LocalizedText> notFoundLocalizedTexts = [];

        if(jsonFile is null) { return new LocalizedText.Results([], []); }

        using(var reader = new StreamReader(jsonFile.OpenReadStream()))
        {
            while(!reader.EndOfStream)
            {
                var text = await reader.ReadLineAsync();

                if(text == null || text == "{" || text == "}") { continue; }
                
                var parsedTextEntry = RegexTools.ParseTextEntry(text);

                SearchTextContext searchCriteria = new(gameFranchise, gameName, parsedTextEntry.Value);

                var localizedText = MatchLocalizedTextEntries(localizedTextCollection, searchCriteria).FirstOrDefault();

                if(localizedText == null) 
                { 
                    var fileName = Path.GetFileNameWithoutExtension(jsonFile.FileName);
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

    // TODO: localizedTextResults SHOULD be LocalizedText.Results (contains both found and not found erntries)...
    public IEnumerable<LocalizedText.FileAndContent> GenerateJSONDocumentsAsync(LocalizedText.Results localizedTextResults, IEnumerable<LocalizedText> localizedTextCollection)
    {
        // Get all texts FOR EACH languages.
        var languages = Enum.GetValues(typeof(Language.Codes)).Cast<Language.Codes>();
        Language.Codes[] skipLanguages = [
            Language.Codes.Unknown, 
            Language.Codes.en_GB, 
            Language.Codes.en_US];

        var jsonFiles = new List<LocalizedText.FileAndContent>();

        if(localizedTextResults.NotFoundTextEntries.Count > 0)
        {
            var notInDatabaseName = "NotInDatabase.json";
            var notInDatabaseDict = new Dictionary<string, string>();
        
            foreach(var textEntry in localizedTextResults.NotFoundTextEntries)
            {
                if(textEntry.Text is null){ continue; }

                notInDatabaseDict.Add(textEntry.Key, textEntry.Text);
            }

            jsonFiles.Add(new(notInDatabaseName, notInDatabaseDict));
        }
        
        if(localizedTextResults.FoundTextEntries.Count > 0)
        {
            foreach(var language in languages)
            {
                if(skipLanguages.Contains(language)) { continue; }

                var foundTextDict = new Dictionary<string, string>();
                var notFoundTextDict = new Dictionary<string, string>();
                var languageString = language.ToString();
                var filename = Path.ChangeExtension(languageString, ".json");
                var notFoundFilename = Path.ChangeExtension(languageString + "_not_found", ".json");

                foreach(var localizedTextResult in localizedTextResults.FoundTextEntries)
                {
                    if(localizedTextResult.Text is null) { continue; }

                    var localizedTextEntry = localizedTextCollection.Where(doc => 
                        doc.Key == localizedTextResult.Key && 
                        doc.Language == language).FirstOrDefault();
                    var key = localizedTextResult.Key;

                    if(localizedTextEntry is null) 
                    { 
                        if(!notFoundTextDict.ContainsKey(key))
                        {
                            notFoundTextDict.Add(key, localizedTextResult.Text);
                        }
                        continue; 
                    }

                    localizedTextEntry.Text ??= "";

                    if(!foundTextDict.ContainsKey(key))
                    {
                        foundTextDict.Add(localizedTextEntry.Key, localizedTextEntry.Text);
                    }
                }

                var jsonString = notFoundTextDict;
                if(notFoundTextDict.Count > 0)
                {
                    jsonFiles.Add(new(notFoundFilename, jsonString));
                }

                if(foundTextDict.Count == 0) { continue; }

                jsonString = foundTextDict;
                jsonFiles.Add(new(filename, jsonString));
            }
        }

        return jsonFiles;
    }

    public async Task<byte[]> GenerateZipFileAsync(IEnumerable<LocalizedText.FileAndContent> results)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var result in results)
            {
                var zipEntry = archive.CreateEntry(result.Filename);
                using var zipStream = zipEntry.Open();
                using var writer = new StreamWriter(zipStream, Encoding.UTF8);
                var formattedJsonString = Tools.FormatDictionaryToJson(result.Content);
                await writer.WriteAsync(Tools.ParseJsonToReadable(formattedJsonString));
            }
        }

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    public async Task UploadAsync(LocalizedText.FormData formData)
    {
        var files = formData.Files;
        var skipStrings = new string[]{"", "{", "}"};
        for(var index = 0; index < files.Length; index++)
        {
            var file = files[index];
            using var reader = new StreamReader(file.OpenReadStream());
            while(!reader.EndOfStream)
            {
                var text = reader.ReadLine();

                if(skipStrings.Contains(text) || text == null) { continue; }

                var filename = Path.GetFileNameWithoutExtension(file.FileName);
                var parsedTextEntry = RegexTools.ParseTextEntry(text);
                var localizedTextEntry = new LocalizedText()
                {
                    Key = parsedTextEntry.Key,
                    Text = parsedTextEntry.Value,
                    Language = Language.GetLanguageCodeEnum(filename),
                    GameFranchise = formData.GameFranchise ?? "",
                    GameName = formData.GameName ?? "",
                };

                await CreateAsync(localizedTextEntry);
            }
        }
    }

    public async Task CreateAsync(LocalizedText newLocalizedText) =>
        await _translationsCollection.InsertOneAsync(newLocalizedText);

    public async Task UpdateAsync(string id, LocalizedText updatedLocalizedText) =>
        await _translationsCollection.ReplaceOneAsync(x => x.Id == id, updatedLocalizedText);

    public async Task RemoveAsync(string id) =>
        await _translationsCollection.DeleteOneAsync(x => x.Id == id);

    private static IEnumerable<LocalizedText> MatchLocalizedTextEntries(IEnumerable<LocalizedText> localizedTextEntries, SearchTextContext searchContext)
    {
        var isMatch = RegexTools.IsMatch;

        return localizedTextEntries.Where(doc =>
            isMatch(doc.Text, searchContext.Text, [RegexPatterns.SpecialCharExceptBraces, RegexPatterns.ComplexStringPattern]) &&
            (string.IsNullOrEmpty(searchContext.GameFranchise) || 
                isMatch(doc.GameFranchise, searchContext.GameFranchise, [RegexPatterns.SpecialCharactersPattern])) && 
            (string.IsNullOrEmpty(searchContext.GameName) || 
                isMatch(doc.GameName, searchContext.GameName, [RegexPatterns.SpecialCharactersPattern])));
    }
}
