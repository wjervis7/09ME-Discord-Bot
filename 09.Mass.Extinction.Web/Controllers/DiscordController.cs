﻿namespace Ninth.Mass.Extinction.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Responses;
using Extinction.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModels.Discord;

[Authorize(Roles = "DiscordAdmin")]
public class DiscordController(ApplicationDbContext context, DiscordService discord) : Controller
{
    private static readonly Regex _channelRegex = new("(?<=\\<\\#)(.*?)(?=\\>)");
    private static readonly Regex _userRegex = new("(?<=\\\"user\\\"\\:\\\")(\\d*)(?=\\\")");

    [HttpGet]
    public async Task<IActionResult> Messages()
    {
        var messages = await context.Messages.Select(m => new AdminMessage
        {
            Id = m.Id,
            Sender = m.Sender,
            Body = m.Body,
            DateSent = ((DateTimeOffset)m.DateSent).ToUnixTimeSeconds(),
            IsAnonymous = m.IsAnonymous
        }).ToListAsync();

        var users = (await discord.GetUsersByUsernames(messages.Select(m => m.Sender).ToList())).ToList();

        var model = messages.Select(m =>
        {
            var senderParts = m.Sender.Split("#");
            var username = senderParts[0];
            var discriminator = senderParts[1];
            var user = users.FirstOrDefault(u => u.User.Username == username && u.User.Discriminator == discriminator);
            m.Sender = user?.Nickname ?? m.Sender;
            return m;
        });

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ActivityReports()
    {
        var reports = await context.ActivityReports.Select(ar => new ActivityReport
        {
            Id = ar.Id,
            InitiatorId = ar.Initiator,
            StartTime = ((DateTimeOffset)ar.StartTime).ToUnixTimeSeconds(),
            EndTime = ((DateTimeOffset)ar.EndTime).ToUnixTimeSeconds(),
            ReportType = ar.ReportType,
            Args = ar.Args,
            Report = ar.Report
        }).ToListAsync();

        var userIds = new List<ulong>();
        userIds.AddRange(reports.Select(r => r.InitiatorId));
        userIds.AddRange(reports.SelectMany(r => GetUserIdsFromArgs(r.Args)));
        var users = (await discord.GetUsersByIds(userIds)).ToList();

        var channels = (await discord.GetChannels(reports.SelectMany(r => GetChannelIdsFromReport(r.Report)).ToList())).ToList();

        var model = reports.Select(r =>
        {
            r.Initiator = users.SingleOrDefault(u => u.User.Id == r.InitiatorId)?.Nickname ?? r.InitiatorId.ToString();
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
            var user = users.SingleOrDefault(u => u.User.Id == userId);
            newArgs = newArgs.Replace($"\"user\":\"{match.Value}\"", $"\"user\":\"{user?.Nickname ?? userId.ToString()}\"");
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
