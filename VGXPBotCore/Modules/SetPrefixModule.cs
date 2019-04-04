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
  public class SetPrefixModule : InteractiveBase
  {

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setprefix", RunMode = RunMode.Async)]
    [Summary("`Sets` the prefix for the bot.")]
    [Alias("sp")]
    public async Task SetPrefixAsync(string prefix)
    {

      //Execute query
      CoreModule.ExecuteQuery(Context.Guild.Id,
        $"update settings set prefix = '{prefix}';");

      //Set embed content
      var embed = CoreModule.SimpleEmbed(
      Color.Green,
      "Set prefix completed",
      $"The **prefix** **`{prefix}`** has been **set**.");

      //Reply embed
      await ReplyAsync("", false, embed.Build());

      //Send notification
      CoreModule.SendNotification(
        Context.Guild.Id,
        "prefix changed",
        $"{Context.User.Mention} **changed** the prefix to **{prefix}**.");
    }
  }
}
