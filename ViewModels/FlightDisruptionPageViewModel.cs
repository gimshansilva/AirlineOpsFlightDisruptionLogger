using AirlineOpsFlightDisruptionLogger.Models;

namespace AirlineOpsFlightDisruptionLogger.ViewModels;

public class FlightDisruptionPageViewModel
{
    public DisruptionLogInputViewModel Form { get; set; } = new();

    public IReadOnlyList<DisruptionType> DisruptionTypes { get; set; } = Array.Empty<DisruptionType>();

    public IReadOnlyList<DisruptionLog> Logs { get; set; } = Array.Empty<DisruptionLog>();

    public int TotalDelayMinutes { get; set; }

    public decimal AverageDelayMinutes { get; set; }

    public int PassengerCareCount { get; set; }

    public int HighImpactCount { get; set; }

    public string TopDisruptionDescription { get; set; } = "No data";

    public IReadOnlyList<DisruptionTypeBreakdown> Breakdown { get; set; } = Array.Empty<DisruptionTypeBreakdown>();
}

public class DisruptionTypeBreakdown
{
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Count { get; set; }

    public double Percentage { get; set; }
}
