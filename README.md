# VistosFácil V2 — Mega Menu + Design Profissional

## O que há de novo nesta versão

- **Mega menu** com submenus por Europa, EUA e Guias
- **Design profissional** com paleta azul/verde/dourado
- **Sidebar** nos artigos com guias relacionados e checklist
- **Filtro por categoria** na homepage com pills animadas
- **Breadcrumbs** em todas as páginas
- **Partilha social** (Facebook, WhatsApp, Twitter) nos artigos
- **Newsletter** na sidebar dos artigos
- **Animações** de entrada nos cards

## Deploy no servidor (substitui o vistosfacil anterior)

```bash
# 1. Clonar no servidor
cd /var/www && rm -rf vistosfacil && git clone https://github.com/SEU_USER/vistosfacilv2.git vistosfacil

# 2. Configurar appsettings.json
nano /var/www/vistosfacil/src/VistosFacil.Web/appsettings.json

# 3. Restore e migrations
cd /var/www/vistosfacil
dotnet restore src/VistosFacil.Core/VistosFacil.Core.csproj
dotnet restore src/VistosFacil.Infrastructure/VistosFacil.Infrastructure.csproj
dotnet restore src/VistosFacil.Web/VistosFacil.Web.csproj
export ConnectionStrings__Default="Server=localhost;Database=vistosfacil;User=cvagora_user;Password=CvAgora2026pass;CharSet=utf8mb4;"
dotnet ef migrations add InitialCreate --project src/VistosFacil.Infrastructure --startup-project src/VistosFacil.Web
dotnet ef database update --project src/VistosFacil.Infrastructure --startup-project src/VistosFacil.Web

# 4. Publicar
dotnet publish src/VistosFacil.Web/VistosFacil.Web.csproj -c Release -o /var/www/vistosfacil-publish

# 5. Reiniciar
systemctl restart vistosfacil
```
