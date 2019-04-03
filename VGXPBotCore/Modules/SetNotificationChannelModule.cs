﻿using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;

using System.Data.SQLite;

namespace VGXPBotCore.Modules
{
  public class SetNotificationChannelModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setchannel")]
    [Summary("`Sets` the notification's channel of the bot.")]
    [Alias("sc")]
    public async Task SetNotificationChannelAsync(SocketTextChannel channel)
    {

      //On server has text channel
      if (Context.Guild.Channels.Contains(channel))
      {

        //Execute query
        CoreModule.ExecuteQuery($"{Context.Guild.Id}.db",
          $"update settings set notificationChannel = '{channel.Name}';");

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Set notification's channel completed",
        $"The **channel** {channel.Mention} has been **set**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        CoreModule.SendNotification(
          $"{Context.Guild.Id}.db",
          "Notification's channel changed",
          $"{Context.User.Mention} **changed** the Notification's channel to {channel.Mention}.",
          Context);
      }
    }
  }
}
