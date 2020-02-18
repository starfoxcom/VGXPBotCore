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
  /// Class which contains commands to set the notification channel of the server for the 
  /// bot notifications.
  /// </summary>
  public class SetNotificationChannelModule : InteractiveBase
  {
    /// <summary>
    /// Command task which sets the notification channel of the server for the bot notifications.
    /// </summary>
    /// <param name="_channel">The channel to send the bot notifications.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setchannel")]
    [Summary("`Sets` the notification's channel of the bot.")]
    [Alias("sc")]
    public async Task
    SetNotificationChannelAsync(SocketTextChannel _channel)
    {
      //On server has text channel.
      if (Context.Guild.Channels.Contains(_channel)) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"update settings set notificationChannel =" +
                                  $" '{_channel.Name}';");

        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Set notification's channel completed",
                                           $"The **channel** {_channel.Mention}" +
                                             $" has been **set**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());

        //Send notification
        CoreModule.SendNotification(Context.Guild.Id,
                                    "Notification's channel changed",
                                    $"{Context.User.Mention} **changed** the Notification's" +
                                      $" channel to {_channel.Mention}.");
      }
    }
  }
}
