using System;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

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
    private CommandService _commands;

    //Define service provider
    private IServiceProvider _services;

    //Bot token
    string token = GetToken("token.txt");

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

      //Determine if the message is a command with prefix
      if (!(message.HasCharPrefix('~', ref argPos) ||
        message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

      //Create a Command Context
      var context = new SocketCommandContext(_client, message);

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
      Modules.CoreModule.CreateDB($"{server.Id}.db");

      //Return task as completed
      return Task.CompletedTask;
    }

    //Left Guild method
    private Task LeftGuild(SocketGuild server)
    {

      //Delete the server database
      Modules.CoreModule.DeleteDB($"{server.Id}.db");
      
      //Return task as completed
      return Task.CompletedTask;
    }
  }
}
