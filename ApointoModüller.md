Anladım, talebin çok net. Projenin her bir parçasını, gereksinimlerini ve teknik detaylarını içeren, "matruşka bebek" gibi iç içe geçmiş bir modüler yapı istiyorsun. Bu, projenin en küçük detayına kadar planlanmasını sağlar ve geliştirme ekibi için adeta bir anayasa niteliği taşır.

İşte bu detay seviyesinde hazırlanmış, ana modüller ve alt modüllerden oluşan, her birinin fonksiyonel ve teknik gereksinimlerini tanımlayan kapsamlı **Modüler Sistem Tasarım Dokümanı**.

---

### **Kuaför & Berber Randevu Sistemi - Detaylı Modüler Tasarım Dokümanı**

**Genel Felsefe:** Proje, dikey olarak dilimlenmiş 6 ana modülden oluşacaktır. Her ana modül, kendi içinde tamamlanması gereken alt modüllere (fonksiyonel birimlere) ayrılmıştır. Her alt modül için Backend (API), Frontend (Web), ve Mobil (RN) görevleri ayrı ayrı tanımlanmıştır.

---

### **Ana Modül 1: Çekirdek Sistem ve Kimlik Yönetimi (Foundation & Identity)**

**Amaç:** Sistemin çalışması için gerekli olan en temel altyapıyı, konfigürasyonları ve tüm kullanıcı tiplerinin sisteme güvenli erişimini sağlayan mekanizmaları kurmak. Bu modül diğer her şeyin temelidir.

#### **Alt Modül 1.1: Proje Kurulumu ve Altyapı**
*   **Gereksinimler:** Proje, Clean Architecture prensiplerine uygun, ölçeklenebilir ve bakımı kolay bir temel üzerine inşa edilmelidir. Tüm ortamlar (geliştirme, test, production) için tutarlı bir yapı sağlanmalıdır.
*   **Backend Görevleri:**
    1.  .NET 9 Solution'ı ve katmanlı projeleri (Core.Domain, Core.Application, Infrastructure, Presentation.API) oluşturmak.
    2.  Merkezi Konfigürasyon Yönetimi: `appsettings.json` dosyalarını ve ortam değişkenlerini (environment variables) okuyacak yapıyı kurmak.
    3.  Dependency Injection (DI) konteynerini yapılandırmak.
    4.  Global Hata Yönetimi (Exception Handling Middleware) eklemek.
    5.  Loglama Altyapısı (Serilog) entegrasyonu.
    6.  Dockerizasyon: Backend ve Frontend için multi-stage `Dockerfile` ve geliştirme ortamı için `docker-compose.yml` dosyalarını hazırlamak.
*   **Frontend/Mobil Görevleri:**
    1.  React (Vite, TS) ve React Native (Expo/CLI, TS) projelerini oluşturmak.
    2.  Klasör Yapısı: `components`, `pages/screens`, `services/api`, `hooks`, `store/state` gibi standartlaştırılmış klasör yapılarını oluşturmak.
    3.  UI Kütüphanesi Entegrasyonu (opsiyonel): Ant Design, Material-UI (Web) veya React Native Paper (Mobil) gibi bir kütüphanenin temel entegrasyonu.
    4.  Merkezi API İstemcisi: Axios instance oluşturarak base URL, interceptor'lar (token ekleme, hata yakalama) gibi ortak yapılandırmaları yapmak.

#### **Alt Modül 1.2: Kullanıcı Kaydı ve Giriş (Authentication)**
*   **Gereksinimler:** Müşterilerin sisteme e-posta/şifre ile veya sosyal medya hesapları ile kaydolabilmesi ve giriş yapabilmesi. Şifrelerin güvenli bir şekilde saklanması.
*   **Backend Görevleri:**
    1.  `User` ve `Role` entity'lerini tasarlamak. Parola alanı hash'lenerek saklanmalıdır (BCrypt).
    2.  Entity Framework Core ile veritabanı tablolarını (`Users`, `Roles`) oluşturmak.
    3.  API Endpoints:
        *   `POST /api/auth/register`: Yeni müşteri kaydı için. Girdi validasyonu yapılmalı (geçerli e-posta, minimum şifre uzunluğu).
        *   `POST /api/auth/login`: Kullanıcı adı ve şifre doğrulaması sonrası JWT (Access Token & Refresh Token) üreten endpoint.
        *   `POST /api/auth/refresh-token`: Süresi dolmuş access token'ı yenilemek için.
    4.  Şifre Sıfırlama Mekanizması: E-posta ile token gönderimi ve şifre güncelleme endpoint'leri.
