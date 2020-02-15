using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

using System.Data.SQLite;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;

namespace VGXPBotCore {
  /// <summary>
  /// Class which contains the core functionality for the bot to operate.
  /// </summary>
  class Program
  {
    /// <summary>
    /// Global <c>DiscordSocketClient</c> member variable.
    /// </summary>
    public static DiscordSocketClient g_client;

    /// <summary>
    /// Global <c>CommandService</c> member variable.
    /// </summary>
    public static CommandService g_commands;

    /// <summary>
    /// Global <c>IServiceProvider</c> member variable.
    /// </summary>
    private IServiceProvider g_services;

    /// <summary>
    /// Private <c>readonly string</c> token member variable.
    /// </summary>
    private readonly string m_token = GetBotToken("token.txt");

    /// <summary>
    /// Main function to run the program.
    /// </summary>
    /// <param name="args">The array of arguments.</param>
    public static void
    Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronous main <c>Task</c>.
    /// </summary>
    public async Task
    MainAsync()
    {
      //Create the servers database.
      Modules.CoreModule.createServersDB();

      //Create discord socket client.
      g_client = new DiscordSocketClient();

      //Create command service.
      g_commands = new CommandService();

      //Create service provider.
      g_services = ConfigureServices();

      //Add Log Event to client.
      g_client.Log += Log;

      //Add JoinedGuild Event to client.
      g_client.JoinedGuild += JoinedGuild;

      //Add LeftGuild Event to client.
      g_client.LeftGuild += LeftGuild;

      //Add UserLeft Event to client.
      g_client.UserLeft += UserLeft;

      //Add Ready Event to client.
      g_client.Ready += Ready;

      //Set client game.
      await g_client.SetGameAsync("~help for commands");

      //Install the commands.
      await InstallCommandsAsync();

      //Login client with token.
      await g_client.LoginAsync(TokenType.Bot, m_token);

      //Start client connection.
      await g_client.StartAsync();

      //Block this task until program is closed.
      await Task.Delay(-1);
    }

    /// <summary>
    /// Gets the bot token.
    /// </summary>
    /// <param name="_fileName">The file which contains the bot token.</param>
    /// <returns>The token string.</returns>
    private static string
    GetBotToken(string _fileName)
    {
      //Create and set the token bot file.
      StreamReader readFile = new StreamReader(_fileName);

      //Create string token.
      string token;

      //Read token on file.
      token = readFile.ReadLine();

      //Close connection.
      readFile.Close();

      //Return token string.
      return token;
    }

    /// <summary>
    /// Installs the commands on the command service member.
    /// </summary>
    public async Task
    InstallCommandsAsync()
    {
      //Add MessageReceived Event to client.
      g_client.MessageReceived += HandleCommandAsync;

      //Add the modules which contains the commands to command service.
      await g_commands.AddModulesAsync(Assembly.GetEntryAssembly(), g_services);
    }

