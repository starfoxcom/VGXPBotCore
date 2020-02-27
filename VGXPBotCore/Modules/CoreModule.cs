using System;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;

using Newtonsoft.Json;

using System.Data.SQLite;

namespace VGXPBotCore.Modules {
  /// <summary>
  /// Class which contains the core functions for the bot commands to operate.
  /// </summary>
  public static class CoreModule
  {
    /// <summary>
    /// Creates the servers database.
    /// </summary>
    public static void
    CreateServersDB()
    {
      //In case that the database file don't exist.
      if (!File.Exists($"Databases/servers.db")) {
        //In case that the database folder don't exist.
        if (!Directory.Exists("Databases")) {
          //Create directory.
          Directory.CreateDirectory("Databases");
        }

        //Create Database file.
        using (File.Create($"Databases/servers.db")) {}

        //Create and set the database connection.
        using (SQLiteConnection dbConnection =
               new SQLiteConnection($"Data Source = Databases/servers.db; Version = 3;")) {
          //Open the connection.
          dbConnection.Open();

          //Create all the tables needed for the database.
          using (SQLiteCommand dbCommand = new SQLiteCommand("CREATE TABLE servers (" +
                                                               "server_id integer NOT NULL," +
                                                               "status text NOT NULL);",
                                                             dbConnection)) {
            //Execute the query.
            dbCommand.ExecuteNonQuery();
          }
        }
      }
    }
    
    /// <summary>
    /// Creates a simple embed object.
    /// </summary>
    /// <param name="_color">The stripe color of the embed bject.</param>
    /// <param name="_author">The title of the embed object.</param>
    /// <param name="_description">The paragraph content of the embed object.</param>
    /// <returns>The embed object.</returns>
    public static EmbedBuilder
    SimpleEmbed(Color _color, string _author, string _description)
    {
      //Create and set embed object.
      var embed = new EmbedBuilder {
        //Set stripe color.
        Color = _color
      };

      //Set embed content.
      embed.WithAuthor(_author, Program.g_client.CurrentUser.GetAvatarUrl()).
            WithDescription(_description);

      //Return embed object.
      return embed;
    }

