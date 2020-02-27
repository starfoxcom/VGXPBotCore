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
  /// Class which contains commands to manage the server bot settings.
  /// </summary>
  public class Settings : InteractiveBase
  {
    /// <summary>
    /// Command task which gets the bot server specific settings.
    /// </summary>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("getsettings")]
    [Summary("`Gets` the settings of the bot.")]
    [Alias("gs")]
    public async Task GetSettingsAsync()
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db;" +
                                    $" Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand =
               new SQLiteCommand("SELECT prefix, role, notifications, notificationChannel" +
                                   " FROM settings LIMIT 1;",
                                 dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read settings info.
            dbDataReader.Read();

            //Create and set socket role.
            SocketRole role = Context.
                              Guild.
                              Roles.
                              FirstOrDefault(x => x.Name == $"{dbDataReader["role"]}");

            //Create and set socket text channel.
            SocketTextChannel channel = Context.
                                        Guild.
                                        Channels.
                                        FirstOrDefault(x => x.Name ==
                                                       $"{dbDataReader["notificationChannel"]}")
                                                       as SocketTextChannel;

            //Create and set embed object.
            var embed = CoreModule.SimpleEmbed(Color.Purple,
                                               "VGXPBot Settings",
                                               "These are the settings for this server.");

            //Create and set embed object.
            embed.AddField("Prefix",
                           $"**`{dbDataReader["prefix"]}`**",
                           false).
                  AddField("Guild Member role",
                           role == null ? $"{dbDataReader["role"]}" : role.Mention,
                           true).
                  AddField("Notifications",
                           $"**`{dbDataReader["notifications"]}`**",
                           true).
                  AddField("Notification's channel",
                           channel == null ?
                             $"{dbDataReader["notificationChannel"]}" : channel.Mention,
                           true);

            //Reply embed.
            await ReplyAsync("", false, embed.Build());
          }
        }
      }
    }

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

    /// <summary>
    /// Command task which changes the notification settings On or Off.
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

    /// <summary>
    /// Command task which sets the bot prefix of the server.
    /// </summary>
    /// <param name="_prefix">The new bot prefix to set for the server.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setprefix")]
    [Summary("`Sets` the prefix for the bot.")]
    [Alias("sp")]
    public async Task
    SetPrefixAsync(string _prefix)
    {
      //Execute query.
      CoreModule.ExecuteQuery(Context.Guild.Id, $"update settings set prefix = '{_prefix}';");

      //Set embed content.
      var embed = CoreModule.SimpleEmbed(Color.Green,
                                         "Set prefix completed",
                                         $"The **prefix** **`{_prefix}`** has been **set**.");

      //Reply embed.
      await ReplyAsync("", false, embed.Build());

      //Send notification.
      CoreModule.SendNotification(Context.Guild.Id,
                                  "prefix changed",
                                  $"{Context.User.Mention} **changed** the prefix to" +
                                    $" **`{_prefix}`**.");
    }

    /// <summary>
    /// Command task which sets the server member role on the bot database of the server.
    /// </summary>
    /// <param name="_role">The role to set on the bot database of the server.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setrole")]
    [Summary("`Sets` the member role for the bot.")]
    [Alias("sr")]
    public async Task
    SetRoleAsync(SocketRole _role)
    {
      //On server has role.
      if (Context.Guild.Roles.Contains(_role)) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"update settings set role = '{_role.Name}';");

        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Set role completed",
                                           $"The **role** {_role.Mention} has been **set**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        //Send notification.
        CoreModule.SendNotification(Context.Guild.Id,
                                    "Member role changed",
                                    $"{Context.User.Mention} **changed** the Guild Member" +
                                      $" **role** to {_role.Mention}.");
      }
    }
  }
}
