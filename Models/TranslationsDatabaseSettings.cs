namespace Translations.Models;

public class TranslationsDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string TranslationsCollectionName { get; set; } = null!;
}
