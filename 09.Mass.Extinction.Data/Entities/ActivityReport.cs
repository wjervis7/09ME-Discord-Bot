﻿namespace Ninth.Mass.Extinction.Data.Entities;

public class ActivityReport
{
    public long Id { get; set; }
    public ulong Initiator { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ReportType { get; set; }
    public string Args { get; set; }
    public string Report { get; set; }
}
