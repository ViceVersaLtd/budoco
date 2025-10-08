using System;
using System.Collections.Generic;
using System.Text;

namespace budoco
{
    public static class bd_sql_builder
    {
        private static IDatabaseProvider GetProvider()
        {
            return DatabaseProviderFactory.GetProvider();
        }

        /// <summary>
        /// Generates an INSERT statement that returns the inserted ID
        /// </summary>
        public static string BuildInsertWithReturnId(string tableName, Dictionary<string, dynamic> columns, string idColumn = "id")
        {
            var provider = GetProvider();
            var columnNames = new List<string>();
            var parameterNames = new List<string>();

            foreach (var column in columns.Keys)
            {
                columnNames.Add(column);
                parameterNames.Add(provider.FormatParameterName(column));
            }

            var sql = new StringBuilder();
            sql.Append($"INSERT INTO {tableName} ");
            sql.Append($"({string.Join(", ", columnNames)}) ");
            sql.Append($"VALUES ({string.Join(", ", parameterNames)})");

            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                sql.Append($" RETURNING {idColumn}");
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                sql.Append($"; {provider.GetLastInsertIdSql(tableName, idColumn)}");
            }

            return sql.ToString();
        }

        /// <summary>
        /// Generates a SELECT statement with LIMIT/OFFSET for pagination
        /// </summary>
        public static string BuildSelectWithLimit(string baseQuery, int limit, int offset = 0)
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                // SQL Server requires ORDER BY for OFFSET/FETCH
                if (!baseQuery.ToUpper().Contains("ORDER BY"))
                {
                    baseQuery += " ORDER BY (SELECT NULL)";
                }
            }

            return baseQuery + provider.GetLimitSql(limit, offset);
        }

        /// <summary>
        /// Converts PostgreSQL ILIKE to appropriate case-insensitive search for the target database
        /// </summary>
        public static string ConvertILike(string column, string parameter)
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                return $"{column} ILIKE {parameter}";
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                return $"LOWER({column}) LIKE LOWER({parameter})";
            }

            return $"{column} LIKE {parameter}";
        }

        /// <summary>
        /// Gets the appropriate boolean literal for the database
        /// </summary>
        public static string GetBooleanLiteral(bool value)
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                return value ? "true" : "false";
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                return value ? "1" : "0";
            }

            return value ? "1" : "0";
        }

        /// <summary>
        /// Gets the appropriate current timestamp function for the database
        /// </summary>
        public static string GetCurrentTimestamp()
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                return "CURRENT_TIMESTAMP";
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                return "GETDATE()";
            }

            return "CURRENT_TIMESTAMP";
        }

        /// <summary>
        /// Gets the appropriate data type for auto-incrementing primary keys
        /// </summary>
        public static string GetAutoIncrementType()
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                return "SERIAL";
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                return "INT IDENTITY(1,1)";
            }

            return "SERIAL";
        }

        /// <summary>
        /// Gets the appropriate boolean data type
        /// </summary>
        public static string GetBooleanType()
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                return "BOOLEAN";
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                return "BIT";
            }

            return "BOOLEAN";
        }

        /// <summary>
        /// Gets the appropriate timestamp with timezone data type
        /// </summary>
        public static string GetTimestampType()
        {
            var provider = GetProvider();
            
            if (provider.DatabaseType == DatabaseType.PostgreSQL)
            {
                return "TIMESTAMPTZ";
            }
            else if (provider.DatabaseType == DatabaseType.SqlServer)
            {
                return "DATETIMEOFFSET";
            }

            return "TIMESTAMP";
        }
    }
}