    /// <summary>
    /// Handles the command on the received message.
    /// </summary>
    /// <param name="_messageParam">The received message.</param>
    private async Task
    HandleCommandAsync(SocketMessage _messageParam)
    {
      // Don't process the command if it was a System Message.
      if (!(_messageParam is SocketUserMessage message)) {
        return;
      }

      //Create a number to track where the prefix ends and the command begins.
      int argPos = 0;

      //Create a Command Context.
      var context = new SocketCommandContext(g_client, message);

      //Create prefix string.
      string prefix = "";

      //On database file exists.
      if (File.Exists($"Databases/{context.Guild.Id}.db")) {
        //Get the prefix from the guild settings.
        prefix = Modules.CoreModule.GetPrefix(context.Guild.Id);
      }

      //Determine if the message is a command with prefix and if the author is not a bot.
      if (!(message.HasStringPrefix(prefix, ref argPos) ||
            message.HasCharPrefix('~', ref argPos) ||
            message.HasMentionPrefix(g_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot) {
        return;
      }

      //Execute the command. (result does not indicate a return value, 
      //rather an object stating if the command executed successfully)
      var result = await g_commands.ExecuteAsync(context, argPos, g_services);
      if (!result.IsSuccess) {
        //Create embed object
        var embed = Modules.CoreModule.SimpleEmbed(Color.Red, "Error", result.ErrorReason);

        //Reply embed
        await context.Channel.SendMessageAsync("", false, embed.Build());
      }
    }

    /// <summary>
    /// Creates the service provider.
    /// </summary>
    /// <returns>The collection of services built.</returns>
    private IServiceProvider
    ConfigureServices()
    {
      //Return the collection of services built for the service provider.
      return new ServiceCollection().AddSingleton(g_client).
                                     AddSingleton(g_commands).
                                     AddSingleton<InteractiveService>().
                                     BuildServiceProvider();
    }

    /// <summary>
    /// Task which logs the message on console.
    /// </summary>
    /// <param name="_msg">The message to log.</param>
    /// <returns>The task as completed.</returns>
    private Task
    Log(LogMessage _msg)
    {
      //Write on console.
      Console.WriteLine(_msg.ToString());

      //Return task as completed.
      return Task.CompletedTask;
    }

    /// <summary>
    /// Task which creates the server database the bot joined.
    /// </summary>
    /// <param name="_server">The server the bot joined.</param>
    /// <returns>The task as completed.</returns>
    private Task
    JoinedGuild(SocketGuild _server)
    {
      //Create the server database.
      Modules.CoreModule.CreateServerDB(_server.Id);

      //Add server to database and check in.
      Modules.CoreModule.ExecuteQuery("servers", 
                                      "INSERT INTO servers (server_id, status) values" +
                                        $"({_server.Id}, 'In');");

      //Create embed object.
      var embed = Modules.CoreModule.SimpleEmbed(Color.Gold,
                                                 $"Thank you for adding me to your server!",
                                                 $"This is my default configuration:\n" +
                                                   $"**`~`** - This is my default prefix.\n" +
                                                   $"**`Not set`** - The guild role has to" +
                                                   $" be **set by you**.\n" +
                                                   $"**`Off`** -  The notifications are" +
                                                   $" **Off** by default.\n" +
                                                   $"**`Not set`** - The notification's" +
                                                   $" channel has to be **set by you**.\n\n" +
                                                   $"You can get more info about" +
                                                   $" my commands using the" +
                                                   $" **`~help`** command.\n" +
                                                   $"(NOTE: The **`~help`** command shows" +
                                                   $" the ones who the user can use based" +
                                                   $" on it's permissions on the server," +
                                                   $" most administration commands require" +
                                                   $" the **`kick members`** permission.)");

      //Send message to default channel.
      _server.DefaultChannel.SendMessageAsync("", false, embed.Build());

      //Return task as completed.
      return Task.CompletedTask;
    }

    /// <summary>
    /// Task which deletes the server database the bot left.
    /// </summary>
    /// <param name="_server">The server the bot left.</param>
    /// <returns>The task as completed.</returns>
    private Task
    LeftGuild(SocketGuild _server)
    {
      //Delete database file.
      Modules.CoreModule.DeleteServerDB(_server.Id);

      //Execute query.
      Modules.CoreModule.ExecuteQuery("servers",
                                      $"DELETE FROM servers WHERE server_id = {_server.Id};");

      //Return task as completed.
      return Task.CompletedTask;
    }

    /// <summary>
    /// Task which deletes a user from the server database it left.
    /// </summary>
    /// <param name="_user">The user who left the server.</param>
    /// <returns>The task as completed.</returns>
    private Task
    UserLeft(SocketGuildUser _user)
    {
      //On user exists on server database.
      if (Modules.CoreModule.UserExistsServerDB(_user.Guild.Id, _user.Id)) {
        //Execute query.
        Modules.CoreModule.ExecuteQuery(_user.Guild.Id,
                                        $"DELETE FROM users WHERE id = {_user.Id};");

        //Send notification.
        Modules.CoreModule.SendNotification(_user.Guild.Id,
                                            "User deleted",
                                            $"{_user.Mention} **left** the server," +
                                              $" {_user.Mention} has been **deleted**" +
                                              $" from the database.");
      }

      //Return task as completed.
      return Task.CompletedTask;
    }

    /// <summary>
    /// Task which deletes any users who left a server and servers which the bot left while 
    /// it was offline.
    /// </summary>
    /// <returns>The task as completed.</returns>
    private Task
    Ready()
    {
      //Update the check status of servers on the servers database.
      Modules.CoreModule.ExecuteQuery("servers", $"UPDATE servers SET status = 'Out';");

      //Create and set the Guild iterator.
      var guild = g_client.Guilds.GetEnumerator();

      //While there's a server, delete the users and check in the server.
      while (guild.MoveNext()) {
        //Create and set socket user.
        SocketUser socketUser = null;

        //Create and set the users to delete list.
        List<string> usersToDelete = new List<string>();

        //On database exists, check for missing users on server.
        if (File.Exists($"Databases/{guild.Current.Id}.db")) {
          //Update the check status of server on database.
          Modules.CoreModule.ExecuteQuery("servers",
                                          $"UPDATE servers SET status = 'In' WHERE" +
                                            $" server_id = {guild.Current.Id};");

          //Create and set the database connection.
          using (SQLiteConnection dbConnection =
                 new SQLiteConnection($"Data Source = Databases/{guild.Current.Id}.db;" +
                                        $" Version = 3;")) {
            //Open the connection.
            dbConnection.Open();

            //Set query.
            using (SQLiteCommand dbCommand = new SQLiteCommand("SELECT id, name FROM users ", 
                                                               dbConnection)) {
              //Create and set the database reader from the command query.
              using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
                //Read users info.
                while (dbDataReader.Read()) {
                  socketUser = guild.
                               Current.
                               Users.
                               FirstOrDefault(x => x.Id == 
                                              Convert.ToUInt64(dbDataReader["id"]));

                  //On user not found.
                  if (socketUser == null) {
                    //Add user to the list.
                    usersToDelete.Add($"{dbDataReader["id"]}");
                  }
                }
              }
            }
          }
        }
        //On database doesn't exists, send thank you message to new server.
        else {
          //Create the server database.
          Modules.CoreModule.CreateServerDB(guild.Current.Id);

          //Create embed object.
          var embed = Modules.CoreModule.SimpleEmbed(Color.Gold,
                                                     $"Thank you for adding me" +
                                                       $" to your server!",
                                                     $"Seems I was added while" +
                                                       $" I was sleeping, this is" +
                                                       $" my default configuration:\n" +
                                                       $"**`~`** - This is" +
                                                       $" my default prefix.\n" +
                                                       $"**`Not set`** - The guild role" +
                                                       $" has to be **set by you**.\n" +
                                                       $"**`Off`** -  The notifications are" +
                                                       $" **Off** by default.\n" +
                                                       $"**`Not set`** - The notification's" +
                                                       $" channel has to be" +
                                                       $" **set by you**.\n\n" +
                                                       $"You can get more info about" +
                                                       $" my commands using the" +
                                                       $" **`~help`** command.\n" +
                                                       $"(NOTE: The **`~help`**" +
                                                       $" command shows the ones who" +
                                                       $" the user can use based on it's" +
                                                       $" permissions on the server," +
                                                       $" most administration commands" +
                                                       $" require the **`kick members`**" +
                                                       $" permission.)");

          //Send message to default channel.
          guild.Current.DefaultChannel.SendMessageAsync("", false, embed.Build());

          //Add server to database and check in.
          Modules.CoreModule.ExecuteQuery("servers",
                                          "INSERT INTO servers (server_id, status) values" +
                                            $"({guild.Current.Id}, 'In');");
        }

        //On users to delete.
        if (usersToDelete.Count != 0) {
          //Create embed object.
          var embed = Modules.CoreModule.SimpleEmbed(Color.Gold,
                                                     $"Users who left the server when" +
                                                       $" I was away",
                                                     $"These users where deleted from" +
                                                       $" the database, since they're not" +
                                                       $" on the server.");

          //Loop the list of users to delete.
          for (int i = 0; i < usersToDelete.Count; ++i) {
            //Add embed field.
            embed.AddField($"{i + 1}", $"<@{usersToDelete[i]}>", true);

            //Execute query.
            Modules.CoreModule.ExecuteQuery(guild.Current.Id,
              $"DELETE FROM users WHERE id = {usersToDelete[i]};");
          }

          //Send message to default channel.
          guild.Current.DefaultChannel.SendMessageAsync("@everyone", false, embed.Build());
        }
      }

      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/servers.db; Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Set query.
        using (SQLiteCommand dbCommand =
               new SQLiteCommand("SELECT server_id, status FROM servers;", dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read servers info.
            while (dbDataReader.Read()) {
              //On server not checked in.
              if ($"{dbDataReader["status"]}" == "Out") {
                //Delete server database.
                Modules.
                CoreModule.
                DeleteServerDB(Convert.ToUInt64($"{dbDataReader["server_id"]}"));
              }
            }
          }
        }
      }

      //Execute query.
      Modules.CoreModule.ExecuteQuery("servers",
                                      $"DELETE FROM servers WHERE status = 'Out';");

      //Return task as completed.
      return Task.CompletedTask;
    }
  }
}