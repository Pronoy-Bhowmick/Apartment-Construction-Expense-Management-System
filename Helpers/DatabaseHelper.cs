using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace ApartmentWinForms.Helpers
{
    public static class DatabaseHelper
    {
        private static readonly string _connectionString;

        static DatabaseHelper()
        {
            _connectionString = LoadConnectionString();
        }

        private static string LoadConnectionString()
        {
            try
            {
                // .env ফাইলের পাথ খুঁজে বের করা
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, ".env");
                }

                if (!File.Exists(filePath))
                {
                    // ফাইল না পাওয়া গেলে ডিফল্ট লোকাল কানেকশন
                    return "Server=localhost\\SQLEXPRESS;Database=ApartmentExpenseDB;Trusted_Connection=True;TrustServerCertificate=True;";
                }

                // ডট-এনভ ফাইলের ডাটা রিড করা
                var env = new Dictionary<string, string>();
                foreach (var line in File.ReadAllLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2) env[parts[0].Trim()] = parts[1].Trim();
                }

                // ENVIRONMENT চেক করা (DEV নাকি PROD)
                string currentEnv = env.ContainsKey("ENVIRONMENT") ? env["ENVIRONMENT"] : "DEV";

                if (currentEnv.ToUpper() == "DEV")
                {
                    string server = env.ContainsKey("DB_SERVER_DEV") ? env["DB_SERVER_DEV"] : "localhost\\SQLEXPRESS";
                    string database = env.ContainsKey("DB_NAME_DEV") ? env["DB_NAME_DEV"] : "ApartmentExpenseDB";
                    return $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;";
                }
                else
                {
                    string server = env["DB_SERVER"];
                    string database = env["DB_NAME"];
                    string user = env["DB_USER"];
                    string pass = env["DB_PASSWORD"];
                    return $"Server={server};Database={database};User Id={user};Password={pass};TrustServerCertificate=True;";
                }
            }
            catch (Exception)
            {
                return "Server=localhost\\SQLEXPRESS;Database=ApartmentExpenseDB;Trusted_Connection=True;TrustServerCertificate=True;";
            }
        }

        // অন্যান্য ফাইল থেকে কানেকশন নেওয়ার মেইন মেথড
        public static SqlConnection GetSqlConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
