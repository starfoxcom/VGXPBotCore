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
  public class GoalModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("goal")]
    [Summary("`shows` the users on the bot database who achieved the specified goal.")]
    [Alias("g")]
    public async Task GoalAsync(int goal)
    {

      //Create and set SocketUser object
      SocketUser user;

      //Create and set embed object
      var Achieved = CoreModule.SimpleEmbed(
        Color.Green,
        $"Users who achieved {goal} XP Fame",
        null);

      //Create and set embed object
      var Achieved2 = CoreModule.SimpleEmbed(
        Color.Green,
        $"Users who achieved {goal} XP Fame (Cont.)",
        null);

      //Create and set embed object
      var notAchieved = CoreModule.SimpleEmbed(
        Color.Red,
        $"Users who not achieved {goal} XP Fame",
        null);

      //Create and set embed object
      var notAchieved2 = CoreModule.SimpleEmbed(
        Color.Red,
        $"Users who not achieved {goal} XP Fame (Cont.)",
        null);

      //Create and set user counter
      int achievedCount = 0, notAchievedCount = 0;

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("SELECT id, (actualXP - lastXP) as totalXP FROM users ORDER BY totalXP DESC;", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read users stats info
            while (dbDataReader.Read())
            {

              //Get the user id and convert to socketUser
              user = Context.Guild.Users.FirstOrDefault(
                x => x.Id == Convert.ToUInt64(dbDataReader["id"]));

              //On goal achieved
              if (Convert.ToInt32(dbDataReader["totalXP"]) >= goal)
              {

                //On less than 25 users
                if (achievedCount < 25)
                {

                  //Add content to embed object
                  Achieved.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                    user.Mention, true);
                }

                //On more that 25 users
                else
                {

                  //Add content to embed object
                  Achieved2.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                    user.Mention, true);
                }

                achievedCount++;
              }

              //On goal not achieved
              else
              {

                //On less than 25 users
                if (notAchievedCount < 25)
                {

                  //Add content to embed object
                  notAchieved.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                    user.Mention, true);
                }

                //On more that 25 users
                else
                {

                  //Add content to embed object
                  notAchieved2.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                    user.Mention, true);
                }

                notAchievedCount++;
              }


            }
          }
        }
      }

      //On at least one user achieved the goal
      if(achievedCount > 0)
      {

        //Reply embed
        await ReplyAsync("", false, Achieved.Build());
      }

      //On more than 25 users achieved the goal
      if(achievedCount > 25)
      {

        //Reply embed
        await ReplyAsync("", false, Achieved2.Build());
      }

      //On at least one user not achieved the goal
      if(notAchievedCount > 0)
      {

        //Reply embed
        await ReplyAsync("", false, notAchieved.Build());
      }

      //On more than 25 users not achieved the goal
      if(notAchievedCount > 25)
      {

        //Reply embed
        await ReplyAsync("", false, notAchieved2.Build());
      }
    }
  }
}
