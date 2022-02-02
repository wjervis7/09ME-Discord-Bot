namespace _09.Mass.Extinction.Web.Controllers;

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
    private static readonly Regex _channelRegex = new("(?<=\\<\\#)(.*?)(?=\\>)");
    private static readonly Regex _userRegex = new("(?<=\\\"user\\\"\\:\\\")(\\d*)(?=\\\")");

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
        userIds.AddRange(reports.SelectMany(r => GetUserIdsFromArgs(r.Args)));
        var users = (await _discord.GetUsers(userIds)).ToList();

        var channelIds = new HashSet<ulong>();
        channelIds.AddRange(reports.SelectMany(r => GetChannelIdsFromReport(r.Report)));
        var channels = (await _discord.GetChannels(channelIds)).ToList();

        var model = reports.Select(r =>
        {
            r.Initiator = users.Single(u => u.User.Id == r.InitiatorId).Nickname;
            r.Args = ReplaceUserIdsWithNames(r.Args, users);
            r.Report = ReplaceChannelIdsWithNames(r.Report, channels);
            return r;
        });


        return View(model);
    }

    private static IEnumerable<ulong> GetUserIdsFromArgs(string args)
    {
        var matches = _userRegex.Matches(args);
        return matches.Select(m => ulong.Parse(m.Value));
    }

    private static IEnumerable<ulong> GetChannelIdsFromReport(string report)
    {
        var matches = _channelRegex.Matches(report);
        return matches.Select(m => ulong.Parse(m.Value));
    }

    private static string ReplaceUserIdsWithNames(string args, IReadOnlyCollection<DiscordGuildMember> users)
    {
        var newArgs = args;

        foreach (Match match in _userRegex.Matches(args))
        {
            var userId = ulong.Parse(match.Value);
            var user = users.Single(u => u.User.Id == userId);
            newArgs = newArgs.Replace($"\"user\":\"{match.Value}\"", $"\"user\":\"{user.Nickname}\"");
        }

        return newArgs;
    }

    private static string ReplaceChannelIdsWithNames(string report, IReadOnlyCollection<DiscordChannel> channels)
    {

        var newReport = report;
        foreach (Match match in _channelRegex.Matches(report))
        {
            var channelId = ulong.Parse(match.Value);
            var channel = channels.SingleOrDefault(c => c.Id == channelId);
            if (channel == null)
            {
                continue;
            }
            newReport = newReport.Replace($"<#{match.Value}>", channel.Name);
        }
        return newReport;
    }
}
