# Email Sender Helper (.NET 8.0)

ASP.NET Core Identity ile entegre, SMTP tabanlı email gönderimi için kullanılan yardımcı sınıf.

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Özellikler](#özellikler)
- [Kurulum](#kurulum)
- [Konfigürasyon](#konfigürasyon)
- [Kullanım](#kullanım)
- [API Referansı](#api-referansı)
- [Örnekler](#örnekler)
- [SMTP Sağlayıcıları](#smtp-sağlayıcıları)
- [Hata Yönetimi](#hata-yönetimi)
- [En İyi Uygulamalar](#en-iyi-uygulamalar)
- [Güvenlik](#güvenlik)

---

## Genel Bakış

`EmailSender` sınıfı, ASP.NET Core Identity'nin `IEmailSender` arayüzünü uygulayarak email gönderimi işlemlerini basitleştirir. SMTP protokolü üzerinden güvenli bir şekilde email gönderir.

### Avantajları
- ✅ ASP.NET Core Identity ile natif entegrasyon
- ✅ SSL/TLS şifreleme desteği
- ✅ Dependency Injection uyumlu
- ✅ HTML email desteği
- ✅ Async/Await desteği
- ✅ Kolay konfigürasyon

---

## Özellikler

| Özellik | Açıklama |
|---------|----------|
| **SMTP Support** | Herhangi bir SMTP sunucusu ile çalışır |
| **SSL/TLS** | Güvenli bağlantı |
| **HTML Support** | HTML formatında email gönderilebilir |
| **Async** | Asynchronous gönderim |
| **IEmailSender** | ASP.NET Core Identity uyumlu |
| **Custom Display Name** | Gönderici adı özelleştirilebilir |

---

## Kurulum

### 1. Dosyaları Projenize Ekleyin

```
Utilities/
├── Helpers/
│   └── EmailSender.cs
└── Models/
    └── EmailSettings.cs
```

### 2. NuGet Paketleri

Genellikle `.NET 8.0` ile zaten dahil olan paketler:

```bash
dotnet add package Microsoft.AspNetCore.Identity.UI
```

Opsiyonel olarak email template için:

```bash
dotnet add package Scriban
dotnet add package MimeKit
```

---

## Konfigürasyon

### appsettings.json

Email ayarlarını konfigürasyon dosyasına ekleyin:

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "DisplayName": "Uygulamanız Adı"
  }
}
```

### appsettings.Development.json (Geliştirme Ortamı)

```json
{
  "EmailSettings": {
    "Host": "smtp.mailtrap.io",
    "Port": 2525,
    "UserName": "your-mailtrap-username",
    "Password": "your-mailtrap-password",
    "DisplayName": "Test Uygulaması"
  }
}
```

### appsettings.Production.json (Üretim Ortamı)

```json
{
  "EmailSettings": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "UserName": "apikey",
    "Password": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "DisplayName": "Şirket Adı"
  }
}
```

### Dependency Injection Yapılandırması (Program.cs)

```csharp
using Utilities.Helpers;
using Utilities.Models;

var builder = WebApplication.CreateBuilder(args);

// Email Settings Konfigürasyonu
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// EmailSender Kaydı
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Diğer servisler...
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();
```

---

## Kullanım

### Temel Kullanım

```csharp
using Microsoft.AspNetCore.Identity.UI.Services;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public NotificationsController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail(string email, string subject, string message)
    {
        try
        {
            await _emailSender.SendEmailAsync(email, subject, message);
            return Ok("Email başarıyla gönderildi.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Email gönderilemedi: {ex.Message}");
        }
    }
}
```

### ASP.NET Core Identity ile Entegrasyon

```csharp
// Program.cs
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// EmailSender kaydı - Identity tarafından otomatik olarak kullanılacak
builder.Services.AddScoped<IEmailSender, EmailSender>();
```

Artık Identity, hesap onayı, şifre sıfırlama gibi işlemlerde otomatik olarak email gönderecektir.

---

## API Referansı

### SendEmailAsync

```csharp
public async Task SendEmailAsync(string email, string subject, string htmlMessage)
```

Email gönderir.

**Parametreler:**
- `email` (string): Alıcı email adresi
- `subject` (string): Email konusu
- `htmlMessage` (string): Email gövdesi (HTML desteklenir)

**Dönen Değer:**
- Task (async işlem)

**İstisnalar:**
- `SmtpException`: SMTP bağlantı hatası
- `SmtpFailedRecipientException`: Geçersiz email adresi
- `FormatException`: Yanlış format

**Örnek:**
```csharp
await _emailSender.SendEmailAsync(
    "user@example.com",
    "Hoşgeldiniz",
    "<h1>Hesabınız Oluşturuldu</h1><p>Lütfen email adresinizi doğrulayın.</p>"
);
```

---

## Örnekler

### 1. Basit Metin Email

```csharp
public async Task SendWelcomeEmail(string email, string userName)
{
    var subject = "Hoşgeldiniz!";
    var htmlMessage = $@"
        <h2>Merhaba {userName},</h2>
        <p>Uygulamaya hoşgeldiniz. Hesabınız oluşturulmuştur.</p>
        <p>İyi günler!</p>
    ";

    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### 2. Email Doğrulama Linki

```csharp
public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
{
    var subject = "Email Adresinizi Doğrulayın";
    var htmlMessage = $@"
        <h2>Email Doğrulaması</h2>
        <p>Hesabınızı etkinleştirmek için aşağıdaki linke tıklayın:</p>
        <a href=""{confirmationLink}"" 
           style=""background-color: #007bff; color: white; padding: 10px 20px; 
                  text-decoration: none; border-radius: 5px; display: inline-block;"">
            Emaili Doğrula
        </a>
        <p>Link 24 saat içinde geçersiz olacaktır.</p>
    ";

    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### 3. Şifre Sıfırlama

```csharp
public async Task SendPasswordResetEmailAsync(string email, string resetLink)
{
    var subject = "Şifre Sıfırlama";
    var htmlMessage = $@"
        <h2>Şifre Sıfırlama İsteği</h2>
        <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
        <a href=""{resetLink}"" style=""color: #007bff; text-decoration: none;"">
            Şifremi Sıfırla
        </a>
        <p><strong>Önemli:</strong> Bu linki siz istemediniz ise bu emaili görmezden gelin.</p>
        <p>Link 1 saat içinde geçersiz olacaktır.</p>
    ";

    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### 4. Sipariş Onay Emaili

```csharp
public async Task SendOrderConfirmationAsync(string email, OrderDto order)
{
    var subject = $"Siparişiniz #{order.OrderNumber} Onaylandı";
    
    var itemsHtml = string.Join("", order.Items.Select(item => $@"
        <tr>
            <td>{item.ProductName}</td>
            <td style=""text-align: center;"">{item.Quantity}</td>
            <td style=""text-align: right;"">{item.Price:C}</td>
        </tr>
    "));

    var htmlMessage = $@"
        <h2>Sipariş Onayı</h2>
        <p>Siparişiniz başarıyla alınmıştır.</p>
        
        <h3>Sipariş Detayları</h3>
        <table style=""width: 100%; border-collapse: collapse;"">
            <thead style=""background-color: #f0f0f0;"">
                <tr>
                    <th style=""text-align: left; padding: 8px;"">Ürün</th>
                    <th style=""text-align: center; padding: 8px;"">Adet</th>
                    <th style=""text-align: right; padding: 8px;"">Fiyat</th>
                </tr>
            </thead>
            <tbody>
                {itemsHtml}
            </tbody>
        </table>
        
        <h3 style=""text-align: right; margin-top: 20px;"">
            Toplam: {order.Total:C}
        </h3>
        
        <p>Sipariş durumunuzu takip etmek için hesabınıza giriş yapabilirsiniz.</p>
        <p>Sorularınız için iletişim kurun.</p>
    ";

    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### 5. Template ile Email (Scriban kullanarak)

```csharp
using Scriban;
using System.Collections.Generic;

public class TemplateEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;

    public TemplateEmailService(IEmailSender emailSender, IWebHostEnvironment environment)
    {
        _emailSender = emailSender;
        _environment = environment;
    }

    public async Task SendFromTemplateAsync(string email, string templateName, Dictionary<string, object> model)
    {
        var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", $"{templateName}.html");
        
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template bulunamadı: {templateName}");

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var template = Template.Parse(templateContent);
        var htmlMessage = await template.RenderAsync(model);

        var subject = model.ContainsKey("subject") ? model["subject"].ToString() : "Bildirim";
        await _emailSender.SendEmailAsync(email, subject, htmlMessage);
    }
}
```

**EmailTemplates/WelcomeEmail.html:**
```html
<h2>Merhaba {{ userName }},</h2>
<p>Uygulamaya hoşgeldiniz.</p>
<p>Başlangıç tarihiniz: {{ startDate }}</p>
<a href="{{ confirmationLink }}">Emaili Doğrula</a>
```

### 6. Service Sınıfıyla Kullanım

```csharp
public interface INotificationService
{
    Task SendWelcomeEmailAsync(User user);
    Task SendPasswordResetAsync(User user, string resetToken);
    Task SendOrderNotificationAsync(User user, Order order);
}

public class NotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public NotificationService(IEmailSender emailSender, IConfiguration configuration)
    {
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task SendWelcomeEmailAsync(User user)
    {
        var subject = "Hoşgeldiniz!";
        var htmlMessage = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2>Merhaba {user.UserName},</h2>
                <p>Uygulamaya hoşgeldiniz! Hesabınız başarıyla oluşturulmuştur.</p>
                <p>Hemen başlamak için <a href=""{_configuration["AppUrl"]}"">buraya tıklayın</a>.</p>
            </div>
        ";

        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
    }

    public async Task SendPasswordResetAsync(User user, string resetToken)
    {
        var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        var subject = "Şifre Sıfırlama";
        var htmlMessage = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2>Şifre Sıfırlama</h2>
                <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
                <a href=""{resetLink}"" style=""background-color: #007bff; color: white; padding: 10px 20px; 
                          text-decoration: none; border-radius: 5px; display: inline-block;"">
                    Şifremi Sıfırla
                </a>
                <p>Link 1 saat içinde geçersiz olacaktır.</p>
            </div>
        ";

        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
    }

    public async Task SendOrderNotificationAsync(User user, Order order)
    {
        var subject = $"Siparişiniz #{order.Id} Hazırlanıyor";
        var trackingLink = $"{_configuration["AppUrl"]}/orders/{order.Id}";
        
        var htmlMessage = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                <h2>Sipariş Güncellenmesi</h2>
                <p>{user.FirstName}, siparişiniz hazırlanmaya başlanmıştır.</p>
                <a href=""{trackingLink}"">Sipariş Durumunu Gör</a>
                <p>Sorularınız için iletişim kurun.</p>
            </div>
        ";

        await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
    }
}

// Program.cs
builder.Services.AddScoped<INotificationService, NotificationService>();
```

### 7. Toplu Email Gönderimi

```csharp
public class BulkEmailService
{
    private readonly IEmailSender _emailSender;

    public BulkEmailService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendToMultipleAsync(
        IEnumerable<string> recipients, 
        string subject, 
        string htmlMessage,
        int delayBetweenEmailsMs = 500)
    {
        foreach (var recipient in recipients)
        {
            try
            {
                await _emailSender.SendEmailAsync(recipient, subject, htmlMessage);
                
                // SMTP rate limiting için gecikme
                await Task.Delay(delayBetweenEmailsMs);
            }
            catch (Exception ex)
            {
                // Log ve devam et
                Console.WriteLine($"Email gönderimi başarısız ({recipient}): {ex.Message}");
            }
        }
    }

    public async Task SendNewsletterAsync(
        IEnumerable<User> subscribers,
        string subject,
        string htmlTemplate)
    {
        var tasks = subscribers.Select(user =>
            _emailSender.SendEmailAsync(user.Email, subject, htmlTemplate)
        );

        var results = await Task.WhenAll(tasks);
    }
}
```

### 8. HTML Email Builder

```csharp
public class EmailBuilder
{
    private string _subject = "";
    private string _body = "";
    private string _footerText = "© 2024 Şirket Adı. Tüm hakları saklıdır.";

    public EmailBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }

    public EmailBuilder AddHeader(string title)
    {
        _body += $"<h2>{title}</h2>";
        return this;
    }

    public EmailBuilder AddParagraph(string text)
    {
        _body += $"<p>{text}</p>";
        return this;
    }

    public EmailBuilder AddButton(string text, string url)
    {
        _body += $@"
            <a href=""{url}"" style=""background-color: #007bff; color: white; padding: 10px 20px; 
                              text-decoration: none; border-radius: 5px; display: inline-block; margin: 10px 0;"">
                {text}
            </a>
        ";
        return this;
    }

    public EmailBuilder AddFooter(string footerText)
    {
        _footerText = footerText;
        return this;
    }

    public string Build()
    {
        return $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                {_body}
                <hr style=""border: none; border-top: 1px solid #ddd; margin-top: 30px;"">
                <p style=""font-size: 12px; color: #666;"">{_footerText}</p>
            </div>
        ";
    }
}

// Kullanım
var emailBody = new EmailBuilder()
    .WithSubject("Hoşgeldiniz")
    .AddHeader("Merhaba Kullanıcı")
    .AddParagraph("Uygulamaya hoşgeldiniz!")
    .AddButton("Siteyi Ziyaret Et", "https://example.com")
    .AddFooter("İletişim: support@example.com")
    .Build();
```

---

## SMTP Sağlayıcıları

### Gmail

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "DisplayName": "Uygulama Adı"
  }
}
```

**Not:** Gmail için [App Password](https://myaccount.google.com/apppasswords) oluşturmanız gerekir.

### SendGrid

```json
{
  "EmailSettings": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "UserName": "apikey",
    "Password": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "DisplayName": "Şirket Adı"
  }
}
```

### Mailgun

```json
{
  "EmailSettings": {
    "Host": "smtp.mailgun.org",
    "Port": 587,
    "UserName": "postmaster@your-domain.mailgun.org",
    "Password": "your-mailgun-password",
    "DisplayName": "Uygulamanız"
  }
}
```

### AWS SES

```json
{
  "EmailSettings": {
    "Host": "email-smtp.us-east-1.amazonaws.com",
    "Port": 587,
    "UserName": "your-ses-username",
    "Password": "your-ses-password",
    "DisplayName": "AWS SES"
  }
}
```

### Mailtrap (Test/Development)

```json
{
  "EmailSettings": {
    "Host": "smtp.mailtrap.io",
    "Port": 2525,
    "UserName": "your-mailtrap-username",
    "Password": "your-mailtrap-token",
    "DisplayName": "Test Uygulaması"
  }
}
```

---

## Hata Yönetimi

### Try-Catch ile Hata Yakalama

```csharp
public async Task<IActionResult> SendEmail(string email, string subject, string message)
{
    try
    {
        await _emailSender.SendEmailAsync(email, subject, message);
        return Ok("Email başarıyla gönderildi.");
    }
    catch (SmtpException ex)
    {
        _logger.LogError($"SMTP Hatası: {ex.Message}");
        return BadRequest("SMTP sunucusuna bağlanılamadı.");
    }
    catch (SmtpFailedRecipientException ex)
    {
        _logger.LogError($"Alıcı Hatası: {ex.Message}");
        return BadRequest("Email adresi geçersiz.");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Genel Hata: {ex.Message}");
        return StatusCode(500, "Email gönderilemedi.");
    }
}
```

### Retry Logic ile Hata Yönetimi

```csharp
public async Task SendEmailWithRetryAsync(
    string email, 
    string subject, 
    string htmlMessage,
    int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);
            return;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            _logger.LogWarning($"Email gönderimi başarısız (Deneme {i + 1}/{maxRetries}): {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
        }
        catch (Exception ex)
        {
            _logger.LogError($"Email gönderimi başarısız (Son deneme): {ex.Message}");
            throw;
        }
    }
}
```

### Logging ile Tracking

```csharp
public class LoggingEmailSender : IEmailSender
{
    private readonly IEmailSender _innerSender;
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(IEmailSender innerSender, ILogger<LoggingEmailSender> logger)
    {
        _innerSender = innerSender;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation($"Email gönderiliyor: {email} - Konu: {subject}");

        try
        {
            await _innerSender.SendEmailAsync(email, subject, htmlMessage);
            _logger.LogInformation($"Email başarıyla gönderildi: {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Email gönderimi başarısız: {email}");
            throw;
        }
    }
}

// Program.cs
builder.Services.AddScoped<IEmailSender>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<EmailSettings>>();
    var logger = sp.GetRequiredService<ILogger<LoggingEmailSender>>();
    var baseSender = new EmailSender(settings);
    return new LoggingEmailSender(baseSender, logger);
});
```

---

## En İyi Uygulamalar

### ✅ Yapılması Gerekenler

1. **Environment-Specific Konfigürasyon**
   ```json
   // appsettings.Development.json
   {
     "EmailSettings": {
       "Host": "smtp.mailtrap.io",
       "Port": 2525
     }
   }
   ```

2. **Password'ları Secret Manager ile Yönetin**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "EmailSettings:Password" "your-password"
   ```

3. **HTML Sanitization Yapın**
   ```csharp
   using HtmlSanitizer = Ganss.Xss.HtmlSanitizer;
   
   var sanitizer = new HtmlSanitizer();
   var cleanHtml = sanitizer.Sanitize(userProvidedHtml);
   ```

4. **Async Tüm Şekilde Kullanın**
   ```csharp
   // ✅ Doğru
   await _emailSender.SendEmailAsync(email, subject, message);
   
   // ❌ Yanlış
   _emailSender.SendEmailAsync(email, subject, message).Wait();
   ```

5. **Timeout Ayarlayın**
   ```csharp
   using var client = new SmtpClient(settings.Host, settings.Port)
   {
       Credentials = new NetworkCredential(settings.UserName, settings.Password),
       EnableSsl = true,
       Timeout = 10000 // 10 saniye
   };
   ```

6. **Validation Yapın**
   ```csharp
   public async Task SendEmailAsync(string email, string subject, string htmlMessage)
   {
       if (string.IsNullOrWhiteSpace(email))
           throw new ArgumentException("Email boş olamaz");
       
       if (!email.Contains("@"))
           throw new ArgumentException("Geçersiz email adresi");
       
       if (string.IsNullOrWhiteSpace(subject))
           throw new ArgumentException("Konu boş olamaz");
   }
   ```

7. **Rate Limiting Yapın**
   ```csharp
   public class RateLimitedEmailSender : IEmailSender
   {
       private readonly IEmailSender _innerSender;
       private int _emailsSentInWindow = 0;
       private DateTime _windowStart = DateTime.UtcNow;
       private const int MaxEmailsPerHour = 100;

       public async Task SendEmailAsync(string email, string subject, string htmlMessage)
       {
           if (DateTime.UtcNow - _windowStart > TimeSpan.FromHours(1))
           {
               _emailsSentInWindow = 0;
               _windowStart = DateTime.UtcNow;
           }

           if (_emailsSentInWindow >= MaxEmailsPerHour)
               throw new InvalidOperationException("Saatlik email limiti aşıldı");

           await _innerSender.SendEmailAsync(email, subject, htmlMessage);
           _emailsSentInWindow++;
       }
   }
   ```

### ❌ Yapılmaması Gerekenler

1. **Şifreleri Doğrudan Kodda Saklamayın**
   ```csharp
   // ❌ Yanlış
   const string password = "mypassword123";
   
   // ✅ Doğru
   var password = configuration["EmailSettings:Password"];
   ```

2. **Synchronous Gönderim Yapmayın**
   ```csharp
   // ❌ Yanlış
   client.Send(mailMessage);
   
   // ✅ Doğru
   await client.SendMailAsync(mailMessage);
   ```

3. **HTML Validation Yapmadan Kullanıcı HTML'ini Göndermeyin**
   ```csharp
   // ❌ Yanlış
   await _emailSender.SendEmailAsync(email, subject, userProvidedHtml);
   
   // ✅ Doğru
   var sanitizer = new HtmlSanitizer();
   var safeHtml = sanitizer.Sanitize(userProvidedHtml);
   await _emailSender.SendEmailAsync(email, subject, safeHtml);
   ```

4. **Hata Yakalama Olmadan Göndermeyin**
   ```csharp
   // ❌ Yanlış
   await _emailSender.SendEmailAsync(email, subject, message);
   
   // ✅ Doğru
   try
   {
       await _emailSender.SendEmailAsync(email, subject, message);
   }
   catch (SmtpException ex)
   {
       _logger.LogError($"Email gönderimi başarısız: {ex.Message}");
   }
   ```

5. **Bağlantı Havuzlamasını Açık Bırakmayın**
   ```csharp
   // ❌ Yanlış - Her çağrıda yeni client
   var client = new SmtpClient(settings.Host, settings.Port);
   
   // ✅ Doğru - DI ile tekrar kullan
   builder.Services.AddScoped<IEmailSender, EmailSender>();
   ```

---

## Güvenlik

### 1. Hassas Verileri Koruyun

**appsettings.json'u asla repository'ye koymayın:**
```bash
# .gitignore
appsettings.*.json
appsettings.Production.json
```

### 2. Secret Manager Kullanın (Geliştirme)

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:Password" "your-secure-password"
dotnet user-secrets list
```

### 3. Environment Variables Kullanın (Üretim)

```bash
# Docker
ENV EmailSettings__Password=your-password

# Azure
az webapp config appsettings set --name <app-name> --resource-group <group-name> \
  --settings EmailSettings__Password=your-password
```

### 4. SSL/TLS Zorla

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true
  }
}
```

### 5. Email Adresi Doğrulaması

```csharp
using System.ComponentModel.DataAnnotations;

public class EmailValidator
{
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

### 6. Rate Limiting ve DDoS Koruması

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "email", configure: options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
    });
});
```

### 7. DKIM, SPF, DMARC Yapılandırması

SMTP sunucunuz (SendGrid, AWS SES vs.) ile sağlanan alan yapılandırmasını kullanın:

- **SPF Record:** Hangi SMTP sunucuları emaili gönderebileceğini tanımlar
- **DKIM:** Email'in orijinalliğini doğrular
- **DMARC:** SPF/DKIM başarısız olduğunda ne yapılacağını tanımlar

---

## Performans Optimizasyonu

### Batch Email Gönderimi

```csharp
public class BatchEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly Channel<EmailJob> _queue;

    public BatchEmailService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
        _queue = Channel.CreateUnbounded<EmailJob>();
        ProcessQueue();
    }

    public async ValueTask QueueEmailAsync(string email, string subject, string htmlMessage)
    {
        await _queue.Writer.WriteAsync(new EmailJob { Email = email, Subject = subject, HtmlMessage = htmlMessage });
    }

    private async void ProcessQueue()
    {
        await foreach (var job in _queue.Reader.ReadAllAsync())
        {
            try
            {
                await _emailSender.SendEmailAsync(job.Email, job.Subject, job.HtmlMessage);
            }
            catch (Exception ex)
            {
                // Log error
            }
        }
    }

    private class EmailJob
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string HtmlMessage { get; set; }
    }
}
```

---

## Testing

### Unit Test Örneği

```csharp
[TestClass]
public class EmailSenderTests
{
    [TestMethod]
    public async Task SendEmailAsync_ValidEmail_Success()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        mockOptions.Setup(o => o.Value).Returns(new EmailSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            UserName = "test@test.com",
            Password = "password",
            DisplayName = "Test"
        });

        var emailSender = new EmailSender(mockOptions.Object);

        // Act & Assert
        // Note: SmtpClient'i mock etmek zordur, integration test yapılması önerilir
    }
}
```

---

## Sürüm Geçmişi

| Versiyon | Açıklama | Tarih |
|----------|----------|-------|
| 1.0.0 | İlk sürüm - SMTP Email desteği | 2024 |

---

## Destek

Sorular veya öneriler için lütfen iletişim kurunuz.

**Kaynaklar:**
- [Microsoft Email Services Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail)
- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [SendGrid SMTP Integration](https://sendgrid.com/docs/for-developers/sending-email/integrating-with-the-smtp-api/)
