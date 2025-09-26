# Konfigürasyon ve Gizli Bilgi Yönetimi

Bu doküman, Modül 1 kapsamýndaki servislerin sorunsuz çalýþabilmesi için gerekli baðlantý bilgilerini, kimlik doðrulama sýrlarýný ve e-posta yapýlandýrmalarýný nasýl yöneteceðinizi açýklar.

## 1. Genel Prensipler
- **Kaynak koda gizli bilgi yazmayýn.** Tüm baðlantý stringleri, JWT imza anahtarlarý ve SMTP kullanýcý bilgileri kullanýcý sýrlarý (user-secrets) veya ortam deðiþkenleri üzerinden yönetilmelidir.
- Proje varsayýlan `appsettings*.json` dosyalarýnda sadece _placeholder_ deðerler býrakýr. Uygulama çalýþýrken bu ayarlarý aþaðýdaki öncelik sýrasýyla okur:
  1. Ortam deðiþkenleri (`Environment Variables`)
  2. Geliþtirme ortamýnda `dotnet user-secrets`
  3. `appsettings.{Environment}.json`

## 2. Geliþtirme Ortamý (dotnet user-secrets)

Geliþtirici makinesinde aþaðýdaki adýmlarý izleyin:

```powershell
cd C:\Users\Hakan\Project\Apointo

dotnet user-secrets init --project src/Apointo.Api/Apointo.Api.csproj

# Veritabaný baðlantýsý (SQL Server örneði)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Apointo;User Id=apointo_dev;Password=ChangeMe!123;TrustServerCertificate=True" --project src/Apointo.Api

# JWT
.dotnet user-secrets set "Jwt:SigningKey" "çok-uzun-ve-rastgele-bir-secret" --project src/Apointo.Api
# Opsiyonel: issuer/audience base ayarlarý
# dotnet user-secrets set "Jwt:Issuer" "Apointo" --project src/Apointo.Api
# dotnet user-secrets set "Jwt:Audience" "ApointoClients" --project src/Apointo.Api

# E-posta (SMTP) ayarlarý
.dotnet user-secrets set "EmailSettings:IsEnabled" "true" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:FromName" "Apointo" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:FromEmail" "noreply@apointo.local" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:SmtpHost" "smtp.sunucunuz.com" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:SmtpPort" "587" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:UseSsl" "true" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:Username" "smtp-user" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:Password" "smtp-password" --project src/Apointo.Api
.dotnet user-secrets set "EmailSettings:PasswordResetUrl" "https://localhost:5173" --project src/Apointo.Api

# Seed Admin (opsiyonel)
dotnet user-secrets set "Seed:Admin:Email" "admin@apointo.local" --project src/Apointo.Api
.dotnet user-secrets set "Seed:Admin:Password" "ChangeMe!123" --project src/Apointo.Api
.dotnet user-secrets set "Seed:Admin:FirstName" "System" --project src/Apointo.Api
.dotnet user-secrets set "Seed:Admin:LastName" "Administrator" --project src/Apointo.Api
```

> **Not:** Email gönderimi devre dýþý kalacaksa `EmailSettings:IsEnabled` deðerini `false` yapabilirsiniz. Bu durumda þifre sýfýrlama uçlarý yine çalýþýr fakat mail gönderme denemez.

## 3. Docker Compose / Lokal Konteyner

1. `.env.docker.example` dosyasýný kopyalayarak `.env.docker` oluþturun:
   ```powershell
   Copy-Item .env.docker.example .env.docker
   ```
2. Dosyadaki tüm placeholder deðerleri gerçek bilgilerle deðiþtirin.
3. Servisleri baþlatmak için:
   ```powershell
   docker-compose up --build
   ```
4. API `http://localhost:8080` üzerinde, Seq log izleme aracý `http://localhost:5341` üzerinde çalýþýr.

## 4. Production / CI Ortamý

Prod veya CI pipeline’ýnda aþaðýdaki ortam deðiþkenlerini tanýmlayýn:

| Ayar | Ortam Deðiþkeni | Örnek |
| --- | --- | --- |
| Default baðlantý | `ConnectionStrings__DefaultConnection` | `Server=sql-prod;Database=Apointo;...` |
| JWT gizli anahtar | `Jwt__SigningKey` | `Prod-Secret-Key` |
| Email Enabled | `EmailSettings__IsEnabled` | `true` |
| SMTP kullanýcý adý | `EmailSettings__Username` | `smtp-prod-user` |
| SMTP þifre | `EmailSettings__Password` | `smtp-prod-pass` |
| Þifre sýfýrlama yönlendirme | `EmailSettings__PasswordResetUrl` | `https://app.apointo.com` |
| Seed admin e-posta | `Seed__Admin__Email` | `admin@apointo.com` |
| Seed admin þifre | `Seed__Admin__Password` | `Bir#Prod1` |

> Ortam deðiþkeni isimlendirmesinde çift alt çizgi (`__`) hiyerarþiyi temsil eder.

## 5. Seed Admin Mantýðý
- Uygulama ilk açýlýþta `ApplicationDbContextInitializer` çalýþtýrýr.
- `Seed:Admin:Email` ve `Seed:Admin:Password` saðlanmýþsa, eðer ayný e-posta ile bir kullanýcý yoksa belirtilen þifreyle **Admin** rolünde kullanýcý oluþturur.
- Bu bilgileri sadece güvenli ortamlarda set edin. Geliþtirmede farklý, prod’da farklý secret kullanýn.

## 6. Email Gönderimi Hakkýnda
- `EmailSettings:IsEnabled = false` ise `ForgotPassword` uçlarý 503 döner ve mail gönderimi denemez.
- `PasswordResetUrl` boþsa backend `PasswordResetUrlNotConfigured` hatasý döner. Frontend/mobil tarafýnýn þifre sýfýrlama ekranýný barýndýrdýðý URL’yi bu alana yazýn veya API isteðinde `clientBaseUrl` parametresini gönderin.

## 7. Hýzlý Kontrol Listesi
- [ ] `dotnet user-secrets` veya ortam deðiþkenleri tanýmlandý mý?
- [ ] JWT `SigningKey` güçlü bir deðer mi?
- [ ] Email gönderimi aktive edilecekse SMTP bilgileri tam mý?
- [ ] Seed admin sadece gerekli ortamlarda mý aktif?
- [ ] Docker için `.env.docker` hazýr mý?

Sorular veya güncelleme ihtiyaçlarý için bu dosyayý referans alýn.