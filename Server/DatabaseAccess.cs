using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RealTimeProject
{
    internal class DatabaseAccess
    {

        private static string LoadConnectionString()
        {
            return @"Data Source=.\GameDB.db";
        }
        // Adds user to database.
        public static void AddUser(User user)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO Users (UserName, Password) VALUES (@UserName, @Password)", user);
            }
        }
        // Gets a list of all users in the database.
        public static List<string> GetUserNames()
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT UserName FROM Users");
                return output.ToList();
            }
        }
        // Checks if a user name exists.
        public static bool CheckIfUserNameExists(string userName)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT UserName FROM Users WHERE UserName='" + userName + "'");
                if (output.Count() > 0)
                    return true;
                return false;
            }
        }
        // Checks if a user exists with a matching name and password.
        public static bool CheckIfUserExists(string userName, string password)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT UserName FROM Users WHERE UserName='" + userName + "' AND Password='" + password + "'");
                if (output.Count() > 0)
                    return true;
                return false;
            }
        }
        // Adds a match to database.
        public static void AddMatch(Match match)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO Matches (StartTime, Players, Winner, Length) VALUES (@StartTime, @Players, @Winner, @Length)", match);
            }
        }
        // Gets all matches from the database.
        public static List<Match> GetAllMatches()
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Match>("SELECT StartTime, Players, Winner, Length FROM Matches");
                output.OrderBy(m => DateTime.ParseExact(m.StartTime, "dd/MM/yyyy HH:mm", null));
                return output.ToList();
            }
        }
        // Gets all matches from the database where the specified user participated.
        public static List<Match> GetMatchesWithUser(string userName)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Match>("SELECT StartTime, Players, Winner, Length FROM Matches WHERE instr(Players, '" + userName + "') > 0");
                output.OrderBy(m => DateTime.ParseExact(m.StartTime, "dd/MM/yyyy HH:mm", null));
                return output.ToList();
            }
        }
    }
}
