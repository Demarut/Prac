using NBomber.CSharp;
using NBomber.Http.CSharp;

using var httpClient = new HttpClient();

// В NBomber 5+ используется Scenario.Create вместо ScenarioBuilder
var scenario = Scenario.Create("http_scenario", async context =>
{
    var request = Http.CreateRequest("GET", "http://localhost:5252/api/stations")
                      .WithHeader("Accept", "application/json");

    var response = await Http.Send(httpClient, request);

    return response;
})
.WithLoadSimulations(
    // В NBomber 5+ используется Simulation.Inject вместо InjectPerSec
    Simulation.Inject(rate: 100, 
                      interval: TimeSpan.FromSeconds(1), 
                      during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();