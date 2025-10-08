using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using Microsoft.Data.SqlClient;

namespace budoco
{
    public enum DatabaseType
    {
        PostgreSQL,
        SqlServer
    }

    public interface IDatabaseProvider
    {
        DbConnection CreateConnection();
        DbCommand CreateCommand(DbConnection connection, string sql, Dictionary<string, dynamic> parameters = null);
        DbDataAdapter CreateDataAdapter(DbCommand command);
        string GetConnectionString();
        DatabaseType DatabaseType { get; }
        string FormatParameterName(string parameterName);
        string GetLastInsertIdSql(string tableName, string idColumn = "id");
        string GetLimitSql(int limit, int offset = 0);
        string GetAutoIncrementType();
        string GetBooleanType();
        string GetTimestampType();
    }

    public class PostgreSQLProvider : IDatabaseProvider
    {
        public DatabaseType DatabaseType => DatabaseType.PostgreSQL;

        public DbConnection CreateConnection()
        {
            return new NpgsqlConnection(GetConnectionString());
        }

        public DbCommand CreateCommand(DbConnection connection, string sql, Dictionary<string, dynamic> parameters = null)
        {
            connection.Open();
            var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)connection);
            
            if (parameters != null)
            {
                foreach (var pair in parameters)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
                }
            }
            
            return cmd;
        }

        public DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new NpgsqlDataAdapter((NpgsqlCommand)command);
        }

        public string GetConnectionString()
        {
            return bd_config.get_connection_string();
        }

        public string FormatParameterName(string parameterName)
        {
            return parameterName.StartsWith("@") ? parameterName : "@" + parameterName;
        }

        public string GetLastInsertIdSql(string tableName, string idColumn = "id")
        {
            return $"SELECT currval(pg_get_serial_sequence('{tableName}', '{idColumn}'))";
        }

        public string GetLimitSql(int limit, int offset = 0)
        {
            return $" LIMIT {limit} OFFSET {offset}";
        }

        public string GetAutoIncrementType()
        {
            return "SERIAL";
        }

        public string GetBooleanType()
        {
            return "BOOLEAN";
        }

        public string GetTimestampType()
        {
            return "TIMESTAMPTZ";
        }
    }

    public class SqlServerProvider : IDatabaseProvider
    {
        public DatabaseType DatabaseType => DatabaseType.SqlServer;

        public DbConnection CreateConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        public DbCommand CreateCommand(DbConnection connection, string sql, Dictionary<string, dynamic> parameters = null)
        {
            connection.Open();
            var cmd = new SqlCommand(sql, (SqlConnection)connection);
            
            if (parameters != null)
            {
                foreach (var pair in parameters)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
                }
            }
            
            return cmd;
        }

        public DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }

        public string GetConnectionString()
        {
            return bd_config.get_connection_string();
        }

        public string FormatParameterName(string parameterName)
        {
            return parameterName.StartsWith("@") ? parameterName : "@" + parameterName;
        }

        public string GetLastInsertIdSql(string tableName, string idColumn = "id")
        {
            return "SELECT SCOPE_IDENTITY()";
        }

        public string GetLimitSql(int limit, int offset = 0)
        {
            return $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
        }

        public string GetAutoIncrementType()
        {
            return "INT IDENTITY(1,1)";
        }

        public string GetBooleanType()
        {
            return "BIT";
        }

        public string GetTimestampType()
        {
            return "DATETIME2";
        }
    }

    public static class DatabaseProviderFactory
    {
        private static IDatabaseProvider _provider;

        public static IDatabaseProvider GetProvider()
        {
            if (_provider == null)
            {
                string dbType;
                try
                {
                    // Try to get from new configuration system first
                    dbType = bd_config.get_database_type();
                }
                catch
                {
                    // Fall back to old configuration system
                    dbType = bd_config.get(bd_config.DbType);
                }
                
                switch (dbType?.ToLower())
                {
                    case "sqlserver":
                    case "sql server":
                    case "mssql":
                        _provider = new SqlServerProvider();
                        break;
                    case "postgresql":
                    case "postgres":
                    case "pg":
                    default:
                        _provider = new PostgreSQLProvider();
                        break;
                }
            }
            
            return _provider;
        }

        public static void ResetProvider()
        {
            _provider = null;
        }
    }
}