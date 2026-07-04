# VistosFácil — Guia de imigração em português

Stack: **ASP.NET Core 8** + **MySQL 8** + **EF Core** + **DigitalOcean VPS**

## Estrutura

```
VistosFacil/
├── src/
│   ├── VistosFacil.Core/           # Entidades (Article, Category, etc.)
│   ├── VistosFacil.Infrastructure/ # EF Core, repositórios, migrations
│   └── VistosFacil.Web/            # Controllers, Views, CSS, JS
```

## Deploy no servidor (mesmo VPS do ilhascaboverde.xyz)

### 1. Criar base de dados MySQL

```bash
mysql -u root -e "CREATE DATABASE vistosfacil CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -u root -e "GRANT ALL PRIVILEGES ON vistosfacil.* TO 'cvagora_user'@'localhost'; FLUSH PRIVILEGES;"
```

### 2. Clonar e configurar

```bash
cd /var/www && git clone https://github.com/SEU_UTILIZADOR/vistosfacil.git vistosfacil
cd /var/www/vistosfacil
```

Editar `src/VistosFacil.Web/appsettings.json` com a password correcta.

### 3. Aplicar migrations

```bash
export ConnectionStrings__Default="Server=localhost;Database=vistosfacil;User=cvagora_user;Password=SUA_PASSWORD;CharSet=utf8mb4;"
dotnet ef migrations add InitialCreate --project src/VistosFacil.Infrastructure/VistosFacil.Infrastructure.csproj --startup-project src/VistosFacil.Web/VistosFacil.Web.csproj
dotnet ef database update --project src/VistosFacil.Infrastructure/VistosFacil.Infrastructure.csproj --startup-project src/VistosFacil.Web/VistosFacil.Web.csproj
```

### 4. Compilar e publicar

```bash
dotnet publish src/VistosFacil.Web/VistosFacil.Web.csproj -c Release -o /var/www/vistosfacil-publish
```

### 5. Configurar Nginx (porta 5001)

```nginx
server {
    listen 80;
    server_name vistosfacil.com www.vistosfacil.com;
    location / {
        proxy_pass http://127.0.0.1:5001;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
ln -sf /etc/nginx/sites-available/vistosfacil /etc/nginx/sites-enabled/
nginx -t && systemctl reload nginx
```

### 6. Serviço systemd

```bash
cat > /etc/systemd/system/vistosfacil.service << 'EOF'
[Unit]
Description=VistosFacil ASP.NET Core
After=network.target mysql.service

[Service]
Type=simple
User=root
WorkingDirectory=/var/www/vistosfacil-publish
ExecStart=/usr/bin/dotnet /var/www/vistosfacil-publish/VistosFacil.Web.dll
Restart=always
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5001
Environment=ConnectionStrings__Default=Server=localhost;Database=vistosfacil;User=cvagora_user;Password=SUA_PASSWORD;CharSet=utf8mb4;
Environment=AdminCredentials__Username=admin
Environment=AdminCredentials__Password=SUA_PASSWORD_ADMIN

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload && systemctl enable vistosfacil && systemctl start vistosfacil
```

### 7. SSL

```bash
certbot --nginx -d vistosfacil.com -d www.vistosfacil.com --non-interactive --agree-tos -m SEU_EMAIL
```

### 8. Painel admin

```
https://vistosfacil.com/admin
```

## Artigos iniciais sugeridos

1. "Como obter visto americano sendo cabo-verdiano" (EUA)
2. "Visto D7 Portugal: guia completo para não-europeus" (Portugal)
3. "Autorização de Residência em Portugal: passo a passo" (Residência)
4. "Como obter a nacionalidade portuguesa" (Nacionalidade)
5. "Visto Schengen: como pedir e quais os documentos" (Schengen)
6. "Visto de turismo para o Brasil: requisitos 2026" (Brasil)

## Deploy rápido após alterações

```bash
cd /var/www/vistosfacil && git pull && dotnet publish src/VistosFacil.Web/VistosFacil.Web.csproj -c Release -o /var/www/vistosfacil-publish && systemctl restart vistosfacil
```
