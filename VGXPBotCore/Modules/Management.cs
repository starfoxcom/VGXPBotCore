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
  /// Class which contains commands to manage the bot server database.
  /// </summary>
  public class Management : InteractiveBase
  {
    /// <summary>
    /// Command task which signs up the user to the server database.
    /// </summary>
    /// <param name="_username">The Vainglory username to register on the database.</param>
    /// <param name="_region">The region the username belongs.</param>
    [Command("signup")]
    [Summary("`registers` the user to the bot database.")]
    [Alias("snup")]
    public async Task
    SignupAsync(string _username, string _region)
    {
      //Create and set socket guild user.
      SocketGuildUser user = Context.User as SocketGuildUser;

      //Create and set role.
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On socket guild user contains guild member role.
      if (user.Roles.Contains(role)) {
        //On user not on database.
        if (!CoreModule.UserExistsServerDB(Context.Guild.Id, user.Id)) {
          //Execute query.
          CoreModule.ExecuteQuery(Context.Guild.Id,
                                  "INSERT INTO users (id,name,region,actualXP,lastXP)" +
                                    $" VALUES ({user.Id},'{_username}','{_region}',0,0);");

          //Create and set embed object.
          var embed = CoreModule.SimpleEmbed(Color.Green,
                                             "Sign up completed",
                                             $"Your **sign up** is **completed**.");

          //Reply embed.
          await ReplyAsync("", false, embed.Build());

          //Send notification
          CoreModule.SendNotification(Context.Guild.Id,
                                      "User registered",
                                      $"{user.Mention} **registered** to the database.");
        }
        //On user in database.
        else {
          //Create and set embed object.
          var embed = CoreModule.SimpleEmbed(Color.Red,
                                             "User found",
                                             "You're already on the database," +
                                               " **`sign up aborted`**.");

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
                                         $"{user.Mention} you're not allowed to use this" +
                                           $" command, only {role.Mention} is allowed" +
                                           $" to use it.");
        }
        //Otherwise role don't exist.
        else {
          //Set embed object.
          embed = CoreModule.SimpleEmbed(Color.Red,
                                         "Role not set",
                                         "The **role** to use the commands has" +
                                           " **not been set**, please use `~setrole` to set" +
                                           " the role, **`sign up aborted`**.");
        }

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task which signs up the specific user to the server database.
    /// </summary>
    /// <param name="_username">The Vainglory username to register on the database.</param>
    /// <param name="_region">The region the username belongs.</param>
    /// <param name="_user">The Discord user to register.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("signup")]
    [Summary("`registers` the specified user to the bot database.")]
    [Alias("snup")]
    public async Task
    SignupAsync(string _username, string _region, [Remainder]SocketGuildUser _user)
    {
      //Create and set role.
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On socket guild user contains guild member role.
      if (_user.Roles.Contains(role)) {
        //On user not on database.
        if (!CoreModule.UserExistsServerDB(Context.Guild.Id, _user.Id)) {
          //Execute query.
          CoreModule.ExecuteQuery(Context.Guild.Id,
                                  "INSERT INTO users (id,name,region,actualXP,lastXP)" +
                                    $" VALUES ({_user.Id},'{_username}','{_region}',0,0);");

          //Create and set embed object.
          var embed = CoreModule.SimpleEmbed(
          Color.Green,
          "Sign up completed",
          $"The **sign up** of the user {_user.Mention} is **completed**.");

          //Reply embed.
          await ReplyAsync("", false, embed.Build());

          //Send notification.
          CoreModule.SendNotification(Context.Guild.Id,
                                      "User registered",
                                      $"{Context.User.Mention} **registered** the user" +
                                        $" {_user.Mention} to the database.");
        }
        //On user in database.
        else {
          //Create and set embed object.
          var embed = CoreModule.SimpleEmbed(Color.Red,
                                             "User found",
                                             "User **already** on the database," +
                                               " **`sign up aborted`**.");

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
          //Set embed object,
          embed = CoreModule.SimpleEmbed(Color.Red,
                                         "Not a member",
                                         $"{_user.Mention} doesn't have the {role.Mention}" +
                                           $" role, **`sign up aborted`**.");
        }
        //Otherwise role don't exist.
        else {
          //Set embed object.
          embed = CoreModule.SimpleEmbed(Color.Red,
                                         "Role not set",
                                         "The **role** to use the commands has" +
                                           " **not been set**, please use **`~setrole`**" +
                                           " to set the role, **`sign up aborted`**.");
        }

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task which updates the Vainglory username of a specific user on the database.
    /// </summary>
    /// <param name="_username">The new username to update.</param>
    /// <param name="_user">The user to update their username.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("rename")]
    [Summary("`Updates` the Vainglory username of the specified user on the database.")]
    [Alias("rn")]
    public async Task
    RenameUsernameAsync(string _username, [Remainder]SocketGuildUser _user)
    {
      //On user on database.
      if (CoreModule.UserExistsServerDB(Context.Guild.Id, _user.Id)) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"UPDATE users SET name = '{_username}'" +
                                  $" where id = {_user.Id};");

        //Set embed object
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Update completed",
                                           $"The **update** of the user {_user.Mention}" +
                                             $" is **completed**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());

        //Send notification.
        CoreModule.SendNotification(Context.Guild.Id,
                                    "User username updated",
                                    $"{Context.User.Mention} **updated** the user" +
                                      $" {_user.Mention} database username to" +
                                      $" **`{_username}`**.");
      }
      //On user not on database.
      else {
        //Create and set embed content.
        var embed = CoreModule.SimpleEmbed(Color.Red,
                                           "User not found",
                                           $"{_user.Mention} doesn't exist on the database," +
                                             $" **`update aborted`**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task which deletes a specific user from the server database.
    /// </summary>
    /// <param name="_user">The user to delete from the server database.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("delete")]
    [Summary("`Delete` the specified user from the bot database.")]
    [Alias("d")]
    public async Task
    DeleteAsync([Remainder]SocketGuildUser _user)
    {
      //On user on database.
      if (CoreModule.UserExistsServerDB(Context.Guild.Id, _user.Id)) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"DELETE FROM users WHERE id = {_user.Id};");

        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Delete completed",
                                           $"The **delete** of the user" +
                                             $" {_user.Mention} is **completed**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());

        //Send notification.
        CoreModule.SendNotification(Context.Guild.Id,
                                    "User deleted",
                                    $"{Context.User.Mention} **deleted** the user" +
                                      $" {_user.Mention} from the database.");
      }
      //On user not on database.
      else {
        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Red,
                                           "User not found",
                                           $"{_user.Mention} doesn't exist on the database," +
                                             $" **`delete aborted`**.");

        //Reply embed.
        await ReplyAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Command task which shows a list of the users on the server database.
    /// </summary>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("list")]
    [Summary("`shows` the users on the bot database.")]
    [Alias("l")]
    public async Task
    ListAsync()
    {
      //Create and set socket user.
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
