public class CarModel {
    public int Id { get; set; }
    [Required]
    public string ModelName { get; set; } = null!;
}

public class ServiceStation {
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
}

public class RepairRequest {
    public int Id { get; set; }
    [Required]
    public int CarModelId { get; set; } = null!;
    [Required]
    public int ServiceStationId { get; set; } = null!;
    [Required]
    public string IssueDescription { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
