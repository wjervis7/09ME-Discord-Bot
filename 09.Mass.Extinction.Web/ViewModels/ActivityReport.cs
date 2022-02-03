namespace _09.Mass.Extinction.Web.ViewModels;

public class ActivityReport
{
    public long Id { get; set; }
    public ulong InitiatorId { get; set; }
    public string Initiator { get; set; }
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public string ReportType { get; set; }
    public string Args { get; set; }
    public string Report { get; set; }
}