*   **Frontend/Mobil Görevleri:**
    1.  Ekranlar/Sayfalar: `Giriş Yap`, `Kayıt Ol`, `Şifremi Unuttum`, `Yeni Şifre Belirle`.
    2.  Formlar ve Validasyon: Formik/Yup veya React Hook Form ile kullanıcı girdilerini (e-posta formatı, şifre eşleşmesi) anlık olarak doğrulamak.
    3.  State Yönetimi: Kullanıcı oturum bilgilerini (token, kullanıcı rolü, isim) global state'e (Redux/Zustand) kaydetmek ve uygulama genelinde erişilebilir kılmak.
    4.  Kalıcı Depolama: Token'ları `localStorage` (Web) ve `AsyncStorage` (Mobil) üzerinde güvenli bir şekilde saklamak.
    5.  Korumalı Rotalar (Protected Routes): Sadece giriş yapmış kullanıcıların erişebileceği sayfaları (örn: Profilim) yöneten bir yapı kurmak.

---

### **Ana Modül 2: İşletme Varlık Yönetimi (Business Entity Management)**

**Amaç:** İşletme sahibinin, sistemin çalışması için gerekli olan temel varlıkları (personel, hizmetler, çalışma saatleri) yönetebileceği arayüzleri ve altyapıyı sunmak. Bu modül tamamen **yönetici paneli** odaklıdır.

#### **Alt Modül 2.1: İşletme ve Personel Yönetimi**
*   **Gereksinimler:** İşletme sahibi, işletme bilgilerini düzenleyebilmeli, sisteme sınırsız sayıda personel ekleyip düzenleyebilmeli ve her personelin çalışma takvimini özelleştirebilmelidir.
*   **Backend Görevleri:**
    1.  Entity'ler: `Business`, `Staff`, `StaffSchedule` (personelin haftalık standart çalışma planı), `StaffAvailabilityOverride` (izinler, ek mesailer gibi standart dışı durumlar).
    2.  API Endpoints (Admin yetkisi gerektirir):
        *   `GET/PUT /api/business/settings`: İşletme genel bilgilerini (adres, telefon, genel çalışma saatleri) yönetmek için.
        *   `GET/POST/PUT/DELETE /api/staff`: Personel CRUD işlemleri.
        *   `GET/PUT /api/staff/{staffId}/schedule`: Belirli bir personelin haftalık çalışma takvimini yönetmek için.
        *   `POST/DELETE /api/staff/{staffId}/availability-override`: Belirli bir personele izin veya ek mesai günü eklemek/silmek için.
