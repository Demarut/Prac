using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api")]
public class ServiceController : ControllerBase {
    private readonly AppDbContext _db;
    public ServiceController(AppDbContext db) => _db = db;

    [AdminKey]
    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests() {
        // Загружаем данные вместе со связанными объектами
        var requests = await _db.RepairRequests
            .Join(_db.CarModels, r => r.CarModelId, c => c.Id, (r, c) => new { r, c })
            .Join(_db.ServiceStations, res => res.r.ServiceStationId, s => s.Id, (res, s) => new {
                res.r.Id,
                res.r.IssueDescription,
                res.r.CreatedAt,
                CarModelName = res.c.ModelName,
                StationName = s.Name
            }).ToListAsync();
        
        return Ok(requests);
    }

    [HttpGet("stations")]
    public async Task<IActionResult> GetStations() => Ok(await _db.ServiceStations.ToListAsync());

    [AdminKey]
    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] RequestDto dto) {
        // 1. Ищем или создаем модель машины
        var car = await _db.CarModels.FirstOrDefaultAsync(c => c.ModelName == dto.CarName);
        if (car == null) {
            car = new CarModel { ModelName = dto.CarName };
            _db.CarModels.Add(car);
            await _db.SaveChangesAsync();
        }

        // 2. Создаем заявку
        var newRequest = new RepairRequest {
            CarModelId = car.Id,
            ServiceStationId = dto.StationId,
            IssueDescription = dto.IssueDescription,
            CreatedAt = DateTime.Now
        };

        _db.RepairRequests.Add(newRequest);
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }
}

// Вспомогательный класс для приема данных
public class RequestDto {
    public string CarName { get; set; } = "";
    public int StationId { get; set; }
    public string IssueDescription { get; set; } = "";
}