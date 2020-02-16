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
  /// Class which contains commands to delete users from server databases.
  /// </summary>
  public class DeleteModule : InteractiveBase
  {
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
  }
}
