using Microsoft.EntityFrameworkCore;
using VistosFacil.Core.Entities;
using VistosFacil.Infrastructure.Data;

namespace VistosFacil.Infrastructure.Repositories;

public class ArticleRepository
{
    private readonly AppDbContext _db;
    public ArticleRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Article>> GetPublishedAsync(int page = 1, int pageSize = 9)
        => await _db.Articles.Where(a => a.Published).Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Article>> GetFeaturedAsync(int count = 3)
        => await _db.Articles.Where(a => a.Published && a.Featured).Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt).Take(count).AsNoTracking().ToListAsync();

    public async Task<Article?> GetBySlugAsync(string slug)
        => await _db.Articles.Include(a => a.Category).AsNoTracking()
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Published);

    public async Task<IEnumerable<Article>> GetByCategoryAsync(string slug, int page = 1, int pageSize = 9)
        => await _db.Articles.Where(a => a.Published && a.Category.Slug == slug).Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt).Skip((page-1)*pageSize).Take(pageSize).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Article>> SearchAsync(string q)
        => await _db.Articles.Where(a => a.Published && (a.Title.Contains(q) || a.Summary.Contains(q)))
            .Include(a => a.Category).OrderByDescending(a => a.CreatedAt).Take(20).AsNoTracking().ToListAsync();

    public async Task<IEnumerable<Article>> GetAllAsync()
        => await _db.Articles.Include(a => a.Category).OrderByDescending(a => a.CreatedAt).AsNoTracking().ToListAsync();

    public async Task<int> GetTotalCountAsync(bool publishedOnly = true)
        => await _db.Articles.CountAsync(a => !publishedOnly || a.Published);

    public async Task<Article> CreateAsync(Article a)
    { _db.Articles.Add(a); await _db.SaveChangesAsync(); return a; }

    public async Task<Article> UpdateAsync(Article a)
    { a.UpdatedAt = DateTime.UtcNow; _db.Articles.Update(a); await _db.SaveChangesAsync(); return a; }

    public async Task DeleteAsync(int id)
    { var a = await _db.Articles.FindAsync(id); if (a != null) { _db.Articles.Remove(a); await _db.SaveChangesAsync(); } }

    public async Task IncrementViewsAsync(int id)
        => await _db.Articles.Where(a => a.Id == id).ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));
}

public class CategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Category>> GetAllAsync()
        => await _db.Categories.OrderBy(c => c.SortOrder).AsNoTracking().ToListAsync();

    public async Task<Category?> GetBySlugAsync(string slug)
        => await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<Category> CreateAsync(Category c)
    { _db.Categories.Add(c); await _db.SaveChangesAsync(); return c; }
}

public class SiteConfigRepository
{
    private readonly AppDbContext _db;
    public SiteConfigRepository(AppDbContext db) => _db = db;

    public async Task<Dictionary<string, string>> GetAllAsync()
        => await _db.SiteConfigs.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);

    public async Task SetValueAsync(string key, string value)
    {
        var c = await _db.SiteConfigs.FirstOrDefaultAsync(s => s.Key == key);
        if (c != null) c.Value = value;
        else _db.SiteConfigs.Add(new SiteConfig { Key = key, Value = value });
        await _db.SaveChangesAsync();
    }
}

public class NewsletterRepository
{
    private readonly AppDbContext _db;
    public NewsletterRepository(AppDbContext db) => _db = db;

    public async Task<bool> SubscribeAsync(string email)
    {
        if (await _db.Newsletters.AnyAsync(n => n.Email == email)) return false;
        _db.Newsletters.Add(new Newsletter { Email = email });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Newsletter>> GetAllAsync()
        => await _db.Newsletters.OrderByDescending(n => n.SubscribedAt).ToListAsync();

    public async Task<int> GetCountAsync()
        => await _db.Newsletters.CountAsync(n => n.Active);
}
