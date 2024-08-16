namespace Translations.Common.Types;

public static class UploadContextTypes
{
    public record UploadFilesContext : CommonContextType
    {
        public IFormFile[] Files { get; set; }
        public UploadFilesContext(string? GameFranchise, string? GameName, IFormFile[] FileEntries) : base(GameFranchise, GameName) 
        {
            Files = FileEntries;
        }
    
        public void Deconstruct(out string? gameFranchise, out string? gameName, out IFormFile[] files)
        {
            gameFranchise = GameFranchise;
            gameName = GameName;
            files = Files;
        }
    }
}
