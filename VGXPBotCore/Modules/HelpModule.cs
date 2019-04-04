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
  public class HelpModule : InteractiveBase
  {

    [Command("help")]
    [Summary("`Shows` the commands of the bot.")]
    [Alias("h")]
    public async Task HelpAsync()
    {

      //Get prefix from guild settings
      string prefix = CoreModule.GetPrefix($"{Context.Guild.Id}.db");

      //Create and set embed object
      var builder = CoreModule.SimpleEmbed(
        Color.Orange,
        "Bot Commands",
        "These are the commands **you can use**");

      //For each module on command service
      foreach (var module in Program._commands.Modules)
      {

        //Create description string
        string description = null;

        //For each command on module
        foreach (var cmd in module.Commands)
        {

          //Create and set precondition
          var result = await cmd.CheckPreconditionsAsync(Context);

          //On precondition success
          if (result.IsSuccess)
          {

            //Add description for each command
            description += $"**`{prefix}`**{cmd.Aliases.First()} {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n";
          }
        }

        //On description is not null
        if (!string.IsNullOrWhiteSpace(description))
        {

          //Add field to embed object
          builder.AddField(x =>
          {

            //Set name
            x.Name = module.Name;

            //Set description
            x.Value = description;

            //Field inline
            x.IsInline = true;
          });
        }
      }

      //Reply embed
      await ReplyAsync("", false, builder.Build());
    }
       
    [Command("help")]
    [Summary("`Shows` the specified command of the bot.")]
    [Alias("h")]
    public async Task HelpAsync(string command)
    {

      //Search for the command on the command service
      var result = Program._commands.Search(Context, command);

      //On command not found
      if (!result.IsSuccess)
      {

        //Create and set embed object
        var embed = CoreModule.SimpleEmbed(
          Color.Red,
          "Command not found",
          $"A command like **{command}** couldn't be found.");

        //Reply embed
        await ReplyAsync("",false, embed.Build());

        return;
      }

      //Get prefix from guild settings
      string prefix = CoreModule.GetPrefix($"{Context.Guild.Id}.db");

      //Create and set embed object
      var builder = CoreModule.SimpleEmbed(
        Color.Orange,
        "Bot Commands",
        $"Here are some commands like **{command}**");

      //For each command match on the command search
      foreach (var match in result.Commands)
      {

        //Create and set command
        var cmd = match.Command;

        //Add field to embed
        builder.AddField(x =>
        {

          //Set name
          x.Name = string.Join(", ", cmd.Aliases);

          //Set description
          x.Value = $"**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                    $"**Summary:** {cmd.Summary}";

          //Field not inline
          x.IsInline = true;
        });
      }

      //Reply embed
      await ReplyAsync("", false, builder.Build());
    }
  }
}
