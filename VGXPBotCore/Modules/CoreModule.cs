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

namespace VGXPBotCore.Modules
{
  public static class CoreModule
  {
    public static void createServersDB()
    {
      //In case that the database file don't exist
      if (!File.Exists($"Databases/servers.db"))
      {

        //In case that the database folder don't exist
        if (!Directory.Exists("Databases"))
        {

          //Create directory
          Directory.CreateDirectory("Databases");
        }

        //Create Database file
        using (File.Create($"Databases/servers.db")) { }

        //Create and set the database connection
        using (SQLiteConnection dbConnection =
          new SQLiteConnection($"Data Source = Databases/servers.db; Version = 3;"))
        {

          //Open the connection
          dbConnection.Open();

          //Create all the tables needed for the database
          using (SQLiteCommand dbCommand = new SQLiteCommand(
          "CREATE TABLE servers (" +
          "server_id integer NOT NULL," +
          "status text NOT NULL" +
          ");", dbConnection))
          {

            //Execute the query
            dbCommand.ExecuteNonQuery();
          }
        }
      }
    }
    //Create a simple embed object
    public static EmbedBuilder SimpleEmbed(
      Color _color,
      string _author,
      string _description)
    {

      //Create & set embed object
      var embed = new EmbedBuilder();

      //Set stripe color
      embed.Color = _color;

      //Set embed content
      embed
        .WithAuthor(_author, Program._client.CurrentUser.GetAvatarUrl())
        .WithDescription(_description);

      return embed;
    }

