using Microsoft.AspNetCore.Mvc;
using VistosFacil.Infrastructure.Repositories;
using VistosFacil.Web.ViewModels;
using VistosFacil.Core.Entities;

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
            TrendingTitle = configs.GetValueOrDefault("trending_title", "Guias mais procurados"),
            AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
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
        await _articles.IncrementViewsAsync(article.Id);
        var configs = await _config.GetAllAsync();
        return View(new ArticleViewModel
        {
            Article = article,
            AdSenseClient = configs.GetValueOrDefault("adsense_client", ""),
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

    [Route("europa/{pais}/{tipo}")]
    public IActionResult EuropaCategoria(string pais, string tipo)
    {
        var slugMap = new Dictionary<string,string>
        {
            {"portugal/residencia","autorizacao-residencia"},
            {"portugal/trabalho","vistos-portugal"},
            {"portugal/turismo","vistos-portugal"},
            {"portugal/estudo","vistos-portugal"},
            {"portugal/d7","vistos-portugal"},
            {"portugal/nomade-digital","vistos-portugal"},
            {"portugal/nacionalidade","nacionalidade"},
            {"schengen/turismo","vistos-schengen"},
            {"schengen/estudo","vistos-schengen"},
            {"schengen/trabalho","vistos-schengen"},
            {"reino-unido/turismo","vistos-schengen"},
            {"reino-unido/trabalho","vistos-schengen"},
        };
        var slug = slugMap.GetValueOrDefault($"{pais}/{tipo}", "vistos-portugal");
        return RedirectToAction("Category", new { slug });
    }

    [Route("eua/{tipo}")] public IActionResult EuaCategoria(string tipo) => RedirectToAction("Category", new { slug="vistos-eua" });
    [Route("europa")] public IActionResult Europa() => RedirectToAction("Category", new { slug="vistos-portugal" });
    [Route("eua")] public IActionResult Eua() => RedirectToAction("Category", new { slug="vistos-eua" });
    [Route("guias")] public IActionResult Guias() => RedirectToAction("Index");
    [Route("guias/{tipo}")] public IActionResult GuiasTipo(string tipo) => RedirectToAction("Index");

    [Route("blog")]
    public async Task<IActionResult> Blog(int page = 1)
    {
        var all = await _articles.GetPublishedAsync(page, 12);
        var total = await _articles.GetTotalCountAsync();
        var configs = await _config.GetAllAsync();
        var vm = new HomeViewModel
        {
            TrendingTitle = "Todos os artigos",
            AllArticles = all.ToList(),
            Categories = (await _categories.GetAllAsync()).ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / 12.0),
            GoogleAnalyticsId = configs.GetValueOrDefault("google_analytics_id", "")
        };
        return View("Index", vm);
    }

    [Route("checklist")] public IActionResult Checklist() => View();
    [Route("prazos")] public IActionResult Prazos() => View();
    [Route("calculadora-custos")] public IActionResult CalculadoraCustos() => View();

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

    [Route("sobre")] public IActionResult Sobre() => View();
    [Route("privacidade")] public IActionResult Privacidade() => View();
    [Route("termos")] public IActionResult Termos() => View();
    [Route("contacto")] public IActionResult Contacto() => View();
    [Route("erro")] public IActionResult Error() => View();
}
