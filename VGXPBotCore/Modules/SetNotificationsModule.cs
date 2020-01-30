using System;
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
  public class SetNotificationsModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setnotifications")]
    [Summary("`Sets` the notifications of the bot.")]
    [Alias("sn")]
    public async Task SetNotificationsAsync(bool boolean)
    {

      //On true
      if (boolean)
      {

        //Execute query
        CoreModule.ExecuteQuery(Context.Guild.Id,
          $"update settings set notifications = 'On';");

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Set notifications completed",
        $"The **notifications** have been set **On**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        //Send notification
        CoreModule.SendNotification(
          Context.Guild.Id,
          "Notification settings changed",
          $"{Context.User.Mention} **changed** the Notification settings to **On**.");
      }

      //On false
      else
      {

        //Send notification
        CoreModule.SendNotification(
          Context.Guild.Id,
          "Notification settings changed",
          $"{Context.User.Mention} **changed** the Notification settings to **Off**.");

        //Execute query
        CoreModule.ExecuteQuery(Context.Guild.Id,
          $"update settings set notifications = 'Off';");

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Set notifications completed",
        $"The **notifications** have been set **Off**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
