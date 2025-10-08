using System;
using System.Collections.Generic;
using System.Text;

namespace budoco
{
    /// <summary>
    /// Provides cross-database schema creation and migration functionality
    /// </summary>
    public static class bd_schema_builder
    {
        /// <summary>
        /// Generates a CREATE TABLE statement with cross-database compatibility
        /// </summary>
        public static string BuildCreateTable(string tableName, Dictionary<string, ColumnDefinition> columns, List<string> indexes = null)
        {
            var provider = DatabaseProviderFactory.GetProvider();
            var sql = new StringBuilder();
            
            sql.AppendLine($"CREATE TABLE {tableName} (");
            
            var columnDefinitions = new List<string>();
            foreach (var column in columns)
            {
                columnDefinitions.Add(BuildColumnDefinition(column.Key, column.Value, provider));
            }
            
            sql.AppendLine(string.Join(",\n", columnDefinitions));
            sql.AppendLine(");");
            
            // Add indexes
            if (indexes != null)
            {
                foreach (var index in indexes)
                {
                    sql.AppendLine(index);
                }
            }
            
            return sql.ToString();
        }
        
        /// <summary>
        /// Builds a column definition for the specified database provider
        /// </summary>
        private static string BuildColumnDefinition(string columnName, ColumnDefinition column, IDatabaseProvider provider)
        {
            var definition = new StringBuilder();
            definition.Append($"    {columnName} ");
            
            // Handle data type
            switch (column.DataType.ToLower())
            {
                case "serial":
                    definition.Append(provider.GetAutoIncrementType());
                    if (column.IsPrimaryKey)
                        definition.Append(" PRIMARY KEY");
                    break;
                case "boolean":
                    definition.Append(provider.GetBooleanType());
                    break;
                case "timestamptz":
                    definition.Append(provider.GetTimestampType());
                    break;
                case "varchar":
                    definition.Append($"VARCHAR({column.Length})");
                    break;
                case "int":
                    definition.Append("INT");
                    break;
                case "text":
                    definition.Append("TEXT");
                    break;
                default:
                    definition.Append(column.DataType);
                    break;
            }
            
            // Handle constraints
            if (column.IsNotNull)
                definition.Append(" NOT NULL");
                
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                if (column.DefaultValue == "CURRENT_TIMESTAMP")
                    definition.Append($" DEFAULT {bd_sql_builder.GetCurrentTimestamp()}");
                else if (column.DefaultValue == "true" || column.DefaultValue == "false")
                    definition.Append($" DEFAULT {bd_sql_builder.GetBooleanLiteral(column.DefaultValue == "true")}");
                else
                    definition.Append($" DEFAULT {column.DefaultValue}");
            }
            
            return definition.ToString();
        }
        
        /// <summary>
        /// Generates a DROP TABLE statement with IF EXISTS support
        /// </summary>
        public static string BuildDropTable(string tableName)
        {
            var provider = DatabaseProviderFactory.GetProvider();
            
            if (provider is SqlServerProvider)
            {
                return $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE {tableName};";
            }
            else
            {
                return $"DROP TABLE IF EXISTS {tableName};";
            }
        }
        
        /// <summary>
        /// Generates a CREATE INDEX statement
        /// </summary>
        public static string BuildCreateIndex(string indexName, string tableName, string[] columns, bool isUnique = false)
        {
            var uniqueKeyword = isUnique ? "UNIQUE " : "";
            var columnList = string.Join(", ", columns);
            return $"CREATE {uniqueKeyword}INDEX {indexName} ON {tableName} ({columnList});";
        }
    }
    
    /// <summary>
    /// Represents a column definition for table creation
    /// </summary>
    public class ColumnDefinition
    {
        public string DataType { get; set; }
        public int Length { get; set; }
        public bool IsNotNull { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string DefaultValue { get; set; }
        
        public ColumnDefinition(string dataType, int length = 0, bool isNotNull = false, bool isPrimaryKey = false, string defaultValue = null)
        {
            DataType = dataType;
            Length = length;
            IsNotNull = isNotNull;
            IsPrimaryKey = isPrimaryKey;
            DefaultValue = defaultValue;
        }
    }
}