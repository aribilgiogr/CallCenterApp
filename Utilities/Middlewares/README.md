# Global Exception Handling Middleware (.NET 8.0)

Uygulamada oluşan tüm hataları merkezi bir yerde yakalayan ve yapılandırılmış hata yanıtları sağlayan middleware.

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Özellikler](#özellikler)
- [Kurulum](#kurulum)
- [Konfigürasyon](#konfigürasyon)
- [Kullanım](#kullanım)
- [API Referansı](#api-referansı)
- [Örnek Senaryolar](#örnek-senaryolar)
- [Özel Hata Yönetimi](#özel-hata-yönetimi)
- [Logging ve Monitoring](#logging-ve-monitoring)
- [En İyi Uygulamalar](#en-iyi-uygulamalar)
- [Sorun Giderme](#sorun-giderme)

---

## Genel Bakış

`GlobalExceptionHandlingMiddleware`, ASP.NET Core'un middleware pipeline'ında tüm istekleri yakalar ve meydana gelen istisnalara tutarlı bir şekilde yanıt verir. Bu sayede:

- Tek bir yerde hata yönetimi yapılır
- API yanıtları standartlaştırılır
- Hata mesajları güvenli bir şekilde sunulur
- Tüm hataların logu tutulur

### Avantajları
- ✅ Merkezi hata yönetimi
- ✅ Tutarlı hata yanıtları
- ✅ Tüm istisnaları yakalar
- ✅ HTTP status kodlarını otomatik olarak ayarlar
- ✅ JSON formatında yanıt verir
- ✅ Logging desteği
- ✅ Security-friendly (hassas veriler gizlenir)

---

## Özellikler

| Özellik | Açıklama |
|---------|----------|
| **Merkezi Yönetim** | Tüm hataları bir yerde yönetir |
| **HTTP Status Mapping** | Hata türüne göre doğru HTTP status kodu döner |
| **JSON Yanıt** | Yapılandırılmış JSON formatında yanıt verir |
| **Automatic Logging** | Tüm hataları otomatik olarak kaydeder |
| **Exception Type Handling** | Farklı hata türlerine göre farklı işlem yapar |
| **Production-Ready** | Sensitif bilgileri gizler |
| **Easy to Extend** | Özel hata yönetimi kolayca eklenebilir |

---

## Kurulum

### 1. Dosyaları Projenize Ekleyin

```
Utilities/
├── Middlewares/
│   └── GlobalExceptionHandlingMiddleware.cs
└── Models/
    └── ErrorDetails.cs
```

### 2. Dependency Injection Yapılandırması (Program.cs)

```csharp
using Utilities.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Servisler
builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

// Middleware Pipeline'da en üstte ekleyin!
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Diğer middleware'ler
app.UseRouting();
app.MapControllers();

app.Run();
```

### 3. Basit Kurulum Örneği

```csharp
// Program.cs - Minimal Setup
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

// Exception Handling Middleware'i ekleyin
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapControllers();
app.Run();
```

---

## Konfigürasyon

### Middleware Sırası (Pipeline Order)

```csharp
var app = builder.Build();

// 1. Exception Handling MUTLAKA ilk olmalı
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. Routing
app.UseRouting();

// 4. Authentication
app.UseAuthentication();

// 5. Authorization
app.UseAuthorization();

// 6. Endpoints
app.MapControllers();

app.Run();
```

### Environment-Specific Konfigürasyon

```csharp
using Utilities.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Environment-specific logging
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddLogging(config =>
    {
        config.AddConsole();
        config.AddDebug();
    });
}
else
{
    builder.Services.AddLogging(config =>
    {
        config.AddEventLog();
        config.AddApplicationInsights();
    });
}

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.MapControllers();
app.Run();
```

---

## Kullanım

### Temel Kullanım

Middleware kurulduktan sonra, kontrollerinizdeki hataları normalde throw edebilirsiniz:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _repository;

    public UsersController(IRepository<User> repository)
    {
        _repository = repository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        // Middleware otomatik olarak hataları yakalar ve işler
        var user = await _repository.ReadByKeyAsync(id) 
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı");

        return Ok(user);
    }
}
```

### Hata Yanıtları

#### 400 Bad Request
```json
{
  "statusCode": 400,
  "message": "Parametre null olamaz."
}
```

#### 404 Not Found
```json
{
  "statusCode": 404,
  "message": "Kullanıcı bulunamadı"
}
```

#### 401 Unauthorized
```json
{
  "statusCode": 401,
  "message": "Bu işlem için yetkiniz yok"
}
```

#### 500 Internal Server Error
```json
{
  "statusCode": 500,
  "message": "Sunucu tarafında bir hata oluştu"
}
```

---

## API Referansı

### ErrorDetails Sınıfı

```csharp
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
```

**Özellikler:**
- `StatusCode` (int): HTTP status kodu
- `Message` (string): Hata mesajı

**Metotlar:**
- `ToString()`: Objeyi JSON formatında döner

### GlobalExceptionHandlingMiddleware Sınıfı

```csharp
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // İstek işler ve hataları yakalar
    }
}
```

---

## Örnek Senaryolar

### 1. Basit Get İsteği

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetProduct(int id)
{
    if (id <= 0)
        throw new ArgumentException("ID pozitif olmalıdır");

    var product = await _repository.ReadByKeyAsync(id)
        ?? throw new KeyNotFoundException("Ürün bulunamadı");

    return Ok(product);
}

// İstek: GET /api/products/0
// Yanıt: 400 Bad Request
// {
//   "statusCode": 400,
//   "message": "ID pozitif olmalıdır"
// }

// İstek: GET /api/products/999
// Yanıt: 404 Not Found
// {
//   "statusCode": 404,
//   "message": "Ürün bulunamadı"
// }
```

### 2. Post İsteği ile Validation

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new ArgumentNullException(nameof(dto.Email), "Email boş olamaz");

    if (!dto.Email.Contains("@"))
        throw new ArgumentException("Geçersiz email adresi");

    var user = new User { Email = dto.Email, Name = dto.Name };
    await _repository.CreateAsync(user);
    
    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
}

// Hatalı İstek:
// {
//   "email": "",
//   "name": "John"
// }
//
// Yanıt: 400 Bad Request
// {
//   "statusCode": 400,
//   "message": "Email boş olamaz"
// }
```

### 3. Authorization Hataları

```csharp
[Authorize]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var user = await _repository.ReadByKeyAsync(id)
        ?? throw new KeyNotFoundException("Kullanıcı bulunamadı");

    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    if (user.Id != currentUserId)
        throw new UnauthorizedAccessException("Bu kullanıcıyı silemezsiniz");

    await _repository.DeleteAsync(user);
    
    return NoContent();
}

// Yanıt (Yetkisiz): 401 Unauthorized
// {
//   "statusCode": 401,
//   "message": "Bu kullanıcıyı silemezsiniz"
// }
```

### 4. Database Hataları

```csharp
[HttpPost("bulk-insert")]
public async Task<IActionResult> BulkInsertUsers([FromBody] List<User> users)
{
    try
    {
        foreach (var user in users)
        {
            await _repository.CreateAsync(user);
        }
        await _context.SaveChangesAsync();
        return Ok("Tüm kullanıcılar başarıyla eklendi");
    }
    catch (DbUpdateException ex)
    {
        // Middleware tarafından otomatik olarak yakalaması
        throw new InvalidOperationException("Database hatası: " + ex.InnerException?.Message, ex);
    }
}

// Yanıt: 500 Internal Server Error
// {
//   "statusCode": 500,
//   "message": "Database hatası: ..."
// }
```

### 5. Service Layer Hataları

```csharp
public interface IUserService
{
    Task<User> GetUserAsync(int id);
    Task CreateUserAsync(CreateUserDto dto);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _repository;

    public async Task<User> GetUserAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Geçersiz kullanıcı ID");

        var user = await _repository.ReadByKeyAsync(id)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı");

        return user;
    }

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentNullException(nameof(dto.Email));

        var existingUser = await _repository.FindFirstAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Bu email zaten kullanımda");

        var user = new User { Email = dto.Email, Name = dto.Name };
        await _repository.CreateAsync(user);
    }
}

// Controller
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var user = await _userService.GetUserAsync(id);
    return Ok(user);
}
```

---

## Özel Hata Yönetimi

### Genişletilmiş Error Details

```csharp
public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? ErrorType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
    }
}
```

### Genişletilmiş Middleware

```csharp
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sistemde global bir hata yakalandı!");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorType) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "ArgumentNull"),
            ArgumentException => (StatusCodes.Status400BadRequest, "ArgumentError"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "InvalidOperation"),
            DbUpdateException => (StatusCodes.Status409Conflict, "DatabaseError"),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError")
        };

        context.Response.StatusCode = statusCode;

        var errorDetails = new ErrorDetails
        {
            StatusCode = statusCode,
            Message = exception.Message,
            ErrorType = errorType,
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };

        return context.Response.WriteAsync(errorDetails.ToString());
    }
}
```

### Validation Exception Handler

```csharp
public class ValidationException : Exception
{
    public ValidationException(Dictionary<string, string[]> errors)
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}

