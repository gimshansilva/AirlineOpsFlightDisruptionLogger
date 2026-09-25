namespace AirlineOpsFlightDisruptionLogger.Models;

public class DisruptionType
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
