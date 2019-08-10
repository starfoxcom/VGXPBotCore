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
  public class ListModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("list")]
    [Summary("`shows` the users on the bot database.")]
    [Alias("l")]
    public async Task ListAsync()
    {

      //Create & set socket user
      SocketUser socketUser = null;

      //Create and set embed object
      var embed = CoreModule.SimpleEmbed(
        Color.Blue,
        $"Users list",
        null);

      //Create and set embed object
      var embed2 = CoreModule.SimpleEmbed(
        Color.Blue,
        $"Users list (Cont.)",
        null);

      //Create and set user counter
      int userCount = 0;

      List<string> usersToDelete = new List<string>();

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("SELECT id, name FROM users ", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read settings info
            while (dbDataReader.Read())
            {
              socketUser = Context.Guild.Users.FirstOrDefault(
                x => x.Id == Convert.ToUInt64(dbDataReader["id"]));

              //On user not found
              if (socketUser == null)
              {

                usersToDelete.Add($"{dbDataReader["id"]}");
              }

              //On user found
              else
              {

                //On less than 25 users
                if (userCount < 25)
                {

                  //Set embed content
                  embed.AddField($"{dbDataReader["name"]}", $"{socketUser.Mention}", true);

                }

                //On more than 25 users
                else
                {

                  //Set embed content
                  embed2.AddField($"{dbDataReader["name"]}", $"{socketUser.Mention}", true);
                }

                //Add counter
                ++userCount;
              }
            }
          }
        }
      }

      //On users to delete
      if(usersToDelete.Count != 0)
      {
        var embed3 = CoreModule.SimpleEmbed(
          Color.Gold,
          $"Users not found",
          $"These users where deleted from the database, since they're not on the server");

        for (int i = 0; i < usersToDelete.Count; ++i)
        {

          //Set embed content
          embed3.AddField($"{i + 1}", $"<@{usersToDelete[i]}>", true);

          //Execute query
          CoreModule.ExecuteQuery(Context.Guild.Id,
            $"DELETE FROM users WHERE id = {usersToDelete[i]};");
        }

        //Reply embed
        await ReplyAsync("", false, embed3.Build());
      }

      //Set embed content
      embed.WithDescription($"The database has a total of **`{userCount}`** users");

      //Reply embed
      await ReplyAsync("", false, embed.Build());

      //On users more than 25
      if (userCount > 25)
      {

        //Reply embed
        await ReplyAsync("", false, embed2.Build());
      }
    }
  }
}
