# Generic Repository Pattern (.NET 8.0)

Veritabanı işlemleri için tutarlı ve yeniden kullanılabilir bir soyutlama katmanı sağlayan jenerik repository deseni uygulaması.

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Özellikler](#özellikler)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [API Referansı](#api-referansı)
- [Örnekler](#örnekler)
- [En İyi Uygulamalar](#en-iyi-uygulamalar)

---

## Genel Bakış

Bu proje, **Entity Framework Core** ile çalışan bir jenerik repository deseni sağlar. `IRepository<T>` arayüzü ve `Repository<T>` soyut sınıfı, veritabanı işlemlerini standartlaştırır ve kod tekrarını azaltır.

### Mimariye Uygunluk
- ✅ Dependency Injection ile uyumlu
- ✅ Unit Testing için tasarlanmış
- ✅ Entity Framework Core 8.0+ destekli
- ✅ Async/Await desteği
- ✅ LINQ Expression desteği

---

## Özellikler

### 🔍 Okuma İşlemleri
| Metod | Açıklama |
|-------|----------|
| `ReadByKeyAsync(object)` | Birincil anahtarla tek bir varlığı bulur |
| `FindFirstAsync(Expression)` | Belirtilen koşulu sağlayan ilk varlığı bulur |
| `ReadManyAsync(Expression, includes)` | Koşulu sağlayan varlıkları listeler, ilişkileri yükler |

### ✍️ Yazma İşlemleri
| Metod | Açıklama |
|-------|----------|
| `CreateAsync(T)` | Tek bir varlık ekler |
| `CreateManyAsync(IEnumerable)` | Birden fazla varlık ekler |
| `UpdateAsync(T)` | Tek bir varlığı günceller |
| `UpdateManyAsync(IEnumerable)` | Birden fazla varlığı günceller |
| `UpdateManyAsync(Expression)` | Koşulu sağlayan varlıkları günceller |

### 🗑️ Silme İşlemleri
| Metod | Açıklama |
|-------|----------|
| `DeleteAsync(T)` | Tek bir varlığı siler |
| `DeleteManyAsync(IEnumerable)` | Birden fazla varlığı siler |
| `DeleteManyAsync(Expression)` | Koşulu sağlayan varlıkları siler |

### 📊 Sorgu İşlemleri
| Metod | Açıklama |
|-------|----------|
| `CountAsync(Expression)` | Koşulu sağlayan varlık sayısını döner |
| `AnyAsync(Expression)` | Koşulu sağlayan varlık olup olmadığını kontrol eder |

---

## Kurulum

### 1. Dosyaları Projenize Ekleyin

```bash
Utilities/
└── Generics/
    ├── IRepository.cs
    └── Repository.cs
```

### 2. Entity Framework Core Yükleyin

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### 3. DbContext Oluşturun

```csharp
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
}
```

### 4. Dependency Injection Yapılandırma

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repository kayıtları
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

---

## Kullanım

### Somut Repository Sınıfı Oluşturma

```csharp
using Utilities.Generics;

public class UserRepository : Repository<User>
{
    public UserRepository(ApplicationDbContext context) 
        : base(context)
    {
    }

    // İsteğe bağlı: Özel metotlar
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await FindFirstAsync(u => u.Email == email);
    }
}
```

### Arayüz Kaydetme

```csharp
builder.Services.AddScoped<IRepository<User>, UserRepository>();
```

---

## API Referansı

### CreateAsync - Varlık Ekleme

```csharp
public virtual async Task CreateAsync(T entity)
```

Yeni bir varlığı veritabanına ekler. Değişiklikleri kaydetmek için `SaveChangesAsync()` çağrılmalıdır.

**Parametreler:**
- `entity` (T): Eklenecek varlık

---

### ReadByKeyAsync - Anahtarla Okuma

```csharp
public virtual async Task<T?> ReadByKeyAsync(object entityKey)
```

Birincil anahtar ile varlığı bulur.

**Parametreler:**
- `entityKey` (object): Varlığın birincil anahtarı

**Dönen Değer:**
- Bulunursa varlık, aksi halde null

---

### FindFirstAsync - Koşullu Arama

```csharp
public virtual async Task<T?> FindFirstAsync(Expression<Func<T, bool>>? expression = null)
```

LINQ expression ile ilk eşleşen varlığı bulur.

**Parametreler:**
- `expression` (opsiyonel): Arama koşulu, null ise ilk varlığı döner

**Dönen Değer:**
- Bulunursa varlık, aksi halde null

---

### ReadManyAsync - Çoklu Okuma

```csharp
public virtual async Task<IEnumerable<T>> ReadManyAsync(
    Expression<Func<T, bool>>? expression = null, 
    params string[] includes)
```

Koşulu sağlayan varlıkları listeler ve ilişkili varlıkları yükler.

**Parametreler:**
- `expression` (opsiyonel): Filtreleme koşulu
- `includes` (opsiyonel): Yüklenecek navigasyon özellikleri

**Dönen Değer:**
- IEnumerable<T> koleksiyonu

---

### UpdateAsync - Güncelleme

```csharp
public virtual async Task UpdateAsync(T entity)
```

Varlığı güncellenmiş durumda işaretler.

**Parametreler:**
- `entity` (T): Güncellenecek varlık

---

### DeleteAsync - Silme

```csharp
public virtual async Task DeleteAsync(T entity)
```

Varlığı silinmiş durumda işaretler.

**Parametreler:**
- `entity` (T): Silinecek varlık

---

### CountAsync - Sayma

```csharp
public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null)
```

Koşulu sağlayan varlık sayısını döner.

**Parametreler:**
- `expression` (opsiyonel): Sayma koşulu

**Dönen Değer:**
- Varlık sayısı (int)

---

### AnyAsync - Var Mı Kontrolü

```csharp
public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? expression = null)
```

Koşulu sağlayan varlık olup olmadığını kontrol eder.

**Parametreler:**
- `expression` (opsiyonel): Kontrol koşulu

**Dönen Değer:**
- Varlık varsa true, yoksa false

---

## Örnekler

### 1. Varlık Ekleme

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRepository<User> _userRepository;
    private readonly ApplicationDbContext _context;

    public UsersController(
        IRepository<User> userRepository, 
        ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = new User { Name = dto.Name, Email = dto.Email };
        
        await _userRepository.CreateAsync(user);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

### 2. Varlık Okuma

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var user = await _userRepository.ReadByKeyAsync(id);
    
    if (user == null)
        return NotFound();
    
    return Ok(user);
}
```

### 3. Koşullu Arama

```csharp
[HttpGet("email/{email}")]
public async Task<IActionResult> GetUserByEmail(string email)
{
    var user = await _userRepository.FindFirstAsync(u => u.Email == email);
    
    if (user == null)
        return NotFound();
    
    return Ok(user);
}
```

### 4. Çoklu Varlık Yükleme

```csharp
[HttpGet]
public async Task<IActionResult> GetAllUsersWithOrders()
{
    var users = await _userRepository.ReadManyAsync(
        expression: null,
        includes: nameof(User.Orders), nameof(User.Address)
    );
    
    return Ok(users);
}
```

### 5. Filtreleme ve Yükleme

```csharp
[HttpGet("active")]
public async Task<IActionResult> GetActiveUsers()
{
    var activeUsers = await _userRepository.ReadManyAsync(
        expression: u => u.IsActive == true,
        includes: nameof(User.Profile)
    );
    
    return Ok(activeUsers);
}
```

### 6. Varlık Güncelleme

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
{
    var user = await _userRepository.ReadByKeyAsync(id);
    
    if (user == null)
        return NotFound();
    
    user.Name = dto.Name;
    user.Email = dto.Email;
    
    await _userRepository.UpdateAsync(user);
    await _context.SaveChangesAsync();
    
    return NoContent();
}
```

### 7. Varlık Silme

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var user = await _userRepository.ReadByKeyAsync(id);
    
    if (user == null)
        return NotFound();
    
    await _userRepository.DeleteAsync(user);
    await _context.SaveChangesAsync();
    
    return NoContent();
}
```

### 8. Toplu İşlemler

```csharp
public async Task BulkOperations()
{
    // Çoklu ekleme
    var users = new List<User>
    {
        new User { Name = "User1", Email = "user1@example.com" },
        new User { Name = "User2", Email = "user2@example.com" }
    };
    await _userRepository.CreateManyAsync(users);
    await _context.SaveChangesAsync();

    // Koşullu silme
    await _userRepository.DeleteManyAsync(u => u.IsActive == false);
    await _context.SaveChangesAsync();

    // Sayma
    var totalUsers = await _userRepository.CountAsync();
    var activeUsers = await _userRepository.CountAsync(u => u.IsActive == true);

    // Varlık kontrolü
    bool exists = await _userRepository.AnyAsync(u => u.Email == "test@example.com");
}
```

### 9. Unit Testing ile Kullanım

```csharp
[TestClass]
public class UserRepositoryTests
{
    private Mock<IRepository<User>> _mockRepository;
    private UserService _userService;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _userService = new UserService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task CreateUser_Success()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userService.CreateUserAsync(user);

        // Assert
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [TestMethod]
    public async Task GetUserByEmail_NotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.FindFirstAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _userService.GetUserByEmailAsync("notfound@example.com");

        // Assert
        Assert.IsNull(result);
    }
}
```

---

## En İyi Uygulamalar

### ✅ Yapılması Gerekenler

1. **SaveChangesAsync Çağrısını Unutmayın**
   ```csharp
   await _repository.CreateAsync(entity);
   await _context.SaveChangesAsync(); // Zorunlu!
   ```

2. **Null Kontrolleri Yapın**
   ```csharp
   var user = await _repository.ReadByKeyAsync(id);
   if (user == null)
       return NotFound();
   ```

3. **İlişkileri Açıkça Yükleyin**
   ```csharp
   var users = await _repository.ReadManyAsync(
       null,
       nameof(User.Orders),
       nameof(User.Address)
   );
   ```

4. **Özel Repository Sınıfları Oluşturun**
   ```csharp
   public class ProductRepository : Repository<Product>
   {
       // Ürün-specific metodlar
   }
   ```

5. **DTOs ile Dönüş Yapın**
   ```csharp
   var user = await _repository.ReadByKeyAsync(id);
   return _mapper.Map<UserDto>(user);
   ```

### ❌ Yapılmaması Gerekenler

1. **SaveChangesAsync Çağrısını Atlamayın**
   ```csharp
   // ❌ Yanlış
   await _repository.CreateAsync(entity);
   return Ok(); // Değişiklikler kaydedilmedi!
   ```

2. **Lazy Loading Bağımlılığı Almayın**
   ```csharp
   // ❌ Yanlış - Varlık detached olabilir
   var user = await _repository.ReadByKeyAsync(id);
   var orders = user.Orders; // Null olabilir
   ```

3. **Aynı Varlığı Birden Fazla Kez Sorgulama**
   ```csharp
   // ❌ Yanlış
   var user = await _repository.ReadByKeyAsync(id);
   var sameUser = await _repository.ReadByKeyAsync(id); // Tekrar sorgulama
   ```

4. **Async Olmayan Methodları Kullanma**
   ```csharp
   // ❌ Yanlış
   var user = _context.Users.FirstOrDefault(u => u.Id == id);
   ```

---

## Mimari Öneriler

### Layered Architecture Yapısı

```
Presentation Layer (Controllers, ViewModels)
        ↓
Application Layer (Services, DTOs)
        ↓
Data Access Layer (IRepository, Repository)
        ↓
Entity Framework Core
        ↓
Database
```

### Dependency Injection Yapılandırması

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Somut Repositoryler
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
```

---

## Performans Notları

### N+1 Problem Çözümü

```csharp
// ✅ Doğru: Include ile ilişkiler bir sorguda yüklenir
var users = await _userRepository.ReadManyAsync(
    null,
    nameof(User.Orders)
);

// ❌ Yanlış: N+1 sorgusu
var users = await _userRepository.ReadManyAsync();
foreach (var user in users)
{
    var orders = user.Orders; // Ekstra sorgular
}
```

### Veri Türünü Seçin

```csharp
// Sayma için: sadece sayı gerekiyorsa
int count = await _repository.CountAsync();

// Varlık kontrolü için: var mı yok mu kontrol
bool exists = await _repository.AnyAsync(u => u.Id == 1);

// Tam varlık gerekiyorsa: ReadByKeyAsync kullanın
var user = await _repository.ReadByKeyAsync(1);
```

---

## Sürüm Geçmişi

| Versiyon | Açıklama | Tarih |
|----------|----------|-------|
| 1.0.0 | İlk sürüm - .NET 8.0 desteği | 2024 |

---

## Lisans

Bu proje MIT lisansı altında yayınlanmıştır.

---

## Destek

Sorular veya öneriler için lütfen iletişim kurunuz.

**Geliştiriciler için Not:** Bu repository pattern, Entity Framework Core 7.0 ve üzeri sürümlerle uyumludur.