    //User exists on database
    public static bool UserDBExists(
      ulong _serverId,
      ulong _userId)
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand($"SELECT id FROM users WHERE id = {_userId};", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read users
            dbDataReader.Read();

            //On user found
            if(dbDataReader.StepCount > 0)
            {

              return true;
            }

            return false;
          }
        }
      }
    }

    //Username exists on Vainglory
    public static bool UserVGExists(
      string _username,
      string _region)
    {

      //Create & set found checker
      bool found = false;

      //Create & set socket URL
      string sURL = $"https://api.dc01.gamelockerapp.com/shards/{_region}/players?" +
        $"filter[playerNames]={_username}";

      //Create & set web request
      var WRGetURL = (HttpWebRequest)WebRequest.Create(sURL);

      //Set method
      WRGetURL.Method = "Get";

      //Set header with Authorization API Key
      WRGetURL.Headers.Add("Authorization", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9." +
        "eyJqdGkiOiJkZTMxMDM0MC1mOGI3LTAxMzQtYjdlYS0wMjQyYWMxMTAwMGIiLCJpc3MiOiJnY" +
        "W1lbG9ja2VyIiwiaWF0IjoxNDkxMDE2NzM3LCJwdWIiOiJzZW1jIiwidGl0bGUiOiJ2YWluZ2" +
        "xvcnkiLCJhcHAiOiJkZTJhZmRjMC1mOGI3LTAxMzQtYjdlOC0wMjQyYWMxMTAwMGIiLCJzY29" +
        "wZSI6ImNvbW11bml0eSIsImxpbWl0IjoxMH0.QxjHDvJEN-lO0KV9PyJZVprL4Zt6cV3awKjxZx3exzc");

      //Set accept
      WRGetURL.Accept = "application/vnd.api+json";

      HttpWebResponse response = null;

      try
      {

        //try get web response
        response = (HttpWebResponse)WRGetURL.GetResponse();
      }

      //In case that try fails
      catch (WebException e)
      {

        //On a protocol error
        if (e.Status == WebExceptionStatus.ProtocolError)
        {

          //Set the response
          response = (HttpWebResponse)e.Response;

          //Write the error code on console
          Console.Write("Errorcode: {0}\n", (int)response.StatusCode);
        }

        //Otherwise write the error on console
        else
        {

          Console.Write("Error: {0}\n", e.Status);
        }
      }
      finally
      {
        if (response != null)
        {

          //On Vainglory username not found
          if (response.StatusDescription == "Not Found" ||
            response.StatusDescription == "Internal Server Error")
          {
            found = false;
          }

          //Otherwise Vainglory username found
          else
          {
            found = true;
          }

          //Close the connection
          response.Close();
        }
      }

      //On not found
      if (found != true)
      {
        return false;
      }

      //Otherwise on found
      else
      {
        return true;
      }
    }

    //Create Server Database
    public static void CreateDB(
      ulong _serverId)
    {

      //In case that the database file don't exist
      if (!File.Exists($"Databases/{_serverId}.db"))
      {

        //In case that the database folder don't exist
        if(!Directory.Exists("Databases"))
        {

          //Create directory
          Directory.CreateDirectory("Databases");
        }

        //Create Database file
        using (File.Create($"Databases/{_serverId}.db")) { }

        //Create and set the database connection
        using (SQLiteConnection dbConnection =
          new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;"))
        { 

          //Open the connection
          dbConnection.Open();

          //Create all the tables needed for the database
          using (SQLiteCommand dbCommand = new SQLiteCommand(
          "CREATE TABLE settings (" +
          "prefix text NOT NULL," +
          "role text NOT NULL, " +
          "notifications text NOT NULL, " +
          "notificationChannel text NOT NULL" +
          ");" +
          "CREATE TABLE users (" +
          "id integer NOT NULL, " +
          "name text NOT NULL, " +
          "region text NOT NULL, " +
          "actualXP integer NOT NULL, " +
          "lastXP integer NOT NULL" +
          ");" +
          //WIP - participants table purpose is not well defined yet
          //"CREATE TABLE participants (" +
          //"id integer NOT NULL, " +
          //"payment integer NOT NULL" +
          //");" +
          "INSERT INTO settings " +
          "(prefix, role, notifications, notificationChannel) values" +
          "('~', 'not set', 'Off', 'not set');", dbConnection))
          {

            //Execute the query
            dbCommand.ExecuteNonQuery();
          }
        }
      }
    }

    //Execute query
    public static void ExecuteQuery(
      ulong _serverId,
      string _query)
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand(_query, dbConnection))
        {

          //Execute the query
          dbCommand.ExecuteNonQuery();
        }
      }
    }

    //Execute query
    public static void ExecuteQuery(
      string _databaseName,
      string _query)
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{_databaseName}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand(_query, dbConnection))
        {

          //Execute the query
          dbCommand.ExecuteNonQuery();
        }
      }
    }

    //Get guild prefix
    public static string GetPrefix(
      ulong _serverId)
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("Select prefix FROM settings LIMIT 1", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read settings info
            dbDataReader.Read();

            //Return prefix
            return $"{dbDataReader["prefix"]}";
          }
        }
      }
    }

    //Get guild member role
    public static SocketRole GetRole(
      ulong _serverId,
      SocketCommandContext _context)
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("SELECT role FROM settings LIMIT 1;", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read role
            dbDataReader.Read();

            //Return role
            return _context.Guild.Roles.FirstOrDefault(x => x.Name == $"{dbDataReader["role"]}");
          }
        }
      }
    }

    //Delete server Database
    public static void DeleteDB(
      ulong _serverId)
    {
      if(File.Exists($"Databases/{_serverId}.db"))
      {
        File.Delete($"Databases/{_serverId}.db");
      }
    }

    //Send notification
    public static void SendNotification(
      ulong _serverId,
      string _author,
      string _description)
    {

      //Create and set the database connection
      using (SQLiteConnection dbConnection =
        new SQLiteConnection($"Data Source = Databases/{_serverId}.db; Version = 3;"))
      {

        //Open the connection
        dbConnection.Open();

        //Set query
        using (SQLiteCommand dbCommand =
          new SQLiteCommand("SELECT notifications, notificationChannel FROM settings LIMIT 1;", dbConnection))
        {

          //Create and set the database reader from the command query
          using (SQLiteDataReader dbDataReader = dbCommand.ExecuteReader())
          {

            //Read settings info
            dbDataReader.Read();

            //Create and set socket role
            string notificationFlag = $"{dbDataReader["notifications"]}";

            //On notifications On
            if (notificationFlag == "On")
            {

              //Create and set socket text channel
              SocketTextChannel channel = 
                Program._client.GetGuild(
                  _serverId).Channels.FirstOrDefault(
                  x => x.Name == $"{dbDataReader["notificationChannel"]}") as SocketTextChannel;

              //On server has text channel
              if(channel != null)
              {

                //Create and set embed object
                var embed = SimpleEmbed(
                  Color.Gold,
                  _author,
                  _description);

                //Set the current time stamp
                embed.WithCurrentTimestamp();

                //Send message to channel
                Program._client.GetGuild(
                  _serverId).GetTextChannel(channel.Id).SendMessageAsync("", false, embed.Build());
              }

              //On no text channel found
              else
              {

                //Create and set embed object
                var embed = SimpleEmbed(
                  Color.Gold,
                  "Channel not found",
                  "Notifications are On, but the **channel** to send them is not found or is not set, " +
                  "please **set** the **notification channel **with** `~setchannel` command**.");

                //Send message to channel
                Program._client.GetGuild(
                  _serverId).DefaultChannel.SendMessageAsync("", false, embed.Build());
              }
            }
          }
        }
      }
    }
  }
}