// Middleware'de
private static Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";

    var (statusCode, errorType) = exception switch
    {
        ValidationException => (StatusCodes.Status400BadRequest, "Validation"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        _ => (StatusCodes.Status500InternalServerError, "InternalServerError")
    };

    context.Response.StatusCode = statusCode;

    ErrorDetails errorDetails;

    if (exception is ValidationException validationException)
    {
        errorDetails = new ErrorDetails
        {
            StatusCode = statusCode,
            Message = "Validation hatası",
            ErrorType = errorType,
            Errors = validationException.Errors
        };
    }
    else
    {
        errorDetails = new ErrorDetails
        {
            StatusCode = statusCode,
            Message = exception.Message,
            ErrorType = errorType
        };
    }

    return context.Response.WriteAsync(errorDetails.ToString());
}
```

### Custom Exception Sınıfları

```csharp
// Temel custom exception
public class AppException : Exception
{
    public int StatusCode { get; set; }

    public AppException(string message, int statusCode = 500) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

// Business Logic Exception
public class BusinessLogicException : AppException
{
    public BusinessLogicException(string message) 
        : base(message, StatusCodes.Status400BadRequest)
    {
    }
}

// Resource Not Found Exception
public class ResourceNotFoundException : AppException
{
    public ResourceNotFoundException(string resource, object id) 
        : base($"{resource} (ID: {id}) bulunamadı", StatusCodes.Status404NotFound)
    {
    }
}

// Access Denied Exception
public class AccessDeniedException : AppException
{
    public AccessDeniedException(string message = "Bu işlemi yapmaya yetkiniz yok") 
        : base(message, StatusCodes.Status403Forbidden)
    {
    }
}

// Middleware'de kullanım
private static Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.ContentType = "application/json";

