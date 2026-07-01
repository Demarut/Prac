using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<ServiceStation> ServiceStations => Set<ServiceStation>();
    public DbSet<RepairRequest> RepairRequests => Set<RepairRequest>();
}