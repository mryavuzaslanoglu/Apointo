# Konfig�rasyon ve Gizli Bilgi Y�netimi

Bu dok�man, Mod�l 1 kapsam�ndaki servislerin sorunsuz �al��abilmesi i�in gerekli ba�lant� bilgilerini, kimlik do�rulama s�rlar�n� ve e-posta yap�land�rmalar�n� nas�l y�netece�inizi a��klar.

## 1. Genel Prensipler
- **Kaynak koda gizli bilgi yazmay�n.** T�m ba�lant� stringleri, JWT imza anahtarlar� ve SMTP kullan�c� bilgileri kullan�c� s�rlar� (user-secrets) veya ortam de�i�kenleri �zerinden y�netilmelidir.
- Proje varsay�lan `appsettings*.json` dosyalar�nda sadece _placeholder_ de�erler b�rak�r. Uygulama �al���rken bu ayarlar� a�a��daki �ncelik s�ras�yla okur:
  1. Ortam de�i�kenleri (`Environment Variables`)
  2. Geli�tirme ortam�nda `dotnet user-secrets`
  3. `appsettings.{Environment}.json`

## 2. Geli�tirme Ortam� (dotnet user-secrets)

Geli�tirici makinesinde a�a��daki ad�mlar� izleyin:

```powershell
cd C:\Users\Hakan\Project\Apointo

dotnet user-secrets init --project src/Apointo.Api/Apointo.Api.csproj

# Veritaban� ba�lant�s� (SQL Server �rne�i)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Apointo;User Id=apointo_dev;Password=ChangeMe!123;TrustServerCertificate=True" --project src/Apointo.Api

# JWT
.dotnet user-secrets set "Jwt:SigningKey" "�ok-uzun-ve-rastgele-bir-secret" --project src/Apointo.Api
# Opsiyonel: issuer/audience base ayarlar�
# dotnet user-secrets set "Jwt:Issuer" "Apointo" --project src/Apointo.Api
# dotnet user-secrets set "Jwt:Audience" "ApointoClients" --project src/Apointo.Api

# E-posta (SMTP) ayarlar�
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

> **Not:** Email g�nderimi devre d��� kalacaksa `EmailSettings:IsEnabled` de�erini `false` yapabilirsiniz. Bu durumda �ifre s�f�rlama u�lar� yine �al���r fakat mail g�nderme denemez.

## 3. Docker Compose / Lokal Konteyner

1. `.env.docker.example` dosyas�n� kopyalayarak `.env.docker` olu�turun:
   ```powershell
   Copy-Item .env.docker.example .env.docker
   ```
2. Dosyadaki t�m placeholder de�erleri ger�ek bilgilerle de�i�tirin.
3. Servisleri ba�latmak i�in:
   ```powershell
   docker-compose up --build
   ```
4. API `http://localhost:8080` �zerinde, Seq log izleme arac� `http://localhost:5341` �zerinde �al���r.

## 4. Production / CI Ortam�

Prod veya CI pipeline��nda a�a��daki ortam de�i�kenlerini tan�mlay�n:

| Ayar | Ortam De�i�keni | �rnek |
| --- | --- | --- |
| Default ba�lant� | `ConnectionStrings__DefaultConnection` | `Server=sql-prod;Database=Apointo;...` |
| JWT gizli anahtar | `Jwt__SigningKey` | `Prod-Secret-Key` |
| Email Enabled | `EmailSettings__IsEnabled` | `true` |
| SMTP kullan�c� ad� | `EmailSettings__Username` | `smtp-prod-user` |
| SMTP �ifre | `EmailSettings__Password` | `smtp-prod-pass` |
| �ifre s�f�rlama y�nlendirme | `EmailSettings__PasswordResetUrl` | `https://app.apointo.com` |
| Seed admin e-posta | `Seed__Admin__Email` | `admin@apointo.com` |
| Seed admin �ifre | `Seed__Admin__Password` | `Bir#Prod1` |

> Ortam de�i�keni isimlendirmesinde �ift alt �izgi (`__`) hiyerar�iyi temsil eder.

## 5. Seed Admin Mant���
- Uygulama ilk a��l��ta `ApplicationDbContextInitializer` �al��t�r�r.
- `Seed:Admin:Email` ve `Seed:Admin:Password` sa�lanm��sa, e�er ayn� e-posta ile bir kullan�c� yoksa belirtilen �ifreyle **Admin** rol�nde kullan�c� olu�turur.
- Bu bilgileri sadece g�venli ortamlarda set edin. Geli�tirmede farkl�, prod�da farkl� secret kullan�n.

## 6. Email G�nderimi Hakk�nda
- `EmailSettings:IsEnabled = false` ise `ForgotPassword` u�lar� 503 d�ner ve mail g�nderimi denemez.
- `PasswordResetUrl` bo�sa backend `PasswordResetUrlNotConfigured` hatas� d�ner. Frontend/mobil taraf�n�n �ifre s�f�rlama ekran�n� bar�nd�rd��� URL�yi bu alana yaz�n veya API iste�inde `clientBaseUrl` parametresini g�nderin.

## 7. H�zl� Kontrol Listesi
- [ ] `dotnet user-secrets` veya ortam de�i�kenleri tan�mland� m�?
- [ ] JWT `SigningKey` g��l� bir de�er mi?
- [ ] Email g�nderimi aktive edilecekse SMTP bilgileri tam m�?
- [ ] Seed admin sadece gerekli ortamlarda m� aktif?
- [ ] Docker i�in `.env.docker` haz�r m�?

Sorular veya g�ncelleme ihtiya�lar� i�in bu dosyay� referans al�n.