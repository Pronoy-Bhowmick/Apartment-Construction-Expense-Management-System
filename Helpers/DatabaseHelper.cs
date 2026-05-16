using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.SqlClient; // অথবা System.Data.SqlClient

namespace ApartmentWinForms.Helpers
{
    public static class DatabaseHelper
    {
        private static string _connectionString;

        // .env ফাইল থেকে ডেটা পড়ার জন্য এবং কানেকশন স্ট্রিং তৈরি করার মেথড
        static DatabaseHelper()
        {
            LoadConnectionString();
        }

        private static void LoadConnectionString()
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
                    throw new FileNotFoundException(".env ফাইলটি খুঁজে পাওয়া যায়নি!");
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
                    string server = env["DB_SERVER_DEV"];
                    string database = env["DB_NAME_DEV"];
                    // Windows Authentication কানেকশন স্ট্রিং
                    _connectionString = $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;";
                }
                else
                {
                    string server = env["DB_SERVER"];
                    string database = env["DB_NAME"];
                    string user = env["DB_USER"];
                    string pass = env["DB_PASSWORD"];
                    // Production সার্ভার কানেকশন স্ট্রিং
                    _connectionString = $"Server={server};Database={database};User Id={user};Password={pass};TrustServerCertificate=True;";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DatabaseHelper initialization failed: " + ex.Message);
            }
        }

        // এই মেথডটি দিয়ে অন্যান্য সার্ভিস বা ফর্ম থেকে কানেকশন অবজেক্ট পাওয়া যাবে
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
