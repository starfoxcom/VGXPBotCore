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

namespace VGXPBotCore.Modules {
  /// <summary>
  /// Class which contains commands to set the member role of the server.
  /// </summary>
  public class SetRoleModule : InteractiveBase
  {
    /// <summary>
    /// Command task which sets the server member role on the bot database of the server.
    /// </summary>
    /// <param name="_role">The role to set on the bot database of the server.</param>
    /// <remarks>
    /// The user requires either be able to kick members on the server or be the bot owner.
    /// </remarks>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setrole")]
    [Summary("`Sets` the member role for the bot.")]
    [Alias("sr")]
    public async Task
    SetRoleDatabaseAsync(SocketRole _role)
    {
      //On server has role.
      if (Context.Guild.Roles.Contains(_role)) {
        //Execute query.
        CoreModule.ExecuteQuery(Context.Guild.Id,
                                $"update settings set role = '{_role.Name}';");

        //Create and set embed object.
        var embed = CoreModule.SimpleEmbed(Color.Green,
                                           "Set role completed",
                                           $"The **role** {_role.Mention} has been **set**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        //Send notification.
        CoreModule.SendNotification(Context.Guild.Id,
                                    "Member role changed",
                                    $"{Context.User.Mention} **changed** the Guild Member" +
                                      $" **role** to {_role.Mention}.");
      }
    }
  }
}
