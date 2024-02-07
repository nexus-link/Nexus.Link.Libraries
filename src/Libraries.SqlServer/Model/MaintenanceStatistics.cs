namespace Nexus.Link.Libraries.SqlServer.Model;

public class MaintenanceStatistics
{
    public string Table { get; set; }

    public string Index { get; set; }

    public double DefragmentationPercentage { get; set; }

    public MaintenanceAction Action { get; set; }

    public int PageCount { get; set; }
}