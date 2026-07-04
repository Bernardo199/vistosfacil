using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VistosFacil.Core.Entities;
using VistosFacil.Infrastructure.Repositories;

namespace VistosFacil.Web.Controllers;

[Authorize]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ArticleRepository _articles;
    private readonly CategoryRepository _categories;
    private readonly SiteConfigRepository _config;
    private readonly NewsletterRepository _newsletter;
    private readonly IConfiguration _appConfig;

    public AdminController(ArticleRepository articles, CategoryRepository categories,
        SiteConfigRepository config, NewsletterRepository newsletter, IConfiguration appConfig)
    {
        _articles = articles; _categories = categories;
        _config = config; _newsletter = newsletter; _appConfig = appConfig;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalArticles = await _articles.GetTotalCountAsync(false);
        ViewBag.PublishedArticles = await _articles.GetTotalCountAsync(true);
        ViewBag.NewsletterCount = await _newsletter.GetCountAsync();
        ViewBag.RecentArticles = (await _articles.GetAllAsync()).Take(10).ToList();
        return View();
    }

    [AllowAnonymous, HttpGet("login")]
    public IActionResult Login() => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index") : View();

    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var validUser = _appConfig["AdminCredentials:Username"] ?? "admin";
        var validPass = _appConfig["AdminCredentials:Password"] ?? "admin";

        if (username != validUser || password != validPass)
        { ViewBag.Error = "Credenciais inválidas."; return View(); }

        var claims = new List<Claim> {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return Redirect("/admin");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    { await HttpContext.SignOutAsync(); return RedirectToAction("Login"); }

    [HttpGet("artigos")]
    public async Task<IActionResult> Articles() => View(await _articles.GetAllAsync());

    [HttpGet("artigos/novo")]
    public async Task<IActionResult> ArticleCreate()
    { ViewBag.Categories = await _categories.GetAllAsync(); return View(new Article()); }

    [HttpPost("artigos/novo")]
    public async Task<IActionResult> ArticleCreate(Article model)
    {
        ModelState.Remove("Slug"); ModelState.Remove("Category");
        ModelState.Remove("MetaTitle"); ModelState.Remove("MetaDescription"); ModelState.Remove("ImageUrl");
        if (!ModelState.IsValid) { ViewBag.Categories = await _categories.GetAllAsync(); return View(model); }
        model.Slug = GenerateSlug(model.Title);
        model.CreatedAt = model.UpdatedAt = DateTime.UtcNow;
        await _articles.CreateAsync(model);
        TempData["Success"] = "Artigo criado com sucesso.";
        return RedirectToAction("Articles");
    }

    [HttpGet("artigos/{id:int}/editar")]
    public async Task<IActionResult> ArticleEdit(int id)
    {
        var all = await _articles.GetAllAsync();
        var article = all.FirstOrDefault(a => a.Id == id);
        if (article == null) return NotFound();
        ViewBag.Categories = await _categories.GetAllAsync();
        return View(article);
    }

    [HttpPost("artigos/{id:int}/editar")]
    public async Task<IActionResult> ArticleEdit(int id, Article model)
    {
        ModelState.Remove("Slug"); ModelState.Remove("Category");
        ModelState.Remove("MetaTitle"); ModelState.Remove("MetaDescription"); ModelState.Remove("ImageUrl");
        if (!ModelState.IsValid) { ViewBag.Categories = await _categories.GetAllAsync(); return View(model); }
        model.Id = id;
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = GenerateSlug(model.Title);
        await _articles.UpdateAsync(model);
        TempData["Success"] = "Artigo actualizado.";
        return RedirectToAction("Articles");
    }

    [HttpPost("artigos/{id:int}/apagar")]
    public async Task<IActionResult> ArticleDelete(int id)
    { await _articles.DeleteAsync(id); TempData["Success"] = "Artigo eliminado."; return RedirectToAction("Articles"); }

    [HttpGet("configuracoes")]
    public async Task<IActionResult> Settings() => View(await _config.GetAllAsync());

    [HttpPost("configuracoes")]
    public async Task<IActionResult> Settings(Dictionary<string, string> configs)
    {
        foreach (var kv in configs) await _config.SetValueAsync(kv.Key, kv.Value ?? "");
        TempData["Success"] = "Configurações guardadas.";
        return RedirectToAction("Settings");
    }

    [HttpGet("newsletter")]
    public async Task<IActionResult> Newsletter() => View(await _newsletter.GetAllAsync());

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace("á","a").Replace("à","a").Replace("ã","a").Replace("â","a")
            .Replace("é","e").Replace("ê","e").Replace("í","i").Replace("ó","o")
            .Replace("ô","o").Replace("õ","o").Replace("ú","u").Replace("ç","c");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
        return slug.Length > 80 ? slug[..80] : slug;
    }
}
