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
  /// Class which contains commands to rename the username of a user on the server database.
  /// </summary>
  public class RenameModule : InteractiveBase
  {
    /// <summary>
    /// Command task which updates the Vainglory username of a specific user on the database.
    /// </summary>
    /// <param name="_username">The new username to update.</param>
    /// <param name="_user">The user to update their username.</param>
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
  }
}