    /// <summary>
    /// Checks if a user exists on a server database.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    /// <param name="_userId">The ID of the user.</param>
    /// <returns>true if user exists on the server database, otherwise false</returns>
    public static bool
    UserExistsServerDB(ulong _serverId, ulong _userId)
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Set query.
        using (SQLiteCommand dbCommand =
               new SQLiteCommand($"SELECT id FROM users WHERE id = {_userId};",
                                 dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read user.
            dbDataReader.Read();

            //On user found, return true.
            if(dbDataReader.StepCount > 0) {
              return true;
            }
            return false;
          }
        }
      }
    }

    /// <summary>
    /// Creates the server database.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    public static void
    CreateServerDB(ulong _serverId)
    {
      //In case that the database file don't exist.
      if (!File.Exists($"Databases/{_serverId}.db")) {
        //In case that the database folder don't exist.
        if(!Directory.Exists("Databases")) {
          //Create directory.
          Directory.CreateDirectory("Databases");
        }

        //Create Database file.
        using (File.Create($"Databases/{_serverId}.db")) {}

        //Create and set the database connection.
        using (SQLiteConnection dbConnection =
               new SQLiteConnection($"Data Source = Databases/{_serverId}.db;" +
                                      $" Version = 3;")) { 
          //Open the connection.
          dbConnection.Open();

          //Create all the tables needed for the database.
          using (SQLiteCommand dbCommand = new SQLiteCommand("CREATE TABLE settings (" +
                                                               "prefix text NOT NULL," +
                                                               " role text NOT NULL," +
                                                               " notifications text" +
                                                               " NOT NULL," +
                                                               " notificationChannel text" +
                                                               " NOT NULL);" +
                                                               "CREATE TABLE users (" +
                                                               "id integer NOT NULL," +
                                                               " name text NOT NULL," +
                                                               " region text NOT NULL," +
                                                               " actualXP integer NOT NULL," +
                                                               " lastXP integer NOT NULL);" +
                                                               "INSERT INTO settings" +
                                                               " (prefix, role," +
                                                               " notifications," +
                                                               " notificationChannel)" +
                                                               " values('~', 'not set'," +
                                                               " 'Off', 'not set');",
                                                             dbConnection)) {
            //Execute the query.
            dbCommand.ExecuteNonQuery();
          }
        }
      }
    }

    /// <summary>
    /// Executes a SQLite query on a server database.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    /// <param name="_query">The query to execute.</param>
    public static void
    ExecuteQuery(ulong _serverId, string _query)
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand = new SQLiteCommand(_query, dbConnection)) {
          //Execute the query.
          dbCommand.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Executes a SQLite query on a database.
    /// </summary>
    /// <param name="_databaseNameFile">The file name of the database.</param>
    /// <param name="_query">The query to execute.</param>
    public static void
    ExecuteQuery(string _databaseNameFile, string _query)
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{_databaseNameFile}.db;" +
             $" Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand = new SQLiteCommand(_query, dbConnection)) {
          //Execute the query.
          dbCommand.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Gets the server bot prefix.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    /// <returns>The bot prefix of the server.</returns>
    public static string GetPrefix(ulong _serverId)
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand = new SQLiteCommand("Select prefix FROM" +
                                                             " settings LIMIT 1",
                                                           dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read settings info.
            dbDataReader.Read();

            //Return the server prefix.
            return $"{dbDataReader["prefix"]}";
          }
        }
      }
    }

    /// <summary>
    /// Gets the guild member role.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    /// <param name="_context">The context of the command.</param>
    /// <returns>The guild member role.</returns>
    public static SocketRole GetRole(ulong _serverId, SocketCommandContext _context)
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand =
               new SQLiteCommand("SELECT role FROM settings LIMIT 1;", dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read role.
            dbDataReader.Read();

            //Return role.
            return _context.
                   Guild.
                   Roles.
                   FirstOrDefault(x => x.Name == $"{dbDataReader["role"]}");
          }
        }
      }
    }

    /// <summary>
    /// Deletes the server database.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    public static void
    DeleteServerDB(ulong _serverId)
    {
      //On database exists, delete it.
      if(File.Exists($"Databases/{_serverId}.db")) {
        File.Delete($"Databases/{_serverId}.db");
      }
    }

    /// <summary>
    /// Sends a notification message.
    /// </summary>
    /// <param name="_serverId">The ID of the server.</param>
    /// <param name="_author">The title of the embed object.</param>
    /// <param name="_description">The paragraph content of the embed object.</param>
    public static void
    SendNotification(ulong _serverId, string _author, string _description)
    {
      //Create and set the database connection.
      using (SQLiteConnection dbConnection =
             new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;")) {
        //Open the connection.
        dbConnection.Open();

        //Create and set query.
        using (SQLiteCommand dbCommand =
               new SQLiteCommand("SELECT notifications, notificationChannel" +
                                   " FROM settings LIMIT 1;",
                                 dbConnection)) {
          //Create and set the database reader from the command query.
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader()) {
            //Read settings info.
            dbDataReader.Read();

            //Create and set notification flag.
            string notificationFlag = $"{dbDataReader["notifications"]}";

            //On notifications activated.
            if (notificationFlag == "On") {
              //On server has text channel.
              if (Program.
                  g_client.
                  GetGuild(_serverId).
                  Channels.
                  FirstOrDefault(x => x.Name == $"{dbDataReader["notificationChannel"]}") is 
                  SocketTextChannel channel) {
                //Create and set embed object.
                var embed = SimpleEmbed(Color.Gold, _author, _description);

                //Set the current time stamp.
                embed.WithCurrentTimestamp();

                //Send message to notification channel.
                Program.
                g_client.
                GetGuild(_serverId).
                GetTextChannel(channel.Id).
                SendMessageAsync("", false, embed.Build());
              }
              //On no text channel found.
              else {
                //Create and set embed object
                var embed = SimpleEmbed(Color.Gold,
                                        "Channel not found",
                                        "Notifications are `On`, but the **channel** to" +
                                          " send them is not found or is not set," +
                                          " please **set** the **notification channel**" +
                                          " with **`~setchannel`**.");

                //Send message to default channel.
                Program.
                g_client.
                GetGuild(_serverId).
                DefaultChannel.
                SendMessageAsync("", false, embed.Build());
              }
            }
          }
        }
      }
    }
  }
}