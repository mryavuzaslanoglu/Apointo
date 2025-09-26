### **Kuaför & Berber Randevu Sistemi - Kavramsal Analiz ve Sistem Tasarım Dokümanı**

**Sürüm:** 2.0
**Revizyon Tarihi:** 26.05.2024
**Not:** Bu doküman, .NET 9, MS SQL ve Docker tabanlı bir mimariyi hedef alarak güncellenmiştir.

### 1. Giriş

#### 1.1. Proje Amacı
Kuaför, berber ve güzellik salonları için modern, modüler ve ölçeklenebilir bir randevu yönetimi platformu geliştirmek. Proje, işletmelerin operasyonel verimliliğini artırmayı, randevu kaçırma oranlarını düşürmeyi ve son kullanıcı için kusursuz bir randevu deneyimi sunmayı hedefler.

#### 1.2. Kapsam
Sistem, üç ana bileşenden oluşacaktır:
1.  **Yönetim Paneli (Web - React):** İşletme sahipleri ve personel için.
2.  **Müşteri Portalı (Web - React):** Web üzerinden randevu almak isteyen müşteriler için.
3.  **Müşteri Uygulaması (Mobil - React Native):** iOS ve Android kullanıcıları için.

#### 1.3. Teknoloji Yığını
*   **Backend:** **.NET 9** (En son C# özellikleri ve performans iyileştirmeleri hedeflenerek)
*   **API Mimarisi:** RESTful API (ASP.NET Core)
*   **Frontend (Web & Mobil):** React & React Native (TypeScript ile)
*   **Veritabanı:** **MS SQL Server**
*   **Kimlik Doğrulama:** JWT (JSON Web Tokens)
*   **Containerization:** **Docker**

---

### 2. Sistem Mimarisi

#### 2.1. Backend Mimarisi (.NET 9 - Clean Architecture)
Proje, test edilebilirlik, bakım kolaylığı ve katmanlar arası bağımsızlığı sağlamak amacıyla **Clean Architecture** prensiplerine sıkı sıkıya bağlı kalacaktır.

*   **Core (Domain & Application Katmanları):**
    *   **Domain:** Projenin iş kurallarını, varlıklarını (`Appointment`, `Service`, `Staff` vb.) ve temel mantığını içerir. Hiçbir dış teknolojiye (veritabanı, UI) bağımlılığı yoktur.
    *   **Application:** Kullanım senaryolarını (Use Cases) ve iş akışlarını yönetir. CQRS (Command Query Responsibility Segregation) deseni ile yapılandırılacaktır.
        *   **Commands:** Veri değişikliği yapan operasyonlar (`CreateAppointmentCommand`, `UpdateStaffScheduleCommand`).
        *   **Queries:** Veri okuma operasyonları (`GetAvailableSlotsQuery`, `GetBusinessAnalyticsQuery`).
        *   Dış dünya ile iletişim kuracak arayüzleri (örn: `IAppointmentRepository`, `IEmailSender`) tanımlar.

*   **Infrastructure Katmanı:**
    *   **Persistence:** Entity Framework Core ve **MS SQL Server provider**'ı kullanılarak veritabanı işlemleri (Repository deseninin implementasyonu) bu katmanda yer alır.
    *   **Dış Servisler:** E-posta, SMS ve Push Notification gönderimi gibi üçüncü parti servis entegrasyonları burada bulunur.

*   **Presentation Katmanı (API):**
    *   ASP.NET Core Web API projesidir. Dış dünyadan gelen HTTP isteklerini karşılar, kimlik doğrulama ve yetkilendirme kontrollerini yapar, uygun Command veya Query'yi Application katmanına iletir.

#### 2.2. Frontend Mimarisi (React & React Native)
*   **Responsive ve Mobil-Öncelikli Tasarım:** Web arayüzü, tüm ekran boyutlarında (mobil, tablet, masaüstü) kusursuz bir kullanıcı deneyimi sunacak şekilde **responsive** olarak tasarlanacaktır. CSS Grid, Flexbox ve Media Query'ler aktif olarak kullanılacaktır.
*   **Bileşen Tabanlı Mimari:** Arayüzler, atomik ve yeniden kullanılabilir bileşenlere (örneğin `CalendarView`, `ServiceSelector`, `StaffCard`) ayrılarak geliştirilecektir.
*   **State Yönetimi:** Uygulama genelindeki global state'i (oturum bilgileri, kullanıcı tercihleri vb.) yönetmek için **Redux Toolkit** veya **Zustand** gibi modern bir state yönetim kütüphanesi kullanılacaktır.
*   **Merkezi API Servisi:** Backend ile iletişimi yöneten, istek/cevap döngülerini ve hata yönetimini merkezileştiren bir API katmanı (örn: Axios instance) oluşturulacaktır.

#### 2.3. Dağıtım ve DevOps Mimarisi (Docker)
Projenin geliştirme, test ve canlı ortamlarda tutarlı bir şekilde çalışabilmesi için **Docker** ile containerize edilecektir.

*   **Backend Dockerizasyonu:**
    *   Proje için multi-stage bir `Dockerfile` oluşturulacaktır. İlk stage'de .NET 9 SDK'sı ile proje build edilir, ikinci stage'de ise sadece gerekli runtime ve DLL'ler daha küçük bir ASP.NET Core runtime imajına kopyalanır. Bu, production imajının boyutunu optimize eder.
*   **Frontend Dockerizasyonu:**
    *   React web uygulaması için de multi-stage bir `Dockerfile` kullanılacaktır. İlk stage'de Node.js ortamında uygulama build edilir, ikinci stage'de ise build sonucu oluşan statik dosyalar (HTML, CSS, JS) hafif bir web sunucusu olan **Nginx** imajına kopyalanarak servis edilir.
*   **Orkestrasyon (Yerel Geliştirme için):**
    *   Backend API ve Frontend web sunucusunu tek bir komutla ayağa kaldırmak için bir `docker-compose.yml` dosyası oluşturulacaktır.
*   **Veritabanı Bağlantısı:**
    *   Kullanıcının talebi doğrultusunda, **MS SQL Server veritabanı container içinde çalışmayacaktır.** Uygulama container'ı, çalışacağı ortama (geliştirme, test, production) göre **environment variable** (ortam değişkeni) olarak verilecek bir **connection string** ile harici veritabanına bağlanacaktır. Bu, esneklik sağlar ve veritabanı yönetimini uygulama yaşam döngüsünden ayırır.

---

### 3. Fonksiyonel Gereksinimler (Modüller)

*Bu bölüm, özellik setinin tam bir listesini içerir. Fazlara ayrılmamış, bütünsel bir hedeftir.*

1.  **Çekirdek Yönetim Modülleri:**
    *   İşletme Yönetimi (Çalışma saatleri, tatil günleri, adres, iletişim).
    *   Personel Yönetimi (Sınırsız personel, özel çalışma takvimleri, izinler).
    *   Hizmet Yönetimi (Sınırsız hizmet/kategori, süre, fiyat, tampon süre, paketler).

2.  **Randevu ve Takvim Modülü:**
    *   Yönetici için birleşik takvim görünümü (sürükle-bırak desteği).
    *   Müşteri için akıllı ve dinamik müsaitlik gösterimi.
    *   Çoklu hizmet seçimi ve otomatik süre hesaplama.
    *   Bekleme listesi özelliği.

3.  **Müşteri ve Etkileşim Modülleri:**
    *   Kimlik Doğrulama (Rol bazlı: Admin, Staff, Customer).
    *   CRM (Müşteri profilleri, geçmişi, özel notlar).
    *   Değerlendirme Sistemi (Puanlama ve yorum).
    *   Sadakat Programı (Puan toplama ve harcama).

4.  **Bildirim Modülü:**
    *   Otomatik Randevu Hatırlatmaları (E-posta, SMS, Push Notification).
    *   Sistem Bildirimleri (İptal, onay, bekleme listesi).

5.  **Analitik ve Raporlama Modülü:**
    *   Finansal Raporlar (Ciro, en karlı hizmetler).
    *   Operasyonel Raporlar (Personel performansı, yoğunluk analizi).

---

### 4. Teknik Prensipler ve Kalite Standartları

#### 4.1. Güvenlik
*   **Kimlik Doğrulama & Yetkilendirme:** Tüm API endpoint'leri JWT ve rol bazlı yetkilendirme ile korunacaktır. Bir müşteri, başka bir müşterinin verisine veya yönetim paneli endpoint'lerine erişemeyecektir.
*   **Veri Bütünlüğü:** Hassas veriler (parolalar) **BCrypt** ile hash'lenecektir. Sunucu tarafında yapılacak olan input validasyonu ile XSS, CSRF gibi zafiyetlerin önüne geçilecektir.
*   **Güvenli Konfigürasyon:** Connection string gibi hassas bilgiler kod içerisine gömülmeyecek, .NET'in Secret Manager'ı (geliştirme) ve ortam değişkenleri (production) aracılığıyla güvenli bir şekilde yönetilecektir.

#### 4.2. Kod Kalitesi ve Sürdürülebilirlik (SOLID & Clean Code)
*   **Tek Sorumluluk Prensibi (SRP):** Her sınıf ve metodun tek bir amacı olacaktır. Bu, kodun anlaşılabilirliğini ve test edilebilirliğini artırır.
*   **Bağımlılıkların Tersine Çevrilmesi (DIP):** Yüksek seviyeli modüller, düşük seviyeli modüllerin somut implementasyonlarına değil, arayüzlerine (interface) bağımlı olacaktır. Bu, .NET'in yerleşik **Dependency Injection (DI)** mekanizması ile sağlanacaktır.
*   **Anlaşılabilirlik:** Kod, kendini açıklayan değişken ve metot isimleri ile yazılacak, karmaşık iş mantıkları için XML yorum satırları ile belgelendirilecektir.
*   **Merkezi Hata Yönetimi:** Uygulama genelinde oluşabilecek istisnaları (exception) yakalayıp, loglayıp, kullanıcıya anlamlı bir hata mesajı dönen bir **Global Exception Handling Middleware**'i oluşturulacaktır.

Bu doküman, projenin teknik ve fonksiyonel çerçevesini çizerek, geliştirme ekibi için sağlam bir başlangıç noktası ve yol haritası sunar.