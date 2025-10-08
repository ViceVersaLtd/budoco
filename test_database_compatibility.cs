using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace budoco
{
    /// <summary>
    /// Test script to verify cross-database compatibility of SQL operations
    /// </summary>
    public class DatabaseCompatibilityTest
    {
        public static void RunTests()
        {
            Console.WriteLine("=== Database Compatibility Test ===");
            
            // Test with current database provider
            TestCurrentProvider();
            
            Console.WriteLine("\n=== Test Complete ===");
        }
        
        private static void TestCurrentProvider()
        {
            var provider = DatabaseProviderFactory.GetProvider();
            Console.WriteLine($"Testing with provider: {provider.GetType().Name}");
            
            // Test SQL Builder methods
            TestSqlBuilder();
            
            // Test Schema Builder methods
            TestSchemaBuilder();
            
            // Test database operations
            TestDatabaseOperations();
        }
        
        private static void TestSqlBuilder()
        {
            Console.WriteLine("\n--- Testing SQL Builder ---");
            
            // Test SELECT with LIMIT
            var selectSql = bd_sql_builder.BuildSelectWithLimit(
                "SELECT * FROM users WHERE us_active = " + bd_sql_builder.GetBooleanLiteral(true),
                1
            );
            Console.WriteLine($"SELECT with LIMIT: {selectSql}");
            
            // Test INSERT with RETURN ID
            var columns = new Dictionary<string, dynamic>
            {
                { "us_username", "'testuser'" },
                { "us_email", "'test@example.com'" },
                { "us_active", bd_sql_builder.GetBooleanLiteral(true) }
            };
            var insertSql = bd_sql_builder.BuildInsertWithReturnId("users", columns, "us_id");
            Console.WriteLine($"INSERT with RETURN ID: {insertSql}");
            
            // Test boolean literals
            Console.WriteLine($"Boolean TRUE: {bd_sql_builder.GetBooleanLiteral(true)}");
            Console.WriteLine($"Boolean FALSE: {bd_sql_builder.GetBooleanLiteral(false)}");
            
            // Test current timestamp
            Console.WriteLine($"Current Timestamp: {bd_sql_builder.GetCurrentTimestamp()}");
        }
        
        private static void TestSchemaBuilder()
        {
            Console.WriteLine("\n--- Testing Schema Builder ---");
            
            var provider = DatabaseProviderFactory.GetProvider();
            
            // Test CREATE TABLE with different column types
            var columns = new Dictionary<string, ColumnDefinition>
            {
                { "test_id", new ColumnDefinition("AUTO_INCREMENT", 0, true, true) },
                { "is_active", new ColumnDefinition("BOOLEAN", 0, true, false, "true") },
                { "created_date", new ColumnDefinition("TIMESTAMP", 0, true, false, "CURRENT_TIMESTAMP") },
                { "name", new ColumnDefinition("VARCHAR", 255, true) }
            };
            
            var createTableSql = bd_schema_builder.BuildCreateTable("test_table", columns);
            Console.WriteLine($"CREATE TABLE SQL:\n{createTableSql}");
            
            // Test DROP TABLE
            var dropTableSql = bd_schema_builder.BuildDropTable("test_table");
            Console.WriteLine($"DROP TABLE SQL: {dropTableSql}");
            
            // Test CREATE INDEX
            var createIndexSql = bd_schema_builder.BuildCreateIndex("idx_test_name", "test_table", new[] { "name" }, false);
            Console.WriteLine($"CREATE INDEX SQL: {createIndexSql}");
        }
        
        private static void TestDatabaseOperations()
        {
            Console.WriteLine("\n--- Testing Database Operations ---");
            
            try
            {
                // Test connection using provider
                var provider = DatabaseProviderFactory.GetProvider();
                using (var connection = provider.CreateConnection())
                {
                    Console.WriteLine("✓ Database connection created successfully");
                    
                    // Test a simple query
                    var sql = bd_sql_builder.BuildSelectWithLimit(
                        "SELECT COUNT(*) as user_count FROM users",
                        1
                    );
                    
                    using (var command = provider.CreateCommand(connection, sql))
                    {
                        var result = command.ExecuteScalar();
                        Console.WriteLine($"✓ Query executed successfully. User count: {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Database operation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test method that can be called from a web page or console application
        /// </summary>
        public static string GetTestResults()
        {
            var originalOut = Console.Out;
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                RunTests();
                return stringWriter.ToString();
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}