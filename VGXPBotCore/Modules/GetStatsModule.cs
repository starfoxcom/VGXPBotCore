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
  public class GetStatsModule : InteractiveBase
  {

    [Command("stats")]
    [Summary("`Shows` the stats of the user.")]
    [Alias("s")]
    public async Task GetStatsAsync()
    {

      //Create and set socket guild user
      SocketGuildUser user = Context.User as SocketGuildUser;

      //Create and set role
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On socket guild user contains guild member role
      if (user.Roles.Contains(role))
      {

        //On user on database
        if (CoreModule.UserDBExists(Context.Guild.Id, user.Id))
        {

          //Create and set the database connection
          using (SQLiteConnection dbConnection =
            new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db; Version = 3;"))
          {

            //Open the connection
            dbConnection.Open();

            //Set query
            using (SQLiteCommand dbCommand =
              new SQLiteCommand("SELECT name, region, actualXP, lastXP, (actualXP - lastXP) as totalXP FROM users " +
              $"WHERE id = {user.Id};", dbConnection))
            {

              //Create and set the database reader from the command query
              using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
              {

                //Read user stats info
                dbDataReader.Read();

                //Create and set embed object
                var embed = CoreModule.SimpleEmbed(
                  Color.Blue,
                  $"{user.Username} stats",
                  null);

                //Add embed fields
                embed
                  .AddField("Vainglory username", $"**`{dbDataReader["name"]}`**", true)
                  .AddField("Region", $"**`{dbDataReader["region"]}`**", true)
                  .AddField("Last XP fame", $"**`{dbDataReader["lastXP"]}`**", true)
                  .AddField("Actual XP fame", $"**`{dbDataReader["actualXP"]}`**", true)
                  .AddField("Total XP fame", $"**`{dbDataReader["totalXP"]}`**", true);

                //Add user profile
                embed
                  .WithThumbnailUrl(user.GetAvatarUrl());

                //Reply embed
                await ReplyAsync("", false, embed.Build());
              }
            }
          }
        }

        //On user not on database
        else
        {

          //Create & set embed content
          var embed = CoreModule.SimpleEmbed(
          Color.Red,
          "User not found",
          $"{user.Mention} doesn't exist on the database, **`stats aborted`**.");

          //Reply embed
          await ReplyAsync("", false, embed.Build());
        }
      }

      //On socket guild user not contains role
      else
      {

        //Create embed object
        EmbedBuilder embed;

        //On role exist
        if (role != null)
        {

          //Set embed object
          embed = CoreModule.SimpleEmbed(
            Color.Red,
            "Not a member",
            $"{user.Mention} you're not allowed to use this command, " +
            $"only {role.Mention} is allowed to use it.");
        }

        //Otherwise role don't exist
        else
        {

          //Set embed object
          embed = CoreModule.SimpleEmbed(
            Color.Red,
            "Role not set",
            "The **role** to use the commands has **not been set**, please use `~setrole` " +
            "to set the role, **`stats aborted`**.");
        }

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("stats")]
    [Summary("`Shows` the stats of the specified user.")]
    [Alias("s")]
    public async Task GetStatsAsync([Remainder]SocketGuildUser user)
    {

      //Create and set role
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On socket guild user contains guild member role
      if (user.Roles.Contains(role))
      {

        //On user on database
        if (CoreModule.UserDBExists(Context.Guild.Id, user.Id))
        {

          //Create and set the database connection
          using (SQLiteConnection dbConnection =
            new SQLiteConnection($"Data Source = Databases/{Context.Guild.Id}.db; Version = 3;"))
          {

            //Open the connection
            dbConnection.Open();

            //Set query
            using (SQLiteCommand dbCommand =
              new SQLiteCommand("SELECT name, region, actualXP, lastXP, (actualXP - lastXP) as totalXP FROM users " +
              $"WHERE id = {user.Id};", dbConnection))
            {

              //Create and set the database reader from the command query
              using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
              {

                //Read user stats info
                dbDataReader.Read();

                //Create and set embed object
                var embed = CoreModule.SimpleEmbed(
                  Color.Blue,
                  $"{user.Username} stats",
                  null);

                //Add embed fields
                embed
                  .AddField("Vainglory username", $"**`{dbDataReader["name"]}`**", true)
                  .AddField("Region", $"**`{dbDataReader["region"]}`**", true)
                  .AddField("Last XP fame", $"**`{dbDataReader["lastXP"]}`**", true)
                  .AddField("Actual XP fame", $"**`{dbDataReader["actualXP"]}`**", true)
                  .AddField("Total XP fame", $"**`{dbDataReader["totalXP"]}`**", true);

                //Add user profile
                embed
                  .WithThumbnailUrl(user.GetAvatarUrl());

                //Reply embed
                await ReplyAsync("", false, embed.Build());
              }
            }
          }
        }

        //On user not on database
        else
        {

          //Create & set embed content
          var embed = CoreModule.SimpleEmbed(
          Color.Red,
          "User not found",
          $"{user.Mention} doesn't exist on the database, **`stats aborted`**.");

          //Reply embed
          await ReplyAsync("", false, embed.Build());
        }
      }

      //On socket guild user not contains role
      else
      {

        //Create embed object
        EmbedBuilder embed;

        //On role exist
        if (role != null)
        {

          //Set embed object
          embed = CoreModule.SimpleEmbed(
            Color.Red,
            "Not a member",
            $"{user.Mention} doesn't have the {role.Mention} role, **`stats aborted`**.");
        }

        //Otherwise role don't exist
        else
        {

          //Set embed object
          embed = CoreModule.SimpleEmbed(
            Color.Red,
            "Role not set",
            "The **role** to use the commands has **not been set**, please use `~setrole` " +
            "to set the role, **`stats aborted`**.");
        }

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
