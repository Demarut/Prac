using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ServiceCenterApp; // Убедитесь, что этот using есть

// 1. Наследуемся от IClassFixture, чтобы фабрика создавалась один раз на класс
public class ServiceTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    // 2. Конструктор получает фабрику
    public ServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        // Клиент создаем здесь, а не как поле с инициализатором
        _client = _factory.CreateClient();
    }

    // --- НАСТРОЙКА БАЗЫ ДАННЫХ (Часть 1) ---
    // Этот метод вызывается ПЕРЕД запуском первого теста в классе
    public async Task InitializeAsync()
    {
        // Создаем scope для доступа к сервисам (в том числе к AppDbContext)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Открываем соединение с SQLite в памяти
        await dbContext.Database.OpenConnectionAsync();
        // Применяем миграции или создаем схему БД с нуля
        await dbContext.Database.MigrateAsync(); // Или EnsureCreatedAsync()
    }

    // Этот метод вызывается ПОСЛЕ выполнения всех тестов в классе
    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetRequests_ReturnsUnauthorized_NoKey()
    {
        // Этот тест не требует данных в БД, поэтому просто проверяем авторизацию
        var response = await _client.GetAsync("/api/Requests");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRequest_ReturnsOk_WithAuth()
    {
        // 3. Для каждого теста, меняющего БД, создаем свой scope,
        // чтобы изменения не влияли на другие тесты.
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Добавляем необходимые данные в БД перед тестом,
        // если ваш контроллер требует их наличия (например, CarModel)
        // dbContext.CarModels.Add(new CarModel { Id = 1, ModelName = "TestModel" });
        // await dbContext.SaveChangesAsync();

        // Выполняем HTTP-запрос
        _client.DefaultRequestHeaders.Add("Admin-Key", "MySecretKey123");
        
        var response = await _client.PostAsJsonAsync("/api/Requests", new CreateRequestDto {
            CarModelId = 1,
            ServiceStationId = 1,
            IssueDescription = "Engine smoke"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}