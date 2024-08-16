namespace Translations.Common.Types;

public static class SearchContextTypes
{
    public record SearchContext : CommonContextType
    {
        public string? Text { get; set; }
        public IFormFile? JsonFile { get; set; }
        public SearchContext(string? GameFranchise, string? GameName, string? TextEntry, IFormFile? JsonFileEntry) : base(GameFranchise, GameName) 
        {
            Text = TextEntry;
            JsonFile = JsonFileEntry;
        }

        public void Deconstruct(out string? gameFranchise, out string? gameName, out string? text, out IFormFile? jsonFile) 
        {
            gameFranchise = GameFranchise;
            gameName = GameName;
            text = Text;
            jsonFile = JsonFile;
        }
    }

    public record SearchTextContext : CommonContextType
    {
        public string? Text { get; set; }
        public SearchTextContext(string? GameFranchise, string? GameName, string? TextEntry) : base(GameFranchise, GameName)
        {
            Text = TextEntry;
        }

        public void Deconstruct(out string? gameFranchise, out string? gameName, out string? text)
        {
            gameFranchise = GameFranchise;
            gameName = GameName;
            text = Text;
        }
    }

    public record SearchFileContext : CommonContextType
    {
        public IFormFile JsonFile { get; set; }
        public SearchFileContext(string? GameFranchise, string? GameName, IFormFile FileEntry) : base(GameFranchise, GameName)
        {
            JsonFile = FileEntry;
        }

        public void Deconstruct(out string? gameFranchise, out string? gameName, out IFormFile jsonFile)
        {
            gameFranchise = GameFranchise;
            gameName = GameName;
            jsonFile = JsonFile;
        }
    }
}
