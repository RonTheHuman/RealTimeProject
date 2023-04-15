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
        public class User
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }

            public User(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }

        }

        public class Match
        {
            public int Id { get; set; }
            public string StartTime { get; set; }
            public string Players { get; set; }
            public string Winner { get; set; }
            
            public string[] GetProperyArray()
            {
                return new string[3] { StartTime, Players, Winner };
            }
        }

        private static string LoadConnectionString()
        {
            return @"Data Source=.\GameDB.db";
        }

        public static void AddUser(User user)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO Users (UserName, Password) VALUES (@UserName, @Password)", user);
            }
        }

        public static List<string> GetUserNames()
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("SELECT UserName FROM Users");
                return output.ToList();
            }
        }

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

        public static void AddMatch(Match match)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("INSERT INTO Matches (StartTime, Players, Winner) VALUES (@StartTime, @Players, @Winner)", match);
            }
        }

        public static List<Match> GetAllMatches()
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Match>("SELECT * FROM Matches ORDER BY StartTime");
                return output.ToList();
            }
        }

        public static List<Match> GetMatchesWithUser(string userName)
        {
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Match>("SELECT * FROM Matches WHERE instr(Players, '" + userName + "') > 0 ORDER BY StartTime");
                return output.ToList();
            }
        }
    }
}
