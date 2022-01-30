namespace _09.Mass.Extinction.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data;
using Data.Entities;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using ActivityReport = ViewModels.ActivityReport;

[Authorize(Roles = "DiscordAdmin")]
public class DiscordController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly DiscordService _discord;
    private static readonly Regex _regex = new("(?<=\\<\\#)(.*?)(?=\\>)");

    public DiscordController(ApplicationDbContext context, DiscordService discord)
    {
        _context = context;
        _discord = discord;
    }

    [HttpGet]
    public async Task<IActionResult> Messages()
    {
        IEnumerable<Message> messages = await _context.Messages.ToListAsync();

        return View(messages);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityReports()
    {
        var reports = await _context.ActivityReports.Select(ar => new ActivityReport {
            Id = ar.Id,
            InitiatorId = ar.Initiator,
            StartTime = ar.StartTime,
            EndTime = ar.EndTime,
            ReportType = ar.ReportType,
            Args = ar.Args,
            Report = ar.Report
        }).ToListAsync();

        var userIds = new HashSet<ulong>();
        userIds.AddRange(reports.Select(r => r.InitiatorId));
        var users = await _discord.GetUsers(userIds);
        
        var model = reports.Select(r =>
        {
            r.Initiator = users.Single(u => u.User.Id == r.InitiatorId).Nickname;
            return r;
        });


        return View(model);
    }

    private string ReplaceChannelIdsWithNames(string report, List<DiscordChannel> channels)
    {

        var newReport = "";
        foreach (Match match in _regex.Matches(report))
        {
            var channelId = ulong.Parse(match.Value);
            var channel = channels.Single(c => c.Id == channelId);
            newReport = report.Replace($"<#{match.Value}>", channel.Name);
        }
        return newReport;
    }

    private IEnumerable<ulong> GetChannelIdsFromReport(string report)
    {
        var matches = _regex.Matches(report);
        return matches.Select(m => ulong.Parse(m.Value));
    }
}
