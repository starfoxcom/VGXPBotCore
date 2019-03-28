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

    //Create a simple embed object
    public static EmbedBuilder SimpleEmbed(
      Color _color,
      string _author,
      string _description,
      string _iconUrl)
    {

      //Create & set embed object
      var embed = new EmbedBuilder();

      //Set stripe color
      embed.Color = _color;

      //Set embed content
      embed
        .WithAuthor(_author, _iconUrl)
        .WithDescription(_description);

      return embed;
    }

    //Create Server Database
    public static void CreateDB(
      string _serverId)
    {

      //In case that the database file don't exist
      if (!File.Exists("Databases/"+_serverId))
      {

        //In case that the database folder don't exist
        if(!Directory.Exists("Databases"))
        {

          //Create directory
          Directory.CreateDirectory("Databases");
        }

        //Create Database file
        using (var stream = File.Create("Databases/" + _serverId)) { }

        //Create and set the database connection
        SQLiteConnection sQLiteConnection =
          new SQLiteConnection($"Data Source = Databases/{_serverId}; Version = 3;");

        //Open the connection
        sQLiteConnection.Open();

        //Create all the tables needed for the database
        SQLiteCommand sQLiteCommand = new SQLiteCommand(
          "CREATE TABLE settings (" +
          "role text NOT NULL, " +
          "notifications integer NOT NULL, " +
          "notificationChannelId integer NOT NULL" +
          ");" +
          "CREATE TABLE users (" +
          "id integer NOT NULL, " +
          "name text NOT NULL, " +
          "region text NOT NULL, " +
          "actualXP integer NOT NULL, " +
          "lastXP integer NOT NULL" +
          ");", sQLiteConnection);

        //Execute the query
        sQLiteCommand.ExecuteNonQuery();

        //Close the connection
        sQLiteConnection.Close();
      }
    }

    public static void DeleteDB(string _serverId)
    {

      //Delete the specified file
      File.Delete("Databases/" + _serverId);
    }
  }
}