    var statusCode = exception switch
    {
        AppException ex => ex.StatusCode,
        _ => StatusCodes.Status500InternalServerError
    };

    context.Response.StatusCode = statusCode;

    var errorDetails = new ErrorDetails
    {
        StatusCode = statusCode,
        Message = exception.Message
    };

    return context.Response.WriteAsync(errorDetails.ToString());
}

// Kullanım
[HttpGet("{id}")]
public async Task<IActionResult> GetProduct(int id)
{
    var product = await _repository.ReadByKeyAsync(id)
        ?? throw new ResourceNotFoundException("Ürün", id);

    return Ok(product);
}
```

---

## Logging ve Monitoring

### Structured Logging

```csharp
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(new EventId(1001, "GlobalException"), ex,
                "Exception {@Method} {@Path} {@Query}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorDetails = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message
        };

        return context.Response.WriteAsync(errorDetails.ToString());
    }
}
```

### Application Insights Entegrasyonu

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;
    private readonly TelemetryClient telemetryClient;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        TelemetryClient telemetryClient)
    {
        this.next = next;
        this.logger = logger;
        this.telemetryClient = telemetryClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sistemde global bir hata yakalandı!");

            // Application Insights'a gönder
            telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                { "Path", context.Request.Path },
                { "Method", context.Request.Method },
                { "TraceId", context.TraceIdentifier }
            });

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorDetails = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            TraceId = context.TraceIdentifier
        };

        return context.Response.WriteAsync(errorDetails.ToString());
    }
}

// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Serilog Entegrasyonu

```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog();

