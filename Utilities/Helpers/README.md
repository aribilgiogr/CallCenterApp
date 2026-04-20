# Utilities Helpers (.NET 8.0)

Bu kütüphane, .NET 8.0 projelerinde sıkça ihtiyaç duyulan **E-posta Gönderimi** (SMTP) ve **Veri İçe Aktarma** (CSV, Excel, JSON) işlemlerini standartlaştırmak için tasarlanmış yardımcı sınıfları içerir.

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Kurulum ve Bağımlılıklar](#kurulum-ve-bağımlılıklar)
- [1. Data Importers (Veri İçe Aktarma)](#1-data-importers-veri-içe-aktarma)
- [2. Email Sender (E-Posta Gönderici)](#2-email-sender-e-posta-gönderici)
- [Hata Yönetimi ve Güvenlik](#hata-yönetimi-ve-güvenlik)

---

## Genel Bakış

- **DataImporters**: `CsvHelper` ve `MiniExcel` kullanarak CSV, Excel ve JSON dosyalarını güçlü tipli (strongly-typed) listelere dönüştürür.
- **EmailSender**: `IEmailSender` arayüzünü uygulayan, SMTP tabanlı ve Identity uyumlu e-posta gönderim servisidir.

---

## Kurulum ve Bağımlılıklar

### Gerekli NuGet Paketleri

```bash
# Veri İşleme
dotnet add package CsvHelper
dotnet add package MiniExcel

# E-posta Servisi
dotnet add package Microsoft.AspNetCore.Identity.UI
```

---

## 1. Data Importers (Veri İçe Aktarma)

`DataImporters.cs` sınıfı, asenkron metodlarla dosya akışlarını işler.

### Desteklenen Formatlar ve Kullanım

| Format | Metot | Paket |
| :--- | :--- | :--- |
| **CSV** | `ImportCsvAsync<T>(stream)` | CsvHelper |
| **Excel** | `ImportExcelAsync<T>(stream)` | MiniExcel |
| **JSON** | `ImportJsonAsync<T>(stream)` | System.Text.Json |

**Örnek Kullanım:**
```csharp
using var stream = file.OpenReadStream();
var data = await DataImporters.ImportExcelAsync<MyViewModel>(stream);
```

---

## 2. Email Sender (E-Posta Gönderici)

### Konfigürasyon (appsettings.json)

```json
{
  "EmailSettings": {
    "Host": "smtp.domain.com",
    "Port": 587,
    "UserName": "info@domain.com",
    "Password": "your-password",
    "DisplayName": "Uygulama Adı"
  }
}
```

### Program.cs Kaydı

```csharp
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();
```

### E-posta Gönderimi

```csharp
await _emailSender.SendEmailAsync("user@example.com", "Konu", "<h1>Mesaj İçeriği</h1>");
```

---

## Hata Yönetimi ve Güvenlik

- **Asenkron Yapı**: Tüm yardımcılar `Task` tabanlıdır. UI kilitlenmelerini önlemek için her zaman `await` kullanın.
- **Hassas Veriler**: SMTP şifrelerini `appsettings.json` yerine **Secret Manager** veya **Environment Variables** üzerinde saklamanız önerilir.
- **SSL/TLS**: Güvenlik için bağlantılarda SSL varsayılan olarak aktiftir (`EnableSsl = true`).
- **Veri Doğrulama**: `DataImporters` kullanmadan önce dosya uzantısını ve içerik tipini kontrol etmeyi unutmayın.

---

## Sürüm Geçmişi
- **v1.1**: Veri içe aktarma yardımcıları (`CSV`, `Excel`, `JSON`) eklendi.
- **v1.0**: SMTP Email desteği ve `IEmailSender` entegrasyonu sağlandı.