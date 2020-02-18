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
  /// <summary>
  /// Class which contains commands to get the bot settings.
  /// </summary>
  public class GetSettingsModule : InteractiveBase
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
    /// Command task which creates the server database.
    /// </summary>
    /// <remarks>
    /// The user requires to be the bot owner.
    /// </remarks>
    [RequireOwner(Group = "Permission")]
    [Command("createdb")]
    public async Task CreateDBAsync()
    {
      //Create database.
      CoreModule.CreateServerDB(Context.Guild.Id);

      //Reply embed.
      await ReplyAsync("",
                       false,
                       CoreModule.SimpleEmbed(Color.Green,
                                              "Create database completed",
                                              "**Database** has been **created**.").Build());
    }

    /// <summary>
    /// Command task which deletes the server database.
    /// </summary>
    /// <remarks>
    /// The user requires to be the bot owner.
    /// </remarks>
    [RequireOwner(Group = "Permission")]
    [Command("deletedb")]
    public async Task DeleteDBAsync()
    {
      //Delete database.
      CoreModule.DeleteServerDB(Context.Guild.Id);

      //Reply embed.
      await ReplyAsync("",
                       false,
                       CoreModule.SimpleEmbed(Color.Green,
                                              "Delete database completed",
                                              "**Database** has been **deleted**.").Build());
    }
  }
}
