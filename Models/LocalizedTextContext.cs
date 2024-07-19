using Microsoft.EntityFrameworkCore;

namespace Translations.Models;

public class LocalizedTextContext : DbContext
{
    public LocalizedTextContext(DbContextOptions<LocalizedTextContext> options) : base(options) { }
    public DbSet<LocalizedText> LocalizedTextEntries { get; set; } = null!;
}