// Middleware'de
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Global Exception: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await HandleExceptionAsync(context, ex);
    }
}
```

---

## En İyi Uygulamalar

### ✅ Yapılması Gerekenler

1. **Middleware'i Pipeline'ın En Başına Ekleyin**
   ```csharp
   var app = builder.Build();
   
   // En ilk satırda!
   app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
   
   // Diğer middleware'ler
   app.UseRouting();
   app.MapControllers();
   ```

2. **Hata Mesajlarını Güvenli Tutun**
   ```csharp
   // ❌ Yanlış - Sensitive bilgi açığa çıkabilir
   var errorDetails = new ErrorDetails
   {
       Message = ex.StackTrace
   };

   // ✅ Doğru - Güvenli mesaj
   var errorDetails = new ErrorDetails
   {
       Message = "Sunucu tarafında bir hata oluştu"
   };
   ```

3. **Tüm Hataları Logla**
   ```csharp
   catch (Exception ex)
   {
       logger.LogError(ex, "Global exception: {Path}",
           context.Request.Path);
       
       await HandleExceptionAsync(context, ex);
   }
   ```

4. **Uygun HTTP Status Kodları Kullanın**
   ```csharp
   context.Response.StatusCode = exception switch
   {
       ArgumentException => StatusCodes.Status400BadRequest,
       KeyNotFoundException => StatusCodes.Status404NotFound,
       UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
       _ => StatusCodes.Status500InternalServerError
   };
   ```

5. **JSON Formatında Yanıt Verin**
   ```csharp
   context.Response.ContentType = "application/json";
   var errorDetails = new ErrorDetails { ... };
   return context.Response.WriteAsync(errorDetails.ToString());
   ```

6. **Test Yazın**
   ```csharp
   [TestClass]
   public class ExceptionHandlingTests
   {
       [TestMethod]
       public async Task InvalidOperation_Returns500()
       {
           // Test implementation
       }
   }
   ```

7. **Trace ID'yi İncludeyin**
   ```csharp
   var errorDetails = new ErrorDetails
   {
       TraceId = context.TraceIdentifier,
       Timestamp = DateTime.UtcNow
   };
   ```

### ❌ Yapılmaması Gerekenler

1. **Middleware'i Pipeline'ın Ortasına Koymayın**
   ```csharp
   // ❌ Yanlış
   app.UseRouting();
   app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
   app.MapControllers();
   ```

2. **Üretimde Detaylı Stack Trace Göstermeyin**
   ```csharp
   // ❌ Yanlış
   Message = $"{ex.Message}\n{ex.StackTrace}"
   ```

3. **Exception'ı Loglamadan Görmezden Gelmeyin**
   ```csharp
   // ❌ Yanlış
   catch (Exception ex)
   {
       // Log yok!
       await HandleExceptionAsync(context, ex);
   }
   ```

4. **Middleware'de Asynchronous Olmayan İşlem Yapmayın**
   ```csharp
   // ❌ Yanlış
   File.WriteAllText("error.txt", ex.Message);
   
   // ✅ Doğru
   await File.WriteAllTextAsync("error.txt", ex.Message);
   ```

5. **Response Yazıldıktan Sonra Durum Kodu Değiştirmeyin**
   ```csharp
   // ❌ Yanlış
   await context.Response.WriteAsync(errorDetails.ToString());
   context.Response.StatusCode = 500; // Çok geç!
   
   // ✅ Doğru
   context.Response.StatusCode = 500;
   await context.Response.WriteAsync(errorDetails.ToString());
   ```

6. **Özel Exception'ları Fark Ettirmeden Geçmeyin**
   ```csharp
   // ❌ Yanlış - Custom exception'lar fark edilmiyor
   var statusCode = exception switch
   {
       KeyNotFoundException => 404,
       _ => 500
   };

   // ✅ Doğru
   var statusCode = exception switch
   {
       KeyNotFoundException => 404,
       BusinessLogicException bex => bex.StatusCode,
       _ => 500
   };
   ```

---

## Sorun Giderme

### Problem: Middleware Çalışmıyor

**Sebep:** Pipeline sırasının yanlış olması

```csharp
// ✅ Doğru Sıra
var app = builder.Build();

