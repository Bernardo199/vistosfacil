using Microsoft.AspNetCore.Mvc;
using VistosFacil.Infrastructure.Repositories;
using VistosFacil.Web.ViewModels;

namespace VistosFacil.Web.Controllers;

public class HomeController : Controller
{
    private readonly ArticleRepository _articles;
    private readonly CategoryRepository _categories;
    private readonly SiteConfigRepository _config;
    private readonly NewsletterRepository _newsletter;

    public HomeController(ArticleRepository articles, CategoryRepository categories,
        SiteConfigRepository config, NewsletterRepository newsletter)
    {
        _articles = articles; _categories = categories;
        _config = config; _newsletter = newsletter;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var configs = await _config.GetAllAsync();
        var featured = await _articles.GetFeaturedAsync(3);
        var all = await _articles.GetPublishedAsync(page, 9);
        var total = await _articles.GetTotalCountAsync();
        var categories = await _categories.GetAllAsync();

        var vm = new HomeViewModel
        {
            HeroTitle = configs.GetValueOrDefault("hero_title", "O seu guia de\nimigração em português"),
            HeroSubtitle = configs.GetValueOrDefault("hero_subtitle", "Tudo sobre vistos, autorizações de residência e nacionalidade."),
            TrendingTitle = configs.GetValueOrDefault("trending_title", "Guias mais procurados"),
            AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
            AdSenseSlot1 = configs.GetValueOrDefault("adsense_slot_1", ""),
            GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", ""),
            FeaturedArticles = featured.ToList(),
            AllArticles = all.ToList(),
            Categories = categories.ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / 9.0)
        };

        return View(vm);
    }

    public async Task<IActionResult> Article(string slug)
    {
        var article = await _articles.GetBySlugAsync(slug);
        if (article == null) return NotFound();
        _ = _articles.IncrementViewsAsync(article.Id);
        var configs = await _config.GetAllAsync();
        return View(new ArticleViewModel
        {
            Article = article,
            AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
            AdSenseSlot1 = configs.GetValueOrDefault("adsense_slot_1", ""),
            GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", "")
        });
    }

    public async Task<IActionResult> Category(string slug, int page = 1)
    {
        var category = await _categories.GetBySlugAsync(slug);
        if (category == null) return NotFound();
        var articles = await _articles.GetByCategoryAsync(slug, page);
        var configs = await _config.GetAllAsync();
        return View(new CategoryViewModel
        {
            Category = category,
            Articles = articles.ToList(),
            CurrentPage = page,
            GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", "")
        });
    }

    [Route("pesquisa")]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return View(new SearchViewModel());
        var results = await _articles.SearchAsync(q);
        return View(new SearchViewModel { Query = q, Results = results.ToList() });
    }

    [HttpPost, Route("subscribe")]
    public async Task<IActionResult> Subscribe(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Json(new { success = false, message = "Email inválido." });
        var ok = await _newsletter.SubscribeAsync(email.Trim().ToLower());
        return Json(new { success = ok, message = ok ? "Subscrito com sucesso!" : "Email já registado." });
    }

    [Route("privacidade")]
    public IActionResult Privacidade() => View();

    [Route("termos")]
    public IActionResult Termos() => View();

    [Route("contacto")]
    public IActionResult Contacto() => View();

    [Route("erro")]
    public IActionResult Error() => View();
}
