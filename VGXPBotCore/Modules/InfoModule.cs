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
  /// Class which contains commands to show info about the bot.
  /// </summary>
  public class InfoModule : InteractiveBase
  {
    /// <summary>
    /// Command task which shows info about the bot.
    /// </summary>
    [Command("info")]
    [Summary("`Shows` info about the bot.")]
    [Alias("i")]
    public async Task
    InfoAsync()
    {
      //Create and set embed object.
      var embed = CoreModule.SimpleEmbed(Color.Gold,
                                         "Bot info",
                                         "**Vainglory XP Fame Tracker for Discord**\n\n" +
                                           "This is a simple, yet useful Discord Bot made" +
                                           " on .Net Core with the help of" +
                                           " [Discord.Net](https://github.com/discord-net" +
                                           "/Discord.Net) which makes easier to keep track" +
                                           " of the XP Fame of members on a guild in the" +
                                           " mobile game Vainglory.");

      //Add embed thumbnail.
      embed.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());

      //Add embed field.
      embed.AddField("Try it out!",
                     "If you want to give it a try and see for yourself if the bot is what" +
                       " you're looking for your guild, don't hesitate and invite it to" +
                       " your server.\n\n**[Invite me to your server](https://discordapp" +
                       ".com/oauth2/authorize?client_id=378327784499445760&permissions=8" +
                       "&scope=bot)\n[Discord bots link](https://top.gg/bot/" +
                       "378327784499445760)**");

      //Add embed field.
      embed.AddField("Contribute!",
                     "Also, if you want to give some feedback or want to work and improve" +
                     " the bot together, you can find me on Discord as **starfoxcom#8144**.");

      //Add embed field.
      embed.AddField("GitHub Repository",
                     "You can find here the link to the project: [VGXPBotCore]" +
                       "(https://github.com/starfoxcom/VGXPBotCore/)");

      //Add embed footer.
      embed.WithFooter("Made with love and a ton of effort by starfoxcom#8144");

      //Reply embed.
      await ReplyAsync("", false, embed.Build());
    }
  }
}