// 1. Exception Handling
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 2. HTTPS
app.UseHttpsRedirection();

// 3. Routing
app.UseRouting();

// 4. Auth
app.UseAuthentication();
app.UseAuthorization();

// 5. Endpoints
app.MapControllers();

app.Run();
```

### Problem: JSON Formatı Yanlış

**Çözüm:** JsonSerializerOptions kullanın

```csharp
public override string ToString()
{
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    return JsonSerializer.Serialize(this, options);
}
```

### Problem: Üretimde Exception Detayları Açığa Çıkıyor

**Çözüm:** Environment check yapın

```csharp
private static Task HandleExceptionAsync(
    HttpContext context,
    Exception exception,
    IWebHostEnvironment environment)
{
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = exception switch
    {
        KeyNotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    };

    var message = environment.IsDevelopment()
        ? exception.Message
        : "Sunucu tarafında bir hata oluştu";

    var errorDetails = new ErrorDetails
    {
        StatusCode = context.Response.StatusCode,
        Message = message
    };

    return context.Response.WriteAsync(errorDetails.ToString());
}
```

### Problem: Asynchronous Context Hatası

**Çözüm:** ConfigureAwait kullanın

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await next(context).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Global Exception");
        await HandleExceptionAsync(context, ex).ConfigureAwait(false);
    }
}
```

---

## Unit Testing Örnekleri

### Basic Test Setup

```csharp
[TestClass]
public class GlobalExceptionHandlingMiddlewareTests
{
    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [TestMethod]
    public async Task KeyNotFound_Returns404()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new KeyNotFoundException("Not found"),
            logger.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.AreEqual(404, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task ArgumentNull_Returns400()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new ArgumentNullException("param"),
            logger.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.AreEqual(400, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task UnauthorizedAccess_Returns401()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new UnauthorizedAccessException("Access denied"),
            logger.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.AreEqual(401, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task GeneralException_Returns500()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new Exception("Internal error"),
            logger.Object);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.AreEqual(500, httpContext.Response.StatusCode);
    }

    [TestMethod]
    public async Task Response_IsJsonFormat()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new KeyNotFoundException("Not found"),
            logger.Object);

        // Act
        await middleware.InvokeAsync(httpContext);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(httpContext.Response.Body).ReadToEnd();

        // Assert
        Assert.IsTrue(body.Contains("statusCode"));
        Assert.IsTrue(body.Contains("message"));
    }
}
```

### Integration Test Örneği

```csharp
[TestClass]
public class ExceptionHandlingIntegrationTests
{
    private WebApplication CreateTestApp()
    {
        var builder = WebApplicationBuilder.CreateBuilder();
        builder.Services.AddControllers();

        var app = builder.Build();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.MapControllers();

        return app;
    }

    [TestMethod]
    public async Task ApiEndpoint_ThrowsException_Returns500()
    {
        // Arrange
        var app = CreateTestApp();
        var client = new HttpClient { BaseAddress = new Uri("http://localhost") };

        // Act
        var response = await client.GetAsync("/test/error");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("statusCode"));
    }
}
```

---

## Performance Considerations

### Middleware Performance

```csharp
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<PerformanceMonitoringMiddleware> logger;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                logger.LogWarning(
                    "Slow request detected: {Path} took {ElapsedMs}ms",
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
```

---

## Sürüm Geçmişi

| Versiyon | Açıklama | Tarih |
|----------|----------|-------|
| 1.0.0 | İlk sürüm - Global exception handling | 2024 |
| 1.1.0 | Custom exceptions ve structured logging | 2024 |

---

## Destek

Sorular veya öneriler için lütfen iletişim kurunuz.

**Kaynaklar:**
- [ASP.NET Core Middleware Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware)
- [Exception Handling Best Practices](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ca1031)
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
