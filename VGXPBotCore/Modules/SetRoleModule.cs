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

namespace VGXPBotCore.Modules
{
  public class SetRoleModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setrole", RunMode = RunMode.Async)]
    [Summary("`Sets` the member role for the bot.")]
    [Alias("sr")]
    public async Task SetRoleDatabaseAsync(SocketRole role)
    {

      //On server has role
      if (Context.Guild.Roles.Contains(role))
      {

        //Execute query
        CoreModule.ExecuteQuery($"{Context.Guild.Id}.db",
          $"update settings set role = '{role.Name}';");

        //Set embed content
        var embed = CoreModule.SimpleEmbed(
        Color.Green,
        "Set role completed",
        $"The **role** {role.Mention} has been **set**.");

        //Reply embed
        await ReplyAsync("", false, embed.Build());

        CoreModule.SendNotification(
          $"{Context.Guild.Id}.db",
          "Member role changed",
          $"{Context.User.Mention} **changed** the Guild Member **role** to {role.Mention}.",
          Context);
      }
    }
  }
}
