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
  /// Class which contains commands to list the users on the server database.
  /// </summary>
  public class ListModule : InteractiveBase
  {
    /// <summary>
    /// Command task which shows a list of the users on the server database.
    /// </summary>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("list")]
    [Summary("`shows` the users on the bot database.")]
    [Alias("l")]
    public async Task
    ListAsync()
    {
      //Create & set socket user.
      SocketUser socketUser = null;

      //Create and set embed object.
      var embed = CoreModule.SimpleEmbed(Color.Blue, $"Users list", null);

      //Create and set embed object.
      var embed2 = CoreModule.SimpleEmbed(Color.Blue, $"Users list (Cont.)", null);

      //Create and set user counter.
      int userCount = 0;

      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db;" +
                                    $" Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand = new SQLiteCommand("SELECT id, name FROM users ",
                                                           dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read users info.
            while (dbDataReader.Read()) {
              //Get the user from the server database.
              socketUser = Context.Guild.Users.FirstOrDefault(x =>
                                                              x.Id ==
                                                              Convert.
                                                              ToUInt64(dbDataReader["id"]));

              //On less than 25 users.
              if (userCount < 25) {
                //Set embed content.
                embed.AddField($"{dbDataReader["name"]}", $"{socketUser.Mention}", true);
              }
              //On more than 25 users.
              else {
                //Set embed content.
                embed2.AddField($"{dbDataReader["name"]}", $"{socketUser.Mention}", true);
              }

              //Add to counter.
              ++userCount;
            }
          }
        }
      }

      //Set embed content.
      embed.WithDescription($"The database has a total of **`{userCount}`** users.");

      //Reply embed.
      await ReplyAsync("", false, embed.Build());

      //On users more than 25.
      if (userCount > 25) {
        //Reply embed.
        await ReplyAsync("", false, embed2.Build());
      }
    }
  }
}
