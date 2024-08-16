namespace Translations.Common.Types;

public record CommonContextType(string? GameFranchise, string? GameName)
{
    public void Deconstruct(out string? gameFranchise, out string? gameName)
    {
        gameFranchise = GameFranchise;
        gameName = GameName;
    }
}
