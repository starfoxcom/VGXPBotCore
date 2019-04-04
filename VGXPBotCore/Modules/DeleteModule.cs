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
  public class DeleteModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("delete")]
    [Summary("`Delete` the specified user from the bot database.")]
    [Alias("d")]
    public async Task DeleteAsync(SocketGuildUser user)
    {

      //On user on database
      if (CoreModule.UserDBExists(Context.Guild.Id, user.Id))
      {

        //Execute query
        CoreModule.ExecuteQuery(Context.Guild.Id,
          $"DELETE FROM users WHERE id = {user.Id};");

        //Set embed object
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Delete completed",
        $"The **delete** of the user {user.Mention} is **completed**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        //Send notification
        CoreModule.SendNotification(
          Context.Guild.Id,
          "User deleted",
          $"{Context.User.Mention} **deleted** the user {user.Mention} from the database.");
      }

      //On user not on database
      else
      {

        //Create & set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Red,
        "User not found",
        $"{user.Mention} doesn't exist on the database, **`delete aborted`**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
