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
  public class GetSettingsModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("getsettings")]
    [Summary("`Gets` the settings of the bot.")]
    [Alias("gs")]
    public async Task GetSettingsAsync()
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("SELECT role, notifications, notificationChannel FROM settings LIMIT 1;", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read settings info
            dbDataReader.Read();

            //Create and set socket role
            SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == $"{dbDataReader["role"]}");

            //Create and set socket text channel
            SocketTextChannel channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == $"{dbDataReader["notificationChannel"]}") as SocketTextChannel;

            //Create and set embed object
            var embed = new EmbedBuilder();

            //Fill embed object
            embed
              .WithColor(Color.Purple)
              .WithAuthor("VGXPBot Settings", Context.Client.CurrentUser.GetAvatarUrl())
              .AddField("Guild Member role", role == null ? $"{dbDataReader["role"]}" : role.Mention, true)
              .AddField("Notifications", $"{dbDataReader["notifications"]}", true)
              .AddField("Notification's channel", channel == null ? $"{dbDataReader["notificationChannel"]}" : channel.Mention, true);

            //Reply embed
            await ReplyAsync("", false, embed.Build());
          }
        }
      }
    }

    [Command("createdb")]
    public async Task createdb()
    {

      CoreModule.CreateDB($"{Context.Guild.Id}.db");
      await ReplyAsync("Database created");
    }

    [Command("deletedb")]
    public async Task deletedb()
    {

      CoreModule.DeleteDB($"{Context.Guild.Id}.db");
      await ReplyAsync("Database deleted");
    }
  }
}
