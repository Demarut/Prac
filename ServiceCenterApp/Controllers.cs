using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api")]
public class ServiceController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServiceController(AppDbContext db) => _db = db;

    // ---------- REPAIR REQUESTS (Заявки) ----------

    // GET: api/requests - Получить список всех заявок
    [AdminKey]
    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests()
    {
        var requests = await _db.RepairRequests
            .Join(_db.CarModels, r => r.CarModelId, c => c.Id, (r, c) => new { r, c })
            .Join(_db.ServiceStations, res => res.r.ServiceStationId, s => s.Id, (res, s) => new
            {
                res.r.Id,
                res.r.IssueDescription,
                res.r.CreatedAt,
                CarModelName = res.c.ModelName,
                StationName = s.Name,
                res.r.CarModelId,
                res.r.ServiceStationId
            })
            .ToListAsync();

        return Ok(requests);
    }

    // GET: api/requests/{id} - Получить заявку по ID
    [AdminKey]
    [HttpGet("requests/{id:int}")]
    public async Task<IActionResult> GetRequestById(int id)
    {
        var request = await _db.RepairRequests
            .Join(_db.CarModels, r => r.CarModelId, c => c.Id, (r, c) => new { r, c })
            .Join(_db.ServiceStations, res => res.r.ServiceStationId, s => s.Id, (res, s) => new
            {
                res.r.Id,
                res.r.IssueDescription,
                res.r.CreatedAt,
                CarModelName = res.c.ModelName,
                StationName = s.Name,
                res.r.CarModelId,
                res.r.ServiceStationId
            })
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null)
        {
            return NotFound();
        }
        return Ok(request);
    }

    // POST: api/requests - Создать новую заявку
    [AdminKey]
    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] RequestDto dto)
    {
        var car = await _db.CarModels.FirstOrDefaultAsync(c => c.ModelName == dto.CarName);
        if (car == null)
        {
            car = new CarModel { ModelName = dto.CarName };
            _db.CarModels.Add(car);
            await _db.SaveChangesAsync(); // Сохраняем новую модель авто для получения её Id
        }

        var newRequest = new RepairRequest
        {
            CarModelId = car.Id,
            ServiceStationId = dto.StationId,
            IssueDescription = dto.IssueDescription,
            CreatedAt = DateTime.Now
        };

        _db.RepairRequests.Add(newRequest);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRequestById), new { id = newRequest.Id }, newRequest);
    }

    // PUT: api/requests/{id} - Обновить заявку по ID
    [AdminKey]
    [HttpPut("requests/{id:int}")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] RequestDto dto)
    {
        var request = await _db.RepairRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        var car = await _db.CarModels.FirstOrDefaultAsync(c => c.ModelName == dto.CarName);
        if (car == null)
        {
            car = new CarModel { ModelName = dto.CarName };
            _db.CarModels.Add(car);
            await _db.SaveChangesAsync();
        }

        request.CarModelId = car.Id;
        request.ServiceStationId = dto.StationId;
        request.IssueDescription = dto.IssueDescription;

        await _db.SaveChangesAsync();
        return NoContent(); // 204 No Content - успешное обновление без возврата тела ответа
    }

    // DELETE: api/requests/{id} - Удалить заявку по ID
    [AdminKey]
    [HttpDelete("requests/{id:int}")]
    public async Task<IActionResult> DeleteRequest(int id)
    {
        var request = await _db.RepairRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        _db.RepairRequests.Remove(request);
        await _db.SaveChangesAsync();
        return NoContent(); // 204 No Content - успешное удаление
    }


    // ---------- SERVICE STATIONS (СТО) ----------

    // GET: api/stations - Получить список всех СТО
    [AdminKey]
    [HttpGet("stations")]
    public async Task<IActionResult> GetStations()
    {
        return Ok(await _db.ServiceStations.ToListAsync());
    }

    // GET: api/stations/{id} - Получить СТО по ID
    [AdminKey]
    [HttpGet("stations/{id:int}")]
    public async Task<IActionResult> GetStationById(int id)
    {
        var station = await _db.ServiceStations.FindAsync(id);
        if (station == null)
        {
            return NotFound();
        }
        return Ok(station);
    }

    // POST: api/stations - Создать новое СТО
    [AdminKey]
    [HttpPost("stations")]
    public async Task<IActionResult> CreateStation([FromBody] ServiceStation station)
    {
        _db.ServiceStations.Add(station);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStationById), new { id = station.Id }, station);
    }

    // PUT: api/stations/{id} - Обновить СТО по ID
    [AdminKey]
    [HttpPut("stations/{id:int}")]
    public async Task<IActionResult> UpdateStation(int id, [FromBody] ServiceStation updatedStation)
    {
        var station = await _db.ServiceStations.FindAsync(id);
        if (station == null)
        {
            return NotFound();
        }

        station.Name = updatedStation.Name;
        station.Address = updatedStation.Address;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/stations/{id} - Удалить СТО по ID
    [AdminKey]
    [HttpDelete("stations/{id:int}")]
    public async Task<IActionResult> DeleteStation(int id)
    {
        var station = await _db.ServiceStations.FindAsync(id);
        if (station == null)
        {
            return NotFound();
        }

         // Проверка на связанные заявки перед удалением (защита от ошибок БД)
         if (_db.RepairRequests.Any(r => r.ServiceStationId == id))
         {
             return BadRequest("Невозможно удалить СТО, так как к нему привязаны заявки.");
         }
         
         _db.ServiceStations.Remove(station);
         await _db.SaveChangesAsync();
         return NoContent();
     }


     // ---------- CAR MODELS (Модели авто) ----------
 
     // GET: api/carmodels - Получить список всех моделей авто
     [AdminKey]
     [HttpGet("carmodels")]
     public async Task<IActionResult> GetCarModels()
     {
         return Ok(await _db.CarModels.ToListAsync());
     }
 
     // GET: api/carmodels/{id} - Получить модель авто по ID
     [AdminKey]
     [HttpGet("carmodels/{id:int}")]
     public async Task<IActionResult> GetCarModelById(int id)
     {
         var model = await _db.CarModels.FindAsync(id);
         if (model == null)
         {
             return NotFound();
         }
         return Ok(model);
     }
 
     // POST: api/carmodels - Создать новую модель авто
     [AdminKey]
     [HttpPost("carmodels")]
     public async Task<IActionResult> CreateCarModel([FromBody] CarModel model)
     {
         _db.CarModels.Add(model);
         await _db.SaveChangesAsync();
         return CreatedAtAction(nameof(GetCarModelById), new { id = model.Id }, model);
     }
 
     // PUT: api/carmodels/{id} - Обновить модель авто по ID
     [AdminKey]
     [HttpPut("carmodels/{id:int}")]
     public async Task<IActionResult> UpdateCarModel(int id, [FromBody] CarModel updatedModel)
     {
         var model = await _db.CarModels.FindAsync(id);
         if (model == null)
         {
             return NotFound();
         }
 
         model.ModelName = updatedModel.ModelName;
 
         await _db.SaveChangesAsync();
         return NoContent();
     }
 
     // DELETE: api/carmodels/{id} - Удалить модель авто по ID
     [AdminKey]
     [HttpDelete("carmodels/{id:int}")]
     public async Task<IActionResult> DeleteCarModel(int id)
     {
         var model = await _db.CarModels.FindAsync(id);
         if (model == null)
         {
             return NotFound();
         }
 
          // Проверка на связанные заявки перед удалением (защита от ошибок БД)
          if (_db.RepairRequests.Any(r => r.CarModelId == id))
          {
              return BadRequest("Невозможно удалить модель авто, так как к ней привязаны заявки.");
          }
          
          _db.CarModels.Remove(model);
          await _db.SaveChangesAsync();
          return NoContent();
      }
}

// Вспомогательный класс для приема данных
public class RequestDto {
    public string CarName { get; set; } = "";
    public int StationId { get; set; }
    public string IssueDescription { get; set; } = "";
}