*   **Frontend (Yönetim Paneli) Görevleri:**
    1.  Sayfalar: `İşletme Ayarları`, `Personel Listesi`, `Personel Ekle/Düzenle`.
    2.  `Personel Ekle/Düzenle` sayfasında, o personele özel haftalık çalışma saatlerini (Pzt 09:00-18:00, Salı kapalı vb.) ayarlayabilen bir arayüz (checkbox'lar ve saat seçiciler ile).
    3.  Personele özel takvim üzerinde izin günlerini işaretleyebilme özelliği.

#### **Alt Modül 2.2: Hizmet ve Kategori Yönetimi**
*   **Gereksinimler:** İşletme, sunduğu hizmetleri kategoriler altında toplayabilmeli, her hizmetin süresini, fiyatını, hazırlık süresini (tampon) ve hangi personel tarafından verilebileceğini tanımlayabilmelidir.
*   **Backend Görevleri:**
    1.  Entity'ler: `ServiceCategory`, `Service`, `StaffService` (personel ve hizmet arası çoka-çok ilişki tablosu). `Service` entity'si `DurationInMinutes`, `BufferTimeInMinutes`, `Price` gibi alanlar içermelidir.
    2.  API Endpoints (Admin yetkisi gerektirir):
        *   `GET/POST/PUT/DELETE /api/services`: Hizmet CRUD işlemleri.
        *   `GET/POST/PUT/DELETE /api/service-categories`: Kategori CRUD işlemleri.
    3.  İş Mantığı: Bir hizmet oluşturulurken/güncellenirken, bu hizmeti hangi personellerin verebileceğini atayan bir mekanizma.
*   **Frontend (Yönetim Paneli) Görevleri:**
    1.  Sayfalar: `Hizmetler Listesi`, `Hizmet Ekle/Düzenle`.
    2.  `Hizmet Ekle/Düzenle` sayfasında, hizmetin adını, fiyatını, süresini vb. alanların yanı sıra, sistemdeki personellerin listelendiği ve bu hizmeti verebilecek olanların seçilebildiği bir çoklu seçim (multi-select) alanı.

---

### **Ana Modül 3: Randevu Yaşam Döngüsü (Appointment Lifecycle)**

**Amaç:** Müşterinin randevu oluşturmasından, işletmenin bu randevuyu yönetmesine kadar olan tüm süreci kapsayan, sistemin kalbi olan modül.

#### **Alt Modül 3.1: Müşteri Randevu Oluşturma Akışı**
*   **Gereksinimler:** Müşteri, adım adım bir sihirbaz (wizard) aracılığıyla, istediği hizmetleri ve personeli seçerek, sistemin sunduğu müsait zaman dilimlerinden birini rezerve edebilmelidir.
*   **Backend Görevleri:**
    1.  Entity: `Appointment` (CustomerID, StaffID, StartTime, EndTime, Status, vb.), `AppointmentService` (randevuda alınan hizmetler için çoka-çok).
    2.  **En Kritik API Endpoint:** `POST /api/slots/find-available`: Gönderilen hizmet(ler), personel tercihi ve tarih aralığına göre müsait saatleri hesaplayan karmaşık bir algoritma. Bu algoritma şunları dikkate almalıdır:
        *   İşletmenin genel çalışma saatleri.
        *   Seçilen personelin özel çalışma takvimi ve izinleri.
        *   Personelin mevcut diğer randevuları.
        *   Seçilen hizmet(ler)in toplam süresi + tampon süreler.
    3.  API Endpoint: `POST /api/appointments`: Müşterinin seçtiği slota randevu oluşturmak için.
*   **Frontend/Mobil Görevleri:**
    1.  Ekran/Sayfa: `Randevu Oluştur` (Multi-step form/wizard).
        *   **Adım 1: Hizmet Seçimi:** Kategorize edilmiş hizmet listesinden bir veya daha fazla hizmet seçimi.
        *   **Adım 2: Personel Seçimi:** Seçilen hizmet(ler)i verebilen personellerin listesi ve "Farketmez / İlk Müsait" seçeneği.
        *   **Adım 3: Tarih ve Saat Seçimi:** `find-available-slots` API'sinden gelen yanıta göre doldurulan interaktif bir takvim ve saat listesi.
        *   **Adım 4: Onay:** Randevu özetinin gösterildiği ve onayın alındığı son adım.

#### **Alt Modül 3.2: Randevu Yönetimi (İşletme & Müşteri)**
*   **Gereksinimler:** Müşteriler kendi randevularını görüntüleyip iptal edebilmeli. İşletme personeli ise tüm randevuları görsel bir takvim üzerinde görüp yönetebilmelidir.
*   **Backend Görevleri:**
    1.  API Endpoints:
        *   `GET /api/appointments/my`: Müşterinin kendi randevularını listelemesi için.
        *   `PUT /api/appointments/{id}/cancel`: Müşterinin randevusunu iptal etmesi için.
        *   `GET /api/appointments/calendar`: Yönetim panelindeki takvimi doldurmak için belirli bir tarih aralığındaki tüm randevuları getiren endpoint.
        *   `PUT /api/appointments/{id}`: Yöneticinin randevu detaylarını (örn: sürükle-bırak ile saatini/personelini) güncellemesi için.
*   **Frontend (Yönetim Paneli) Görevleri:**
    1.  Sayfa: `Takvim`. FullCalendar.io gibi bir kütüphane ile entegrasyon.
    2.  Özellikler: Günlük/haftalık/aylık görünüm. Personel bazlı filtreleme. Randevuya tıklayınca detayları gösteren modal. Sürükle-bırak ile randevu taşıma. Takvimdeki boş bir slota tıklayarak manuel randevu ekleme.
*   **Frontend/Mobil (Müşteri) Görevleri:**
    1.  Ekran/Sayfa: `Randevularım`. Yaklaşan ve geçmiş randevuları ayrı sekmelerde listeleyen arayüz.
    2.  İptal etme butonu ve onay mekanizması.

---

### **Ana Modül 4: İletişim ve Bildirimler (Communication)**

**Amaç:** Sistem ile kullanıcılar arasında (özellikle randevu hatırlatmaları gibi kritik anlarda) proaktif ve otomatik bir iletişim kurmak.

#### **Alt Modül 4.1: Bildirim Altyapısı ve Şablonlar**
*   **Gereksinimler:** E-posta, SMS ve Push Notification kanallarını destekleyen, merkezi olarak yönetilebilen bir bildirim sistemi kurulmalıdır.
*   **Backend Görevleri:**
    1.  Soyutlama: `INotificationService` arayüzü ve kanallara özel implementasyonları (örn: `MailKitEmailService`, `TwilioSmsService`, `FcmPushNotificationService`).
    2.  Şablon Yönetimi: E-posta ve SMS içerikleri için veritabanında veya dosya sisteminde tutulan, dinamik verilerle (müşteri adı, randevu saati) doldurulabilen şablonlar.
    3.  Event-Driven Tetikleme: MediatR kütüphanesinin `INotification` özelliğini kullanarak, "Randevu Oluşturuldu", "Randevu İptal Edildi" gibi domain olaylarından sonra bildirim servislerini tetiklemek.
*   **Frontend/Mobil Görevleri:**
    1.  Mobil tarafta Firebase SDK entegrasyonu ve kullanıcıdan bildirim izni isteme.
    2.  Cihazın FCM token'ını alıp `POST /api/users/my/device-token` gibi bir endpoint ile backend'e kaydetme.

#### **Alt Modül 4.2: Otomatik Hatırlatmalar**
*   **Gereksinimler:** Randevuya gelmeme oranını düşürmek için randevudan belirli süreler önce (örn: 24 saat ve 2 saat) müşterilere otomatik hatırlatma gönderilmelidir.
*   **Backend Görevleri:**
    1.  Zamanlanmış Görev (Scheduled Job): .NET'in `BackgroundService`'ini veya Hangfire/Quartz.NET gibi daha gelişmiş bir kütüphaneyi kullanarak, periyodik olarak (örn: her 15 dakikada bir) veritabanını tarayan ve hatırlatma zamanı gelmiş randevular için bildirim gönderen bir servis yazmak.
*   **Frontend/Mobil Görevleri:**
    1.  Gelen push notification'ları yakalayıp kullanıcıya cihazın standart bildirim arayüzünde göstermek.

---

### **Ana Modül 5: Müşteri Etkileşimi ve Sadakat (Engagement & Loyalty)**

**Amaç:** Müşteri geri bildirimlerini toplamak, sosyal kanıt oluşturmak ve tekrar gelen müşterileri ödüllendirerek müşteri bağlılığını artırmak.

#### **Alt Modül 5.1: Puanlama ve Yorum Sistemi**
*   **Gereksinimler:** Müşteriler, tamamlanan hizmetler sonrası hem işletmeyi hem de hizmeti aldıkları personeli puanlayıp yorum yapabilmelidir. Bu yorumlar, yöneticinin onayından sonra diğer müşterilere gösterilebilir olmalıdır.
*   **Backend Görevleri:**
    1.  Entity: `Review` (AppointmentID, StaffID, Rating, Comment, Status: Pending/Approved).
    2.  API Endpoints:
        *   `POST /api/reviews`: Yorum ve puan göndermek için. Sadece randevusu tamamlanmış kullanıcılar yapabilmelidir.
        *   `GET /api/staff/{staffId}/reviews`: Bir personelin onaylanmış yorumlarını listelemek için.
        *   `GET/PUT /api/admin/reviews`: Yöneticinin bekleyen yorumları listeleyip onaylaması/reddetmesi için.
*   **Frontend/Mobil (Müşteri) Görevleri:**
    1.  `Randevularım` sayfasındaki geçmiş randevuların yanında "Değerlendir" butonu.
    2.  Personel ve hizmet detay sayfalarında diğer müşterilerin yorum ve puanlarını gösteren bir bölüm.

#### **Alt Modül 5.2: Sadakat Programı**
*   **Gereksinimler:** Basit bir puan tabanlı sadakat sistemi. Müşteriler tamamlanan her randevudan veya harcadıkları tutara göre puan kazanmalı ve bu puanları indirim olarak kullanabilmelidir.
*   **Backend Görevleri:**
    1.  Entity: `LoyaltyPointsTransaction` (UserID, Points, TransactionType: Earn/Redeem, Description).
    2.  İş Mantığı: Randevu tamamlandığında otomatik olarak puan ekleyen, indirim kullanıldığında puan düşen servisler.
    3.  API Endpoints:
        *   `GET /api/users/my/loyalty-points`: Müşterinin mevcut puan bakiyesini ve işlem geçmişini getirmek için.
*   **Frontend/Mobil (Müşteri) Görevleri:**
    1.  Profil/Hesabım ekranında mevcut sadakat puanlarını ve işlem geçmişini gösteren bir bölüm.

---

### **Ana Modül 6: Raporlama ve Analitik (Business Intelligence)**

**Amaç:** İşletme sahibine, işiyle ilgili veriye dayalı kararlar alabilmesi için anlamlı ve görsel raporlar sunmak.

#### **Alt Modül 6.1: Finansal ve Operasyonel Raporlar**
*   **Gereksinimler:** İşletme sahibi, belirli tarih aralıklarına göre ciro, randevu sayısı, personel performansı ve en popüler hizmetler gibi metrikleri görebilmelidir.
*   **Backend Görevleri:**
    1.  Veri Toplama (Aggregation) Query'leri: Veritabanından doğrudan ham veri çekmek yerine, EF Core'un `GroupBy` ve `Sum` gibi fonksiyonlarını kullanarak optimize edilmiş, özetlenmiş veriler üreten karmaşık sorgular yazmak.
    2.  API Endpoints (Admin yetkisi gerektirir):
        *   `GET /api/analytics/dashboard-summary`: Ana panel için temel metrikler (bugünkü ciro, bekleyen randevu sayısı vb.).
        *   `GET /api/analytics/revenue-report?startDate=...&endDate=...`: Tarih aralığına göre ciro raporu.
        *   `GET /api/analytics/staff-performance-report?startDate=...`: Personel bazında randevu sayısı ve ciro raporu.
*   **Frontend (Yönetim Paneli) Görevleri:**
    1.  Sayfa: `Raporlar`.
    2.  Görselleştirme: Chart.js veya Recharts gibi bir grafik kütüphanesi kullanarak, API'den gelen verileri çizgi, bar ve pasta grafikleriyle görselleştirmek.
    3.  Filtreleme: Raporları tarih aralığına ve personele göre filtreleme imkanı sunan arayüz elemanları (Tarih Seçici, Dropdown).