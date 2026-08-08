using Microsoft.EntityFrameworkCore;
using VistosFacil.Core.Entities;

namespace VistosFacil.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SiteConfig> SiteConfigs => Set<SiteConfig>();
    public DbSet<Newsletter> Newsletters => Set<Newsletter>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Article>(e => {
            e.HasIndex(a => a.Slug).IsUnique();
            e.HasIndex(a => a.Published);
            e.Property(a => a.Title).HasMaxLength(300);
            e.Property(a => a.Slug).HasMaxLength(300);
            e.HasOne(a => a.Category).WithMany(c => c.Articles).HasForeignKey(a => a.CategoryId);
        });
        m.Entity<Category>(e => e.HasIndex(c => c.Slug).IsUnique());
        m.Entity<SiteConfig>(e => e.HasIndex(s => s.Key).IsUnique());
        m.Entity<Newsletter>(e => e.HasIndex(n => n.Email).IsUnique());

        m.Entity<Category>().HasData(
            new Category { Id=1, Name="Vistos Portugal", Slug="vistos-portugal", ColorClass="c-azul", TagLabel="Portugal", Emoji="🇵🇹", SortOrder=1 },
            new Category { Id=2, Name="Vistos EUA", Slug="vistos-eua", ColorClass="c-verde", TagLabel="EUA", Emoji="🇺🇸", SortOrder=2 },
            new Category { Id=3, Name="Vistos Schengen", Slug="vistos-schengen", ColorClass="c-azul", TagLabel="Schengen", Emoji="🇪🇺", SortOrder=3 },
            new Category { Id=4, Name="Autorização Residência", Slug="autorizacao-residencia", ColorClass="c-verde", TagLabel="Residência", Emoji="🏠", SortOrder=4 },
            new Category { Id=5, Name="Nacionalidade", Slug="nacionalidade", ColorClass="c-dourado", TagLabel="Nacionalidade", Emoji="📜", SortOrder=5 },
            new Category { Id=6, Name="Vistos Brasil", Slug="vistos-brasil", ColorClass="c-dourado", TagLabel="Brasil", Emoji="🇧🇷", SortOrder=6 }
        );

        m.Entity<SiteConfig>().HasData(
            new SiteConfig { Id=1, Key="trending_title", Value="Guias mais procurados", Description="Título da secção" },
            new SiteConfig { Id=2, Key="adsense_client", Value="", Description="AdSense Publisher ID" },
            new SiteConfig { Id=3, Key="google_analytics_id", Value="", Description="GA4 ID" }
        );
    }
}
