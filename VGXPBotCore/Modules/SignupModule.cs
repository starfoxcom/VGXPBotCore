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
  public class SignupModule : InteractiveBase
  {

    [Command("signup")]
    [Summary("`registers` the user to the bot database.")]
    [Alias("snup")]
    public async Task SignupAsync(string username, string region)
    {

      //Create and set socket guild user
      SocketGuildUser user = Context.User as SocketGuildUser;

      //Create and set role
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);

      //On socket guild user contains guild member role
      if (user.Roles.Contains(role))
      {

        //On user not on database
        if (!CoreModule.UserExistsServerDB(Context.Guild.Id, user.Id))
        {

          //On username exists
          //if (CoreModule.UserVGExists(username, region)) - Not working since VGAPI is not longer available
          {

            //Add user to database
            CoreModule.ExecuteQuery(Context.Guild.Id,
              "INSERT INTO users (" +
                "id," +
                "name," +
                "region," +
                "actualXP," +
                "lastXP" +
                ") VALUES (" +
                $"{user.Id}," +
                $"'{username}'," +
                $"'{region}'," +
                $"0," +
                $"0" +
                $");");

            //Create and set embed content
            var embed = CoreModule.SimpleEmbed(
            Color.Green,
            "Sign up completed",
            $"Your **sign up** is **completed**.");

            //Reply embed
            await ReplyAsync("", false, embed.Build());

            //Send notification
            CoreModule.SendNotification(
              Context.Guild.Id,
              "User registered",
              $"{user.Mention} **registered** to the database.");
          }

          //On username don't exist - Not working since VGAPI is not longer available
          //else
          //{

          //  //Create and set embed content
          //  var embed = CoreModule.SimpleEmbed(
          //  Color.Red,
          //  "Vainglory player not found",
          //  "The Vainglory username that you typed wasn't found, " +
          //    "please check the **`spell`** or the **`region`** and try again. **`Sign up aborted.`**");

          //  //Reply embed
          //  await ReplyAsync("", false, embed.Build());
          //}
        }

        //On user in database
        else
        {

          //Create and set embed object
          var embed = CoreModule.SimpleEmbed(
            Color.Red,
            "User found",
            "You're already on the database, **`sign up aborted`**.");

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
            "to set the role, **`sign up aborted`**.");
        }

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }

    [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    [Command("signup")]
    [Summary("`registers` the specified user to the bot database.")]
    [Alias("snup")]
    public async Task SignupAsync(string username, string region, [Remainder]SocketGuildUser user)
    {

      //Create and set role
      SocketRole role = CoreModule.GetRole(Context.Guild.Id, Context);
      
      //On socket guild user contains guild member role
      if (user.Roles.Contains(role))
      {

        //On user not on database
        if (!CoreModule.UserExistsServerDB(Context.Guild.Id, user.Id))
        {

          //On username exists
          //if (CoreModule.UserVGExists(username, region)) - Not working since VGAPI is not longer available
          {

            //Add user to database
            CoreModule.ExecuteQuery(Context.Guild.Id,
              "INSERT INTO users (" +
                "id," +
                "name," +
                "region," +
                "actualXP," +
                "lastXP" +
                ") VALUES (" +
                $"{user.Id}," +
                $"'{username}'," +
                $"'{region}'," +
                $"0," +
                $"0" +
                $");");

            //Create and set embed content
            var embed = CoreModule.SimpleEmbed(
            Color.Green,
            "Sign up completed",
            $"The **sign up** of the user {user.Mention} is **completed**.");

            //Reply embed
            await ReplyAsync("", false, embed.Build());

            //Send notification
            CoreModule.SendNotification(
              Context.Guild.Id,
              "User registered",
              $"{Context.User.Mention} **registered** the user {user.Mention} to the database.");
          }

          //On username don't exist - Not working since VGAPI is not longer available
          //else
          //{

          //  //Create and set embed content
          //  var embed = CoreModule.SimpleEmbed(
          //  Color.Red,
          //  "Vainglory player not found",
          //  "The Vainglory username that you typed wasn't found, " +
          //    "please check the **spell** or the **region** and try again. **`Sign up aborted.`**");

          //  //Reply embed
          //  await ReplyAsync("", false, embed.Build());
          //}
        }

        //On user in database
        else
        {

          //Create and set embed object
          var embed = CoreModule.SimpleEmbed(
            Color.Red,
            "User found",
            "User **already** on the database, **`sign up aborted`**.");

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
            $"{user.Mention} doesn't have the {role.Mention} role, **`sign up aborted`**.");
        }

        //Otherwise role don't exist
        else
        {

          //Set embed object
          embed = CoreModule.SimpleEmbed(
            Color.Red,
            "Role not set",
            "The **role** to use the commands has **not been set**, please use **`~setrole`** " +
            "to set the role, **`sign up aborted`**.");
        }

        //Reply embed
        await ReplyAsync("", false, embed.Build());
      }
    }
  }
}
