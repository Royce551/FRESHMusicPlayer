using FRESHMusicPlayer.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
namespace FRESHMusicPlayer.Handlers
{
    static class DatabaseHandler
    {
        public static readonly int DatabaseVersion = 1;
        public static readonly string DatabasePath;
        static DatabaseHandler()
        {
            DatabasePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FRESHMusicPlayer";
        }
        /// <summary>
        /// Returns all of the tracks in the database.
        /// </summary>
        /// <returns>A list of file paths in the database.</returns>
        public static List<string> ReadSongs()
        {
            if (!File.Exists(DatabasePath + "\\database.json"))
            {
                Directory.CreateDirectory(DatabasePath);
                File.WriteAllText(DatabasePath + "\\database.json", $"{{\"Version\":{DatabaseVersion},\"Songs\":[]}}");
            }
            using (StreamReader file = File.OpenText(DatabasePath + "\\database.json")) // Read json file
            {
                JsonSerializer serializer = new JsonSerializer();
                DatabaseFormat database = (DatabaseFormat)serializer.Deserialize(file, typeof(DatabaseFormat));
                return database.Songs;
            }
        }

        public static void ImportSong(string filepath)
        {
            List<string> ExistingSongs;

            List<string> database = ReadSongs();
            ExistingSongs = database; // Add the existing songs to a list to use later

            ExistingSongs.Add(filepath); // Add the new song in
            DatabaseFormat format = new DatabaseFormat();
            format.Version = 1;
            format.Songs = new List<string>();
            format.Songs = ExistingSongs;

            using (StreamWriter file = File.CreateText(DatabasePath + "\\database.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, format);
            }

        }
        public static void ImportSong(string[] filepath)
        {
            List<string> ExistingSongs;

            List<string> database = ReadSongs();
            ExistingSongs = database; // Add the existing songs to a list to use later

            ExistingSongs.AddRange(filepath);
            DatabaseFormat format = new DatabaseFormat();
            format.Version = 1;
            format.Songs = new List<string>();
            format.Songs = ExistingSongs;

            using (StreamWriter file = File.CreateText(DatabasePath + "\\database.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, format);
            }

        }
        public static void ImportSong(List<string> filepath)
        {
            List<string> ExistingSongs;

            List<string> database = ReadSongs();
            ExistingSongs = database; // Add the existing songs to a list to use later

            ExistingSongs.AddRange(filepath);
            DatabaseFormat format = new DatabaseFormat();
            format.Version = 1;
            format.Songs = new List<string>();
            format.Songs = ExistingSongs;

            using (StreamWriter file = File.CreateText(DatabasePath + "\\database.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, format);
            }

        }
        public static void ImportSong(IList<string> filepath)
        {
            List<string> ExistingSongs;

            List<string> database = ReadSongs();
            ExistingSongs = database; // Add the existing songs to a list to use later

            ExistingSongs.AddRange(filepath);
            DatabaseFormat format = new DatabaseFormat();
            format.Version = 1;
            format.Songs = new List<string>();
            format.Songs = ExistingSongs;

            using (StreamWriter file = File.CreateText(DatabasePath + "\\database.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, format);
            }

        }
        public static void DeleteSong(string filepath)
        {
            List<string> database = ReadSongs();
            database.Remove(filepath);
            DatabaseFormat format = new DatabaseFormat();
            format.Version = 1;
            format.Songs = database;
            

            using (StreamWriter file = File.CreateText(DatabasePath + "\\database.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, format);
            }
        }
        public static void ClearLibrary()
        {
            if (File.Exists(DatabasePath + "\\database.json"))
            {
                File.Delete(DatabasePath + "\\database.json");
                File.WriteAllText(DatabasePath + "\\database.json", @"{""Version"":1,""Songs"":[]}");
            }
        }
    }

}
