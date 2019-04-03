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
    public async Task SetNotificationsAsync(string response)
    {

      //On response 0
      if (response == "0")
      {

        CoreModule.SendNotification(
          $"{Context.Guild.Id}.db",
          "Notification settings changed",
          $"{Context.User.Mention} **changed** the Notification settings to **Off**.",
          Context);

        //Execute query
        CoreModule.ExecuteQuery($"{Context.Guild.Id}.db",
          $"update settings set notifications = 'Off';");

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Set notifications completed",
        $"The **notifications** has been set **Off**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

      }

      //On response 1
      else if (response == "1")
      {

        //Execute query
        CoreModule.ExecuteQuery($"{Context.Guild.Id}.db",
          $"update settings set notifications = 'On';");

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Set notifications completed",
        $"The **notifications** has been set **On**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        CoreModule.SendNotification(
          $"{Context.Guild.Id}.db",
          "Notification settings changed",
          $"{Context.User.Mention} **changed** the Notification settings to **On**.",
          Context);
      }

      //On response not match the options provided
      else
      {

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
          Color.Red,
          "Error",
          "The response didn't match any of the options. The options are:\n" +
          "```cs\n" +
          "[0] Turn Off bot notifications.\n" +
          "[1] Turn On bot notifications.\n" +
          "```\n" +
          "**`Set notifications aborted`**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
