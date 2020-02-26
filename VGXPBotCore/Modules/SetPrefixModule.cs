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
  /// Class which contains commands to set the bot prefix.
  /// </summary>
  public class SetPrefixModule : InteractiveBase
  {
    /// <summary>
    /// Command task which sets the bot prefix of the server.
    /// </summary>
    /// <param name="_prefix">The new bot prefix to set for the server.</param>
    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("setprefix")]
    [Summary("`Sets` the prefix for the bot.")]
    [Alias("sp")]
    public async Task
    SetPrefixAsync(string _prefix)
    {
      //Execute query.
      CoreModule.ExecuteQuery(Context.Guild.Id, $"update settings set prefix = '{_prefix}';");

      //Set embed content.
      var embed = CoreModule.SimpleEmbed(Color.Green,
                                         "Set prefix completed",
                                         $"The **prefix** **`{_prefix}`** has been **set**.");

      //Reply embed.
      await ReplyAsync("", false, embed.Build());

      //Send notification.
      CoreModule.SendNotification(Context.Guild.Id,
                                  "prefix changed",
                                  $"{Context.User.Mention} **changed** the prefix to" +
                                    $" **`{_prefix}`**.");
    }
  }
}
