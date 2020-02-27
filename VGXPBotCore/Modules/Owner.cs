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
  /// Class which contains commands only for the bot owner.
  /// </summary>
  public class Owner : InteractiveBase
  {
    /// <summary>
    /// Command task which creates the server database.
    /// </summary>
    /// <remarks>
    /// The user requires to be the bot owner.
    /// </remarks>
    [RequireOwner(Group = "Permission")]
    [Command("createdb")]
    public async Task
    CreateDBAsync()
    {
      //Create database.
      CoreModule.CreateServerDB(Context.Guild.Id);

      //Reply embed.
      await ReplyAsync("",
                       false,
                       CoreModule.SimpleEmbed(Color.Green,
                                              "Create database completed",
                                              "**Database** has been **created**.").Build());
    }

    /// <summary>
    /// Command task which deletes the server database.
    /// </summary>
    /// <remarks>
    /// The user requires to be the bot owner.
    /// </remarks>
    [RequireOwner(Group = "Permission")]
    [Command("deletedb")]
    public async Task
    DeleteDBAsync()
    {
      //Delete database.
      CoreModule.DeleteServerDB(Context.Guild.Id);

      //Reply embed.
      await ReplyAsync("",
                       false,
                       CoreModule.SimpleEmbed(Color.Green,
                                              "Delete database completed",
                                              "**Database** has been **deleted**.").Build());
    }

    /// <summary>
    /// Command task which shows the count, owner and name of the servers the bot is in.
    /// </summary>
    /// <remarks>
    /// The user requires to be the bot owner.
    /// </remarks>
    [RequireOwner(Group = "Permission")]
    [Command("servercount")]
    public async Task
    ServerCountAsync()
    {
      //Create and set the guild iterator.
      var guild = Program.g_client.Guilds.GetEnumerator();

      string description = $"The bot is on {Program.g_client.Guilds.Count} server(s)\n\n";
      //Create and set embed object.
      var embed = CoreModule.SimpleEmbed(Color.Blue,
                                         "Server count",
                                         null);

      //Loop guild iterator.
      while (guild.MoveNext()) {
        //Add to description.
        description += $"Owner: **`{guild.Current.Owner.Username}#" +
                         $"{guild.Current.Owner.Discriminator}`**," +
                         $" Name: **`{guild.Current.Name}`**\n";
      }

      //Add description.
      embed.WithDescription(description);

      //Reply embed.
      await ReplyAsync("", false, embed.Build());
    }

    /// <summary>
    /// Command task which sends a global notification to all the servers the bot is in.
    /// </summary>
    /// <remarks>
    /// The user requires to be the bot owner.
    /// </remarks>
    [RequireOwner(Group = "Permission")]
    [Command("globalnotification")]
    public async Task
    GlobalNotificationAsync(string _title, [Remainder] string _description)
    {
      //Create and set the guild iterator.
      var guild = Program.g_client.Guilds.GetEnumerator();

      //Loop guild iterator.
      while (guild.MoveNext()) {
        //Send message to default channel.
        await guild.
              Current.
              DefaultChannel.
              SendMessageAsync("",
                               false,
                               CoreModule.SimpleEmbed(Color.Blue,
                                                      _title,
                                                      _description).Build());

        //Delay task.
        await Task.Delay(1000);
      }
    }
  }
}
