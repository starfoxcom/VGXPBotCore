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
  public class UpdateStatsModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("updatestats")]
    [Summary("`Updates` the actual XP of the user on the database.")]
    [Alias("us")]
    public async Task UpdateStatsAsync(int xpfame, [Remainder]SocketGuildUser user)
    {

      //On user on database
      if (CoreModule.UserExistsServerDB(Context.Guild.Id, user.Id))
      {

        //Execute query
        CoreModule.ExecuteQuery(Context.Guild.Id,
          $"UPDATE users SET actualXP = {xpfame} where id = {user.Id};");

        //Set embed object
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Update completed",
        $"The **update** of the user {user.Mention} is **completed**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        //Send notification
        CoreModule.SendNotification(
          Context.Guild.Id,
          "User stats updated",
          $"{Context.User.Mention} **updated** the user {user.Mention} database stats.");
      }

      //On user not on database
      else
      {

        //Create & set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Red,
        "User not found",
        $"{user.Mention} doesn't exist on the database, **`update aborted`**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
