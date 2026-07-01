using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting; 

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment; 

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    // Теперь мы используем переменную 'env', которая доступна в этом контексте
    if (env.IsEnvironment("Testing"))
    {
        // Этот метод станет доступен после установки пакета из Шага 1
        options.UseInMemoryDatabase("InMemoryDbForApp"); 
    }
    else
    {
        options.UseSqlite("Data Source=service.db");
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Service Center API", Version = "v1" });
    
    // Добавляем поле для ввода ключа в Swagger UI
    c.AddSecurityDefinition("AdminKey", new OpenApiSecurityScheme {
        Description = "Введите секретный ключ в поле ниже. Пример: MySecretKey123",
        Name = "Admin-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "AdminKey" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseStaticFiles();

// Инициализация базы данных (для теста)
if (!env.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated(); // Или db.Database.Migrate();
    }
}

app.Run();
namespace ServiceCenterApp
{
    public partial class Program { }
}