using Translations.Models;
using Translations.Common.Utilities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.RegularExpressions;

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

    public async Task<List<LocalizedText>> GetAsync(string? text, string? gameFranchise, string? gameName)
    {
        var isTextEmpty = string.IsNullOrEmpty(text);
        var isGameFranchiseEmpty = string.IsNullOrEmpty(gameFranchise);
        var isGameNameEmpty = string.IsNullOrEmpty(gameName);

        var localizedTexts = await _translationsCollection.Find(FilterDefinition<LocalizedText>.Empty).ToListAsync();
        var preProcessedString = RegexTools.PreProcessString(text);
        var matchingLocTexts = localizedTexts.Where(doc => 
            (isTextEmpty || RegexTools.PreProcessString(doc.Text) == preProcessedString) &&
            (isGameFranchiseEmpty || doc.GameFranchise == gameFranchise) &&
            (isGameNameEmpty || doc.GameName == gameName)).ToList();

        return matchingLocTexts;
    }

    public async Task CreateAsync(LocalizedText newLocalizedText) =>
        await _translationsCollection.InsertOneAsync(newLocalizedText);

    public async Task UpdateAsync(string id, LocalizedText updatedLocalizedText) =>
        await _translationsCollection.ReplaceOneAsync(x => x.Id == id, updatedLocalizedText);

    public async Task RemoveAsync(string id) =>
        await _translationsCollection.DeleteOneAsync(x => x.Id == id);
}
