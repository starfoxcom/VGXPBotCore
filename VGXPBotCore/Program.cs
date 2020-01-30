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

namespace VGXPBotCore
{

  class Program
  {
    //Define discord socket client
    public static DiscordSocketClient _client;

    //Define command service
    public static CommandService _commands;

    //Define service provider
    private IServiceProvider _services;

    //Bot token
    string token = GetToken("token.txt");

    //TODO: UserLeftOnAbsence function

    public static void Main(string[] argss)
      => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {

      //Set socket client
      _client = new DiscordSocketClient();

      //Set command service
      _commands = new CommandService();

      //configure services
      _services = ConfigureServices();

      //Set client log
      _client.Log += Log;

      //Set Client Joined Guild
      _client.JoinedGuild += JoinedGuild;

      //Set Client Left Guild
      _client.LeftGuild += LeftGuild;

      //Set user left
      _client.UserLeft += UserLeft;

      //Set Client ready
      _client.Ready += Ready;

      //Set game
      await _client.SetGameAsync("~help for commands");
      //await _client.SetGameAsync("On development");

      //Install the commands
      await InstallCommandsAsync();

      //Set login with bot token
      await _client.LoginAsync(TokenType.Bot, token);

      //Start connection
      await _client.StartAsync();

      //Block this task until program is closed
      await Task.Delay(-1);
    }

    public static string GetToken(string _file)
    {

      //Create & set the token bot file
      StreamReader readFile = new StreamReader(_file);

      //Create string reader
      string line;

      //Read token
      line = readFile.ReadLine();
      //Console.WriteLine("token: " + line);

      //Close connection
      readFile.Close();

      return line;
    }

