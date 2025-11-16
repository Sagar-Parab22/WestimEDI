using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNRService.Helpers
{
    public static class SqlHelper
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["LogStar.GenCare.DatabaseConnection"].ConnectionString;
        }

        #region New Methods for SQL Query Files

        // Gets the base path for the queries folder
        private static string GetQueryBasePath()
        {
            string queryPath = ConfigurationManager.AppSettings["QueryFolderPath"];

            if (string.IsNullOrEmpty(queryPath))
            {
                // Log an error and throw an exception if the setting is missing
                string errorMsg = "FATAL ERROR: App.config setting 'QueryFolderPath' is missing or empty.";
                // Check if Writefile is accessible; might need adjustment if static context is an issue
                try { MNREDIService.Writefile(errorMsg); } catch { Console.Error.WriteLine(errorMsg); } // Log the error
                throw new ConfigurationErrorsException(errorMsg);
            }

            if (!Directory.Exists(queryPath))
            {
                // Log an error and throw an exception if the folder doesn't exist
                string errorMsg = $"FATAL ERROR: The specified QueryFolderPath does not exist: {queryPath}";
                // Check if Writefile is accessible
                try { MNREDIService.Writefile(errorMsg); } catch { Console.Error.WriteLine(errorMsg); } // Log the error
                throw new DirectoryNotFoundException(errorMsg);
            }

            // --- Added Log Line ---
            // Log the path that was successfully retrieved and validated
            string successMsg = $"Using QueryFolderPath from App.config: {queryPath}";
            try { MNREDIService.Writefile(successMsg); } catch { Console.Out.WriteLine(successMsg); } // Log success
            // --- End Added Log Line ---

            return queryPath; // Return the path from App.config
        }

        // Reads the SQL text from a file
        private static string GetQueryText(string queryFileName)
        {
            string filePath = Path.Combine(GetQueryBasePath(), queryFileName);
            if (!File.Exists(filePath))
            {
                MNREDIService.Writefile($"FATAL ERROR: SQL query file not found: {filePath}");
                throw new FileNotFoundException($"SQL query file not found: {queryFileName}", filePath);
            }
            return File.ReadAllText(filePath);
        }

        // Executes a text query that doesn't return a value
        public static int ExecuteNonQueryText(string queryFileName, params MySqlParameter[] parameters)
        {
            string queryText = GetQueryText(queryFileName);

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(queryText, conn))
            {
                cmd.CommandType = CommandType.Text; // Use CommandType.Text
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // Executes a text query that returns a SqlDataReader
        public static MySqlDataReader ExecuteReaderText(string queryFileName, params MySqlParameter[] parameters)
        {
            string queryText = GetQueryText(queryFileName);
            var conn = new MySqlConnection(GetConnectionString());

            using (var cmd = new MySqlCommand(queryText, conn))
            {
                cmd.CommandType = CommandType.Text; // Use CommandType.Text
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        // Executes a text query that returns an output parameter
        public static string ExecuteNonQueryOutputResultText(string queryFileName, string outputParamName, params MySqlParameter[] parameters)
        {
            string queryText = GetQueryText(queryFileName);

            using (var conn = new MySqlConnection(GetConnectionString()))
            using (var cmd = new MySqlCommand(queryText, conn))
            {
                cmd.CommandType = CommandType.Text; // Use CommandType.Text
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                cmd.ExecuteNonQuery();

                foreach (MySqlParameter param in cmd.Parameters)
                {
                    if (param.Direction == ParameterDirection.Output && param.ParameterName.Equals(outputParamName, StringComparison.OrdinalIgnoreCase))
                    {
                        return param.Value?.ToString();
                    }
                }
                return null;
            }
        }
        #endregion
    }

    public static class DBConstants
    {
        // From ClsMNREDI
        public const string PROC_MNR_GETEDILOGID = "PROC_MNR_GETEDILOGID";
        public const string PROC_MNR_GETEDIACTIVITYDETAILS = "PROC_MNR_GETEDIACTIVITYDETAILS";
        public const string PROC_MNR_MANAGEEDISTATUS = "PROC_MNR_MANAGEEDISTATUS";
        public const string PROC_MNR_MANAGINGEDIDETAILS = "PROC_MNR_MANAGINGEDIDETAILS";
        public const string PROC_MNR_GET_MSK_EDIACTIVITYDETAILS = "PROC_MNR_GET_MSK_EDIACTIVITYDETAILS";
        public const string PROC_MNR_GET_GetDEMAGEIMAGEFOREDI = "PROC_MNR_GET_GetDEMAGEIMAGEFOREDI";

        // From ClsMNREDI.CreateEmailStructure
        public const string PROC_MNR_Insert_EmailStructure = "PROC_MNR_Insert_EmailStructure";
    }
}
