namespace Translations.Common.Types;

public record SearchContext(string? GameFranchise, string? GameName, string? Text, IFormFile? JsonFile)
{
    public void Deconstruct(out string? gameFranchise, out string? gameName, out string? text, out IFormFile? jsonFile)
    {
        gameFranchise = GameFranchise;
        gameName = GameName;
        text = Text;
        jsonFile = JsonFile;
    }
}

public record SearchTextContext(string? GameFranchise, string? GameName, string? Text)
{
    public void Deconstruct(out string? gameFranchise, out string? gameName, out string? text)
    {
        gameFranchise = GameFranchise;
        gameName = GameName;
        text = Text;
    }
}

public class SearchFileContext(string? GameFranchise, string? GameName, IFormFile JsonFile)
{
    public void Deconstruct(out string? gameFranchise, out string? gameName, out IFormFile jsonFile)
    {
        gameFranchise = GameFranchise;
        gameName = GameName;
        jsonFile = JsonFile;
    }
}
