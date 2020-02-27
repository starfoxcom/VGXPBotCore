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
  /// Class which contains commands to manage the stats of the users on the server database.
  /// </summary>
  public class Stats : InteractiveBase
  {
    /// <summary>
    /// Command task to get the stats of the user.
    /// </summary>
    [Command("stats")]
    [Summary("`Shows` the stats of the user.")]
    [Alias("s")]
    public async Task
    GetStatsAsync()
    {
      //Create and set socket guild user.
      SocketGuildUser user = Context.User as SocketGuildUser;

      //Create and get the server member role.
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On socket guild user contains member role.
      if (user.Roles.Contains(role)) {
        //On user on database.
        if (CoreModule.UserExistsServerDB(Context.Guild.Id, user.Id)) {
          //Create and set the database connection.
          using (SQLiteConnection dbConnection =
                 new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db;" +
                                        $" Version = 3;")) {
            //Open the connection.
            dbConnection.Open();

            //Create and set query.
            using (SQLiteCommand dbCommand = new SQLiteCommand("SELECT name, region," +
                                                                 " actualXP, lastXP," +
                                                                 " (actualXP - lastXP) as" +
                                                                 " totalXP FROM users " +
                                                                 $"WHERE id = {user.Id};",
                                                               dbConnection)) {
              //Create and set the database reader from the command query.
              using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
                //Read user stats info.
                dbDataReader.Read();

                //Create and set embed object.
                var embed = CoreModule.SimpleEmbed(Color.Blue, $"{user.Username} stats", null);

                //Add embed fields.
                embed.AddField("Vainglory username", $"**`{dbDataReader["name"]}`**", true).
                      AddField("Region", $"**`{dbDataReader["region"]}`**", true).
                      AddField("Last XP fame", $"**`{dbDataReader["lastXP"]}`**", true).
                      AddField("Actual XP fame", $"**`{dbDataReader["actualXP"]}`**", true).
                      AddField("Total XP fame", $"**`{dbDataReader["totalXP"]}`**", true);

                //Add user profile.
                embed.WithThumbnailUrl(user.GetAvatarUrl());

                //Reply embed.
                await ReplyAsync("", false, embed.Build());
              }
            }
          }
        }
        //On user not on database.
        else {
          //Create and set embed content.
          var embed = CoreModule.SimpleEmbed(Color.Red,
                                             "User not found",
                                             $"{user.Mention} doesn't exist on the" +
                                               $" database, **`stats aborted`**.");

          //Reply embed.
          await ReplyAsync("", false, embed.Build());
        }
      }
      //On socket guild user not contains role.
      else {
        //Create embed object.
        EmbedBuilder embed;

        //On role exist.
        if (role != null) {
          //Set embed object.
          embed = CoreModule.SimpleEmbed(Color.Red,
                                         "Not a member",
                                         $"{user.Mention} you're not allowed to use" +
                                           $" this command, only {role.Mention} is allowed" +
                                           $" to use it.");
        }
        //Otherwise role don't exist.
        else {
          //Set embed object.
          embed = CoreModule.SimpleEmbed(Color.Red,
                                         "Role not set",
                                         "The **role** to use the commands has" +
                                           " **not been set**, please use `~setrole`" +
                                           " to set the role, **`stats aborted`**.");
        }

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task to get the stats of a specific user.
    /// </summary>
    /// <param name="_user">The user to get the stats.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("stats")]
    [Summary("`Shows` the stats of the specified user.")]
    [Alias("s")]
    public async Task
    GetStatsAsync([Remainder]SocketGuildUser _user)
    {
      //Create and get server member role.
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On user on database.
      if (CoreModule.UserExistsServerDB(Context.Guild.Id, _user.Id)) {
        //Create and set the database connection.
        using (SQLiteConnection dbConnection =
               new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db;" +
                 $" Version = 3;")) {
          //Open the connection.
          dbConnection.Open();

          //Create and set query.
          using (SQLiteCommand dbCommand = new SQLiteCommand("SELECT name, region," +
                                                               " actualXP, lastXP," +
                                                               " (actualXP - lastXP) as" +
                                                               " totalXP FROM users " +
                                                               $"WHERE id = {_user.Id};",
                                                             dbConnection)) {
            //Create and set the database reader from the command query.
            using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
              //Read user stats info.
              dbDataReader.Read();

              //Create and set embed object.
              var embed = CoreModule.SimpleEmbed(Color.Blue,
                                                 $"{_user.Username} stats",
                                                 null);

              //Add embed fields.
              embed.AddField("Vainglory username", $"**`{dbDataReader["name"]}`**", true).
                    AddField("Region", $"**`{dbDataReader["region"]}`**", true).
                    AddField("Last XP fame", $"**`{dbDataReader["lastXP"]}`**", true).
                    AddField("Actual XP fame", $"**`{dbDataReader["actualXP"]}`**", true).
                    AddField("Total XP fame", $"**`{dbDataReader["totalXP"]}`**", true);

              //Add user profile.
              embed.WithThumbnailUrl(_user.GetAvatarUrl());

              //Reply embed.
              await ReplyAsync("", false, embed.Build());
            }
          }
        }
      }
      //On user not on database.
      else {
        //Create & set embed content.
        var embed = CoreModule.SimpleEmbed(Color.Red,
                                           "User not found",
                                           $"{_user.Mention} doesn't exist on the" +
                                             $" database, **`stats aborted`**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task which updates the XP fame of the user on the server database.
    /// </summary>
    /// <param name="_xpfame">The new XP fame to set.</param>
    /// <param name="_user">The Discord user to update.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("updatestats")]
    [Summary("`Updates` the actual XP fame of the user on the database.")]
    [Alias("us")]
    public async Task
    UpdateStatsAsync(int _xpfame, [Remainder]SocketGuildUser _user)
    {
      //On user on database.
      if (CoreModule.UserExistsServerDB(Context.Guild.Id, _user.Id)) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"UPDATE users SET actualXP = {_xpfame}" +
                                  $" where id = {_user.Id};");

        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Update completed",
                                           $"The **update** of the user {_user.Mention}" +
                                             $" is **completed**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());

        //Send notification.
        CoreModule.SendNotification(Context.Guild.Id,
                                    "User stats updated",
                                    $"{Context.User.Mention} **updated** the user" +
                                      $" {_user.Mention} database stats.");
      }
      //On user not on database.
      else {
        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Red,
                                           "User not found",
                                           $"{_user.Mention} doesn't exist on the database," +
                                             $" **`update aborted`**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task which starts new stats for the users on the server database.
    /// </summary>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("newstats")]
    [Summary("`Starts new stats` of the users on the database.")]
    [Alias("ns")]
    public async Task
    StartNewStatsAsync()
    {
      //Execute query.
      CoreModule.ExecuteQuery(Context.Guild.Id, "UPDATE users SET lastXP = actualXP;");

      //Create and set embed object.
      var embed = CoreModule.SimpleEmbed(Color.Green,
                                         "Start new stats completed",
                                         "The **`new stats`** of the database is completed.");

      //Reply embed.
      await ReplyAsync("", false, embed.Build());
    }

    /// <summary>
    /// Command task which list the users based on the average XP fame of all users on the
    ///  server database.
    /// </summary>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("average")]
    [Summary("`Shows the XP fame average` of the users on the database.")]
    [Alias("avg")]
    public async Task
    AverageAsync()
    {
      //Create socket user list.
      List<SocketUser> socketUsers = new List<SocketUser>();

      //Create totalXP list.
      List<int> totalXP = new List<int>();

      //Create and set average XP.
      int averageXP = 0;

      //Create and set minimum XP.
      int minimumXP = 0;

      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db;" +
                                    $" Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand = new SQLiteCommand("SELECT id, (actualXP - lastXP)" +
                                                             " as totalXP FROM users" +
                                                             " ORDER BY totalXP DESC;",
                                                           dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read users stats info.
            while (dbDataReader.Read()) {

              //Add user.
              socketUsers.Add(Context.
                              Guild.
                              Users.
                              FirstOrDefault(x => 
                                             x.Id == 
                                             Convert.ToUInt64(dbDataReader["id"])));

              //Add total XP.
              totalXP.Add(Convert.ToInt32(dbDataReader["totalXP"]));
            }
          }
        }
      }

      //Loop users.
      for (int i = 0; i < socketUsers.Count; ++i) {
        //Sum users XP.
        averageXP += totalXP[i];
      }

      //Get average XP.
      averageXP /= socketUsers.Count;

      //Get minimum XP.
      minimumXP = averageXP >> 1;

      //Create and set embed object.
      var average = CoreModule.SimpleEmbed(Color.Blue,
                                           "Average",
                                           $"Average XP: **`{averageXP}`**\n" +
                                             $"Minimum XP: **`{minimumXP}`**");

      //Create and set embed object.
      var Achieved = CoreModule.SimpleEmbed(Color.Green,
                                            "Achieved",
                                            $"Users above **`{averageXP}`** XP Fame");

      //Create and set embed object.
      var Achieved2 = CoreModule.SimpleEmbed(Color.Green,
                                             "Achieved (Cont.)",
                                             $"Users above {averageXP} XP Fame");

      //Create and set embed object.
      var barelyAchieved = CoreModule.SimpleEmbed(Color.Gold,
                                                  "Barely achieved",
                                                  $"Users below {averageXP} but" +
                                                    $" above {minimumXP} XP Fame");

      //Create and set embed object.
      var barelyAchieved2 = CoreModule.SimpleEmbed(Color.Gold,
                                                   "Barely achieved (Cont.)",
                                                   $"Users below {averageXP} but" +
                                                    $" above {minimumXP} XP Fame");

      //Create and set embed object.
      var notAchieved = CoreModule.SimpleEmbed(Color.Red,
                                               "Not achieved",
                                               $"Users below {minimumXP} XP Fame");

      //Create and set embed object.
      var notAchieved2 = CoreModule.SimpleEmbed(Color.Red,
                                                "Not achieved (Cont.)",
                                                $"Users below {minimumXP} XP Fame");

      //Create and set user counters.
      int achievedCount = 0;
      int barelyAchievedCount = 0;
      int notAchievedCount = 0;

      //Loop users.
      for (int i = 0; i < socketUsers.Count; ++i) {
        //On above average.
        if (totalXP[i] >= averageXP) {
          //On less than 25 users.
          if (achievedCount < 25) {
            //Add embed field.
            Achieved.AddField($"{totalXP[i]}", $"{socketUsers[i].Mention}", true);
          }
          //On more than 25 users.
          else {
            //Add embed field.
            Achieved2.AddField($"{totalXP[i]}", $"{socketUsers[i].Mention}", true);
          }

          //Add to counter.
          achievedCount++;
        }
        //On below average but above minimum average
        else if (totalXP[i] < averageXP && totalXP[i] >= minimumXP) {
          //On less than 25 users.
          if (barelyAchievedCount < 25) {
            //Add embed field.
            barelyAchieved.AddField($"{totalXP[i]}", $"{socketUsers[i].Mention}", true);
          }
          //On more than 25 users.
          else {
            //Add embed field.
            barelyAchieved2.AddField($"{totalXP[i]}", $"{socketUsers[i].Mention}", true);
          }

          //Add to counter.
          barelyAchievedCount++;
        }
        //On below minimum average
        else {
          //On less than 25 users.
          if (notAchievedCount < 25) {
            //Add embed field.
            notAchieved.AddField($"{totalXP[i]}", $"{socketUsers[i].Mention}", true);
          }
          //On more than 25 users.
          else {
            //Add embed field.
            notAchieved2.AddField($"{totalXP[i]}", $"{socketUsers[i].Mention}", true);
          }

          //Add to counter.
          notAchievedCount++;
        }
      }

      //Reply embed.
      await ReplyAsync("", false, average.Build());

      //Delay task.
      await Task.Delay(1000);

      //On at least one user achieved the average.
      if (achievedCount > 0) {
        //Reply embed.
        await ReplyAsync("", false, Achieved.Build());
      }

      //On more than 25 users achieved the average.
      if (achievedCount > 25) {
        //Reply embed.
        await ReplyAsync("", false, Achieved2.Build());
      }

      //Delay task.
      await Task.Delay(1000);

      //On at least one user achieved the minimum average.
      if (barelyAchievedCount > 0) {
        //Reply embed.
        await ReplyAsync("", false, barelyAchieved.Build());
      }

      //On more than 25 users achieved the minimum average.
      if (barelyAchievedCount > 25) {
        //Reply embed.
        await ReplyAsync("", false, barelyAchieved2.Build());
      }

      //Delay task.
      await Task.Delay(1000);

      //On at least one user not achieved the minimum average.
      if (barelyAchievedCount > 0) {
        //Reply embed.
        await ReplyAsync("", false, barelyAchieved.Build());
      }

      //On more than 25 users not achieved the minimum average.
      if (barelyAchievedCount > 25) {
        //Reply embed.
        await ReplyAsync("", false, barelyAchieved2.Build());
      }
    }

    /// <summary>
    /// Command task to list the users who achieved a specific XP fame goal.
    /// </summary>
    /// <param name="_goal">The XP fame to achieve.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("goal")]
    [Summary("`shows` the users on the bot database who achieved the specified goal.")]
    [Alias("g")]
    public async Task
    GoalAsync(int _goal)
    {
      //Create socket user object.
      SocketUser user;

      //Create and set embed object.
      var Achieved = CoreModule.SimpleEmbed(Color.Green,
                                            $"Users who achieved {_goal} XP Fame",
                                            null);

      //Create and set embed object.
      var Achieved2 = CoreModule.SimpleEmbed(Color.Green,
                                             $"Users who achieved {_goal} XP Fame (Cont.)",
                                             null);

      //Create and set embed object.
      var notAchieved = CoreModule.SimpleEmbed(Color.Red,
                                               $"Users who not achieved {_goal} XP Fame",
                                               null);

      //Create and set embed object.
      var notAchieved2 = CoreModule.SimpleEmbed(Color.Red,
                                                $"Users who not achieved {_goal} XP Fame" +
                                                  $" (Cont.)",
                                                null);

      //Create and set user counters.
      int achievedCount = 0;
      int notAchievedCount = 0;

      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db;" +
                                    $" Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand = new SQLiteCommand("SELECT id, (actualXP - lastXP)" +
                                                             " as totalXP FROM users" +
                                                             " ORDER BY totalXP DESC;",
                                                           dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read users stats info.
            while (dbDataReader.Read()) {
              //Get the user id and convert to socketUser.
              user = Context.
                     Guild.
                     Users.
                     FirstOrDefault(x => x.Id == Convert.ToUInt64(dbDataReader["id"]));

              //On goal achieved.
              if (Convert.ToInt32(dbDataReader["totalXP"]) >= _goal) {
                //On less than 25 users.
                if (achievedCount < 25) {
                  //Add content to embed object.
                  Achieved.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                                    user.Mention,
                                    true);
                }
                //On more that 25 users.
                else {
                  //Add content to embed object.
                  Achieved2.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                                     user.Mention,
                                     true);
                }
                //Add to counter.
                achievedCount++;
              }
              //On goal not achieved.
              else {
                //On less than 25 users.
                if (notAchievedCount < 25) {
                  //Add content to embed object.
                  notAchieved.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                                       user.Mention,
                                       true);
                }
                //On more that 25 users.
                else {
                  //Add content to embed object.
                  notAchieved2.AddField($"**`{Convert.ToInt32(dbDataReader["totalXP"])}`**",
                                        user.Mention,
                                        true);
                }
                //Add to counter.
                notAchievedCount++;
              }
            }
          }
        }
      }

      //On at least one user achieved the goal.
      if (achievedCount > 0) {
        //Reply embed.
        await ReplyAsync("", false, Achieved.Build());
      }

      //On more than 25 users achieved the goal.
      if (achievedCount > 25) {
        //Reply embed.
        await ReplyAsync("", false, Achieved2.Build());
      }

      //On at least one user not achieved the goal.
      if (notAchievedCount > 0) {
        //Reply embed.
        await ReplyAsync("", false, notAchieved.Build());
      }

      //On more than 25 users not achieved the goal.
      if (notAchievedCount > 25) {
        //Reply embed.
        await ReplyAsync("", false, notAchieved2.Build());
      }
    }
  }
}
