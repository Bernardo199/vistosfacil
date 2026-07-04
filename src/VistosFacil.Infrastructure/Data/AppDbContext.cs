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
        m.Entity<Category>(e => { e.HasIndex(c => c.Slug).IsUnique(); e.Property(c => c.Name).HasMaxLength(100); });
        m.Entity<SiteConfig>(e => e.HasIndex(s => s.Key).IsUnique());
        m.Entity<Newsletter>(e => { e.HasIndex(n => n.Email).IsUnique(); e.Property(n => n.Email).HasMaxLength(254); });

        // Seed categorias
        m.Entity<Category>().HasData(
            new Category { Id=1, Name="Vistos Portugal", Slug="vistos-portugal", ColorClass="c-azul", TagLabel="Portugal", Emoji="🇵🇹", SortOrder=1 },
            new Category { Id=2, Name="Vistos EUA", Slug="vistos-eua", ColorClass="c-verde", TagLabel="EUA", Emoji="🇺🇸", SortOrder=2 },
            new Category { Id=3, Name="Vistos Schengen", Slug="vistos-schengen", ColorClass="c-azul", TagLabel="Schengen", Emoji="🇪🇺", SortOrder=3 },
            new Category { Id=4, Name="Autorização Residência", Slug="autorizacao-residencia", ColorClass="c-verde", TagLabel="Residência", Emoji="🏠", SortOrder=4 },
            new Category { Id=5, Name="Nacionalidade", Slug="nacionalidade", ColorClass="c-dourado", TagLabel="Nacionalidade", Emoji="📜", SortOrder=5 },
            new Category { Id=6, Name="Vistos Brasil", Slug="vistos-brasil", ColorClass="c-dourado", TagLabel="Brasil", Emoji="🇧🇷", SortOrder=6 }
        );

        // Seed configurações
        m.Entity<SiteConfig>().HasData(
            new SiteConfig { Id=1, Key="hero_title", Value="O seu guia de\nimigração em português", Description="Título do hero (\\n para quebra de linha)" },
            new SiteConfig { Id=2, Key="hero_subtitle", Value="Tudo sobre vistos, autorizações de residência e nacionalidade — explicado de forma simples para a diáspora lusófona.", Description="Subtítulo do hero" },
            new SiteConfig { Id=3, Key="site_name", Value="VistosFácil", Description="Nome do site" },
            new SiteConfig { Id=4, Key="adsense_client", Value="", Description="Google AdSense Publisher ID (ca-pub-XXXXXXXXXX)" },
            new SiteConfig { Id=5, Key="adsense_slot_1", Value="", Description="AdSense Slot ID 1" },
            new SiteConfig { Id=6, Key="google_analytics_id", Value="", Description="Google Analytics 4 ID (G-XXXXXXXXXX)" },
            new SiteConfig { Id=7, Key="trending_title", Value="Guias mais procurados", Description="Título da secção de artigos" }
        );
    }
}
