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

namespace VGXPBotCore.Modules {
  /// <summary>
  /// Class which contains commands to set the bot notifications.
  /// </summary>
  public class SetNotificationsModule : InteractiveBase
  {
    /// <summary>
    /// Command task which changes the notification settings on or off.
    /// </summary>
    /// <param name="_boolean">The bool to change the notifications.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setnotifications")]
    [Summary("`Sets` the notifications of the bot.")]
    [Alias("sn")]
    public async Task
    SetNotificationsAsync(bool _boolean)
    {
      //On true.
      if (_boolean) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"UPDATE settings SET notifications = 'On';");

        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Set notifications completed",
                                           $"The **notifications** have been set **`On`**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());

        //Send notification
        CoreModule.SendNotification(Context.Guild.Id,
                                    "Notification settings changed",
                                    $"{Context.User.Mention} **changed** the Notification" +
                                      $" settings to **`On`**.");
      }
      //On false.
      else {
        //Send notification.
        CoreModule.SendNotification(Context.Guild.Id,
                                    "Notification settings changed",
                                    $"{Context.User.Mention} **changed** the Notification" +
                                      $" settings to **`Off`**.");

        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"UPDATE settings SET notifications = 'Off';");

        //Create and set embed content.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Set notifications completed",
                                           $"The **notifications** have been set **`Off`**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
