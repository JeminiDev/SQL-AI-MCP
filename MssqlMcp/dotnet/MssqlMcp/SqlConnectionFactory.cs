// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Data.SqlClient;

namespace Mssql.McpServer;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public async Task<SqlConnection> GetOpenConnectionAsync()
    {
        var connectionString = GetConnectionString();

        // Let ADO.Net handle connection pooling
        var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        return conn;
    }

    private static string GetConnectionString()
    {
        // First, check if a full connection string is provided
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Otherwise, build connection string from individual environment variables
        var authType = Environment.GetEnvironmentVariable("AUTH_TYPE") ?? "azure-ad";
        var serverName = Environment.GetEnvironmentVariable("SERVER_NAME");
        var serverPort = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "1433";
        var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
        var trustServerCertificate = Environment.GetEnvironmentVariable("TRUST_SERVER_CERTIFICATE")?.ToLower() == "true";
        var connectionTimeout = Environment.GetEnvironmentVariable("CONNECTION_TIMEOUT") ?? "30";

        if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException(
                "Database connection not configured. Either set CONNECTION_STRING or provide SERVER_NAME and DATABASE_NAME environment variables.\n\n" +
                "Option 1: Set CONNECTION_STRING environment variable\n" +
                "Example: CONNECTION_STRING=Server=.;Database=test;Trusted_Connection=True;TrustServerCertificate=True\n\n" +
                "Option 2: Set individual environment variables\n" +
                "- AUTH_TYPE=sql (or azure-ad)\n" +
                "- SERVER_NAME=localhost\n" +
                "- SERVER_PORT=1433 (optional)\n" +
                "- DATABASE_NAME=test\n" +
                "- SQL_USERNAME=sa (for AUTH_TYPE=sql)\n" +
                "- SQL_PASSWORD=yourPassword (for AUTH_TYPE=sql)");
        }

        // Build server string with port
        var server = serverPort != "1433" ? $"{serverName},{serverPort}" : serverName;

        if (authType.ToLower() == "sql")
        {
            // SQL Authentication
            var username = Environment.GetEnvironmentVariable("SQL_USERNAME");
            var password = Environment.GetEnvironmentVariable("SQL_PASSWORD");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("SQL_USERNAME and SQL_PASSWORD must be set when using AUTH_TYPE=sql");
            }

            return $"Server={server};Database={databaseName};User Id={username};Password={password};" +
                   $"TrustServerCertificate={trustServerCertificate};Connection Timeout={connectionTimeout};";
        }
        else
        {
            // Azure AD or Windows Authentication
            return $"Server={server};Database={databaseName};Integrated Security=True;" +
                   $"TrustServerCertificate={trustServerCertificate};Connection Timeout={connectionTimeout};";
        }
    }
}
