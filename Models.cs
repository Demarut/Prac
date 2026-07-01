public class CarModel {
    public int Id { get; set; }
    public string ModelName { get; set; } = "";
}

public class ServiceStation {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
}

public class RepairRequest {
    public int Id { get; set; }
    public int CarModelId { get; set; }
    public int ServiceStationId { get; set; }
    public string IssueDescription { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}