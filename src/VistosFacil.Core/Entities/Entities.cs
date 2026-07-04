namespace VistosFacil.Core.Entities;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Body { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool Published { get; set; }
    public bool Featured { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string ColorClass { get; set; } = "c-azul";
    public string TagLabel { get; set; } = "";
    public string Emoji { get; set; } = "📄";
    public int SortOrder { get; set; }
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

public class SiteConfig
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Description { get; set; }
}

public class Newsletter
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}
