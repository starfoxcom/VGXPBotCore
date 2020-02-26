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
  /// Class which contains commands to update the database user stats.
  /// </summary>
  public class UpdateStatsModule : InteractiveBase
  {
    /// <summary>
    /// 
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
  }
}