    public async Task InstallCommandsAsync()
    {

      //Hook the MessageReceived Event into our Command Handler
      _client.MessageReceived += HandleCommandAsync;

      //Discover all of the commands in this assembly and load them.
      await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {

      // Don't process the command if it was a System Message
      var message = messageParam as SocketUserMessage;
      if (message == null) return;

      //Create a number to track where the prefix ends and the command begins
      int argPos = 0;

      //Create a Command Context
      var context = new SocketCommandContext(_client, message);

      //Create prefix string
      string prefix = "";

      //On database file exists
      if (File.Exists($"Databases/{context.Guild.Id}.db"))
      {

        //Get the prefix from the guild settings
        prefix = Modules.CoreModule.GetPrefix(context.Guild.Id);
      }

      //Determine if the message is a command with prefix
      if (!(File.Exists($"Databases/{context.Guild.Id}.db") ? 
        message.HasStringPrefix(prefix, ref argPos) : 
        message.HasCharPrefix('~', ref argPos) ||
        message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

      //Execute the command. (result does not indicate a return value, 
      //rather an object stating if the command executed successfully)
      var result = await _commands.ExecuteAsync(context, argPos, _services);
      if (!result.IsSuccess)
      {

        var embed = Modules.CoreModule.SimpleEmbed(
          Color.Red,
          "Error",
          result.ErrorReason);

        await context.Channel.SendMessageAsync("", false, embed.Build());
      }
    }

    private IServiceProvider ConfigureServices()
    {
      return new ServiceCollection()
        .AddSingleton(_client)
        .AddSingleton(_commands)
        .AddSingleton<InteractiveService>()
        .BuildServiceProvider();
      ;
    }

    //Logging method
    private Task Log(LogMessage msg)
    {

      //Write on console
      Console.WriteLine(msg.ToString());

      //Return task as completed
      return Task.CompletedTask;
    }

    //Joined Guild method
    private Task JoinedGuild(SocketGuild server)
    {

      //Create the server database
      Modules.CoreModule.CreateDB(server.Id);

      //Add server to database and check in
      Modules.CoreModule.ExecuteQuery("servers",
        "INSERT INTO servers " +
        "(server_id, status) values" +
        $"({server.Id}, 'In');");

      var embed = Modules.CoreModule.SimpleEmbed(
            Color.Gold,
            $"Thank you for adding me to your server!",
            $"This is my default configuration:\n" +
            $"**`~`** - This is my default prefix.\n" +
            $"**`Not set`** - The guild role has to be **set by you**.\n" +
            $"**`Off`** -  The notifications are **Off** by default.\n" +
            $"**`Not set`** - The  notification's channel has to be **set by you**.\n\n" +
            $"You can get more info about my commands using the **`~help`** command.\n" +
            $"(NOTE: The **`~help`** command shows the ones who the user can use based on it's " +
            $"permissions on the server, most administration commands require the **`kick members`** permission.)");

      //Send message to channel
      server.DefaultChannel.SendMessageAsync("", false, embed.Build());

      //Return task as completed
      return Task.CompletedTask;
    }

    //Left Guild method
    private Task LeftGuild(SocketGuild server)
    {

      //Delete database file
      Modules.CoreModule.DeleteDB(server.Id);

      //Execute query
      Modules.CoreModule.ExecuteQuery("servers",
        $"DELETE FROM servers WHERE server_id = {server.Id};");

      //Return task as completed
      return Task.CompletedTask;
    }

    //User left Guild method
    private Task UserLeft(SocketGuildUser user)
    {

      if(Modules.CoreModule.UserDBExists(user.Guild.Id, user.Id))
      {

        //Execute query
        Modules.CoreModule.ExecuteQuery(user.Guild.Id,
          $"DELETE FROM users WHERE id = {user.Id};");

        //Send notification
        Modules.CoreModule.SendNotification(
          user.Guild.Id,
          "User deleted",
          $"{user.Mention} **left** the server, {user.Mention} has been **deleted** from the database.");
      }

      return Task.CompletedTask;
    }

    //Client ready
    private Task Ready()
    {

      //Update the check status of servers on database
      Modules.CoreModule.ExecuteQuery("servers", $"UPDATE servers SET status = 'Out';");

      //Create and set the Guild iterator
      var guild = _client.Guilds.GetEnumerator();

      //While iterator move next
      while(guild.MoveNext())
      {

        //Create and set socket user
        SocketUser socketUser = null;

        //Create and set the users to delete list
        List<string> usersToDelete = new List<string>();

        //On database exists, check for missing users on server
        if (File.Exists($"Databases/{guild.Current.Id}.db"))
        {

          //Update the check status of server on database
          Modules.CoreModule.ExecuteQuery("servers",
            $"UPDATE servers SET status = 'In' WHERE server_id = {guild.Current.Id};");

          //Create and set the database connection
          using (SQLiteConnection dbConnection =
            new SQLiteConnection($"Data Source = Databases/{guild.Current.Id}.db; Version = 3;"))
          {
            //Open the connection
            dbConnection.Open();

            //Set query
            using (SQLiteCommand dbCommand =
              new SQLiteCommand("SELECT id, name FROM users ", dbConnection))
            {

              //Create and set the database reader from the command query
              using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
              {

                //Read users info
                while (dbDataReader.Read())
                {
                  socketUser = guild.Current.Users.FirstOrDefault(
                    x => x.Id == Convert.ToUInt64(dbDataReader["id"]));

                  //On user not found
                  if (socketUser == null)
                  {

                    //Add user to the list
                    usersToDelete.Add($"{dbDataReader["id"]}");
                  }
                }
              }
            }
          }
        }

        //On database doesn't exists, send thank you message to new server
        else
        {

          //Create the server database
          Modules.CoreModule.CreateDB(guild.Current.Id);

          var embed = Modules.CoreModule.SimpleEmbed(
            Color.Gold,
            $"Thank you for adding me to your server!",
            $"Seems I was added while I was sleeping, this is my default configuration:\n" +
            $"**`~`** - This is my default prefix.\n" +
            $"**`Not set`** - The guild role has to be **set by you**.\n" +
            $"**`Off`** -  The notifications are **Off** by default.\n" +
            $"**`Not set`** - The  notification's channel has to be **set by you**.\n\n" +
            $"You can get more info about my commands using the **`~help`** command.\n" +
            $"(NOTE: The **`~help`** command shows the ones who the user can use based on it's " +
            $"permissions on the server, most administration commands require the **`kick members`** permission.)");

          //Send message to channel
          guild.Current.DefaultChannel.SendMessageAsync("", false, embed.Build());

          //Add server to database and check in
          Modules.CoreModule.ExecuteQuery("servers",
            "INSERT INTO servers " +
            "(server_id, status) values" +
            $"({guild.Current.Id}, 'In');");
        }

        //On users to delete
        if (usersToDelete.Count != 0)
        {
          var embed = Modules.CoreModule.SimpleEmbed(
            Color.Gold,
            $"Users who left the server when I was away",
            $"These users where deleted from the database, since they're not on the server.");

          for (int i = 0; i < usersToDelete.Count; ++i)
          {

            //Set embed content
            embed.AddField($"{i + 1}", $"<@{usersToDelete[i]}>", true);

            //Execute query
            Modules.CoreModule.ExecuteQuery(guild.Current.Id,
              $"DELETE FROM users WHERE id = {usersToDelete[i]};");
          }

          //Send message to channel
          guild.Current.DefaultChannel.SendMessageAsync("@everyone", false, embed.Build());
        }

      }
      
      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/servers.db; Version = 3;"))
      {
        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("SELECT server_id, status FROM servers;", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read servers info
            while (dbDataReader.Read())
            {
              
              //On server not checked in
              if($"{dbDataReader["status"]}" == "Out")
              {

                //Delete server database
                Modules.CoreModule.DeleteDB(Convert.ToUInt64($"{dbDataReader["server_id"]}"));
              }
            }
          }
        }
      }

      //Execute query
      Modules.CoreModule.ExecuteQuery("servers",
        $"DELETE FROM servers WHERE status = 'Out';");

      return Task.CompletedTask;
    }
  }
}
