namespace _09.Mass.Extinction.Web.ViewModels;

using System;

public class ActivityReport
{
    public long Id { get; set; }
    public ulong InitiatorId { get; set; }
    public string Initiator { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ReportType { get; set; }
    public string Args { get; set; }
    public string Report { get; set; }
}
