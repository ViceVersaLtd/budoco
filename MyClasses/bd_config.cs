using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace budoco
{
    /*
    Budoco now uses the standard ASP.NET Core appsettings.json configuration system.
    
    This provides several benefits:
    * Environment-specific configuration (Development, Production, etc.)
    * Hierarchical configuration structure
    * Built-in support for connection strings
    * Integration with ASP.NET Core's dependency injection
    * Support for configuration providers (JSON, environment variables, etc.)
    * Strong typing support
    
    All configuration is now organized under the "Budoco" section in appsettings.json
    with logical subsections for different areas of functionality.
    */

    public static class bd_config
    {
        // Legacy constant names maintained for backward compatibility
        public const string DbType = "DbType";
        public const string DbServer = "DbServer";
        public const string DbDatabase = "DbDatabase";
        public const string DbUser = "DbUser";
        public const string DbPassword = "DbPassword";
        public const string DebugSkipSendingEmails = "DebugSkipSendingEmails";
        public const string SmtpHost = "SmtpHost";
        public const string SmtpPort = "SmtpPort";
        public const string SmtpUser = "SmtpUser";
        public const string SmtpPassword = "SmtpPassword";
        public const string ImapHost = "ImapHost";
        public const string ImapPort = "ImapPort";
        public const string ImapUser = "ImapUser";
        public const string ImapPassword = "ImapPassword";
        public const string OutgoingEmailDisplayName = "OutgoingEmailDisplayName";
        public const string UseDeveloperExceptionPage = "UseDeveloperExceptionPage";
        public const string WebsiteUrlRootWithoutSlash = "WebsiteUrlRootWithoutSlash";
        public const string AppName = "AppName";
        public const string UseCustomCss = "UseCustomCss";
        public const string CustomCssFilename = "CustomCssFilename";
        public const string RowsPerPage = "RowsPerPage";
        public const string LogFileFolder = "LogFileFolder";
        public const string DateFormat = "DateFormat";
        public const string NewUserStartsInactive = "NewUserStartsInactive";
        public const string NewUserStartsReportOnly = "NewUserStartsReportOnly";
        public const string DebugAutoConfirmRegistration = "DebugAutoConfirmRegistration";
        public const string DebugEnableRunSql = "DebugEnableRunSql";
        public const string MaxNumberOfSendingRetries = "MaxNumberOfSendingRetries";
        public const string EnableIncomingEmail = "EnableIncomingEmail";
        public const string SecondsToSleepAfterCheckingIncomingEmail = "SecondsToSleepAfterCheckingIncomingEmail";
        public const string DebugSkipDeleteOfIncomingEmails = "DebugSkipDeleteOfIncomingEmails";
        public const string DebugPathToEmailFile = "DebugPathToEmailFile";
        public const string CheckForDangerousSqlKeywords = "CheckForDangerousSqlKeywords";
        public const string DangerousSqlKeywords = "DangerousSqlKeywords";
        public const string DebugFolderToSaveEmails = "DebugFolderToSaveEmails";

        public const string CustomFieldEnabled1 = "CustomFieldEnabled1";
        public const string CustomFieldLabelSingular1 = "CustomFieldLabelSingular1";
        public const string CustomFieldLabelPlural1 = "CustomFieldLabelPlural1";

        public const string CustomFieldEnabled2 = "CustomFieldEnabled2";
        public const string CustomFieldLabelSingular2 = "CustomFieldLabelSingular2";
        public const string CustomFieldLabelPlural2 = "CustomFieldLabelPlural2";

        public const string CustomFieldEnabled3 = "CustomFieldEnabled3";
        public const string CustomFieldLabelSingular3 = "CustomFieldLabelSingular3";
        public const string CustomFieldLabelPlural3 = "CustomFieldLabelPlural3";

        public const string CustomFieldEnabled4 = "CustomFieldEnabled4";
        public const string CustomFieldLabelSingular4 = "CustomFieldLabelSingular4";
        public const string CustomFieldLabelPlural4 = "CustomFieldLabelPlural4";

        public const string CustomFieldEnabled5 = "CustomFieldEnabled5";
        public const string CustomFieldLabelSingular5 = "CustomFieldLabelSingular5";
        public const string CustomFieldLabelPlural5 = "CustomFieldLabelPlural5";

        public const string CustomFieldEnabled6 = "CustomFieldEnabled6";
        public const string CustomFieldLabelSingular6 = "CustomFieldLabelSingular6";
        public const string CustomFieldLabelPlural6 = "CustomFieldLabelPlural6";

        public const string RegistrationRequestExpirationInHours = "RegistrationRequestExpirationInHours";
        public const string InviteUserExpirationInHours = "InviteUserExpirationInHours";
        public const string IssueEmailPreamble = "IssueEmailPreamble";
        public const string EnableIncomingEmailIssueCreation = "EnableIncomingEmailIssueCreation";
        public const string DebugLogLevelMicrosoft = "DebugLogLevelMicrosoft";
        public const string DebugLogLevelBudoco = "DebugLogLevelBudoco";
        public const string DebugLogLevelPostgres = "DebugLogLevelPostgres";

        private static IConfiguration _configuration;
        private static readonly object _lock = new object();

        // Configuration path mappings for backward compatibility
        private static readonly Dictionary<string, string> _configPaths = new Dictionary<string, string>
        {
            // Database settings (now use connection strings)
            { DbType, "Budoco:DatabaseType" },
            
            // Application settings
            { AppName, "Budoco:Application:AppName" },
            { WebsiteUrlRootWithoutSlash, "Budoco:Application:WebsiteUrlRootWithoutSlash" },
            { UseCustomCss, "Budoco:Application:UseCustomCss" },
            { CustomCssFilename, "Budoco:Application:CustomCssFilename" },
            { RowsPerPage, "Budoco:Application:RowsPerPage" },
            { DateFormat, "Budoco:Application:DateFormat" },
            { UseDeveloperExceptionPage, "Budoco:Application:UseDeveloperExceptionPage" },
            
            // Email settings
            { SmtpHost, "Budoco:Email:Smtp:Host" },
            { SmtpPort, "Budoco:Email:Smtp:Port" },
            { SmtpUser, "Budoco:Email:Smtp:User" },
            { SmtpPassword, "Budoco:Email:Smtp:Password" },
            { ImapHost, "Budoco:Email:Imap:Host" },
            { ImapPort, "Budoco:Email:Imap:Port" },
            { ImapUser, "Budoco:Email:Imap:User" },
            { ImapPassword, "Budoco:Email:Imap:Password" },
            { OutgoingEmailDisplayName, "Budoco:Email:OutgoingEmailDisplayName" },
            { IssueEmailPreamble, "Budoco:Email:IssueEmailPreamble" },
            { EnableIncomingEmail, "Budoco:Email:EnableIncomingEmail" },
            { EnableIncomingEmailIssueCreation, "Budoco:Email:EnableIncomingEmailIssueCreation" },
            { MaxNumberOfSendingRetries, "Budoco:Email:MaxNumberOfSendingRetries" },
            { SecondsToSleepAfterCheckingIncomingEmail, "Budoco:Email:SecondsToSleepAfterCheckingIncomingEmail" },
            
            // User registration settings
            { NewUserStartsInactive, "Budoco:UserRegistration:NewUserStartsInactive" },
            { NewUserStartsReportOnly, "Budoco:UserRegistration:NewUserStartsReportOnly" },
            { RegistrationRequestExpirationInHours, "Budoco:UserRegistration:RegistrationRequestExpirationInHours" },
            { InviteUserExpirationInHours, "Budoco:UserRegistration:InviteUserExpirationInHours" },
            
            // Custom fields
            { CustomFieldEnabled1, "Budoco:CustomFields:Field1:Enabled" },
            { CustomFieldLabelSingular1, "Budoco:CustomFields:Field1:LabelSingular" },
            { CustomFieldLabelPlural1, "Budoco:CustomFields:Field1:LabelPlural" },
            { CustomFieldEnabled2, "Budoco:CustomFields:Field2:Enabled" },
            { CustomFieldLabelSingular2, "Budoco:CustomFields:Field2:LabelSingular" },
            { CustomFieldLabelPlural2, "Budoco:CustomFields:Field2:LabelPlural" },
            { CustomFieldEnabled3, "Budoco:CustomFields:Field3:Enabled" },
            { CustomFieldLabelSingular3, "Budoco:CustomFields:Field3:LabelSingular" },
            { CustomFieldLabelPlural3, "Budoco:CustomFields:Field3:LabelPlural" },
            { CustomFieldEnabled4, "Budoco:CustomFields:Field4:Enabled" },
            { CustomFieldLabelSingular4, "Budoco:CustomFields:Field4:LabelSingular" },
            { CustomFieldLabelPlural4, "Budoco:CustomFields:Field4:LabelPlural" },
            { CustomFieldEnabled5, "Budoco:CustomFields:Field5:Enabled" },
            { CustomFieldLabelSingular5, "Budoco:CustomFields:Field5:LabelSingular" },
            { CustomFieldLabelPlural5, "Budoco:CustomFields:Field5:LabelPlural" },
            { CustomFieldEnabled6, "Budoco:CustomFields:Field6:Enabled" },
            { CustomFieldLabelSingular6, "Budoco:CustomFields:Field6:LabelSingular" },
            { CustomFieldLabelPlural6, "Budoco:CustomFields:Field6:LabelPlural" },
            
            // Security settings
            { CheckForDangerousSqlKeywords, "Budoco:Security:CheckForDangerousSqlKeywords" },
            { DangerousSqlKeywords, "Budoco:Security:DangerousSqlKeywords" },
            
            // Logging settings
            { LogFileFolder, "Budoco:Logging:LogFileFolder" },
            { DebugLogLevelMicrosoft, "Budoco:Logging:LogLevelMicrosoft" },
            { DebugLogLevelBudoco, "Budoco:Logging:LogLevelBudoco" },
            { DebugLogLevelPostgres, "Budoco:Logging:LogLevelPostgres" },
            
            // Debug settings
            { DebugSkipSendingEmails, "Budoco:Debug:SkipSendingEmails" },
            { DebugAutoConfirmRegistration, "Budoco:Debug:AutoConfirmRegistration" },
            { DebugEnableRunSql, "Budoco:Debug:EnableRunSql" },
            { DebugSkipDeleteOfIncomingEmails, "Budoco:Debug:SkipDeleteOfIncomingEmails" },
            { DebugPathToEmailFile, "Budoco:Debug:PathToEmailFile" },
            { DebugFolderToSaveEmails, "Budoco:Debug:FolderToSaveEmails" }
        };

        public static void set_configuration(IConfiguration configuration)
        {
            lock (_lock)
            {
                _configuration = configuration;
                bd_util.log("bd_config: Configuration initialized with ASP.NET Core IConfiguration");
            }
        }

        public static void load_config()
        {
            // This method is maintained for backward compatibility but no longer loads from file
            // Configuration is now loaded automatically by ASP.NET Core
            if (_configuration == null)
            {
                bd_util.log("bd_config.load_config(): Configuration not yet initialized. Call set_configuration() first.");
                return;
            }
            
            bd_util.log("bd_config.load_config(): Using ASP.NET Core configuration system");
        }

        public static void log_config()
        {
            if (_configuration == null)
            {
                bd_util.log("bd_config.log_config(): Configuration not initialized");
                return;
            }

            // Log all Budoco configuration values
            var budocoSection = _configuration.GetSection("Budoco");
            if (budocoSection.Exists())
            {
                LogConfigurationSection(budocoSection, "Budoco");
            }
            
            // Log connection strings
            var connectionStrings = _configuration.GetSection("ConnectionStrings");
            if (connectionStrings.Exists())
            {
                LogConfigurationSection(connectionStrings, "ConnectionStrings");
            }
        }

        private static void LogConfigurationSection(IConfigurationSection section, string prefix)
        {
            foreach (var child in section.GetChildren())
            {
                if (child.GetChildren().Any())
                {
                    LogConfigurationSection(child, $"{prefix}:{child.Key}");
                }
                else
                {
                    bd_util.log($"{prefix}:{child.Key}:{child.Value}");
                }
            }
        }

        public static dynamic get(string key)
        {
            if (_configuration == null)
            {
                bd_util.log($"bd_config.get({key}): Configuration not initialized, returning empty string");
                return "";
            }

            // Check if we have a mapped path for this key
            if (_configPaths.ContainsKey(key))
            {
                string configPath = _configPaths[key];
                string value = _configuration[configPath];
                
                if (value != null)
                {
                    // Try to parse as integer if possible
                    if (int.TryParse(value, out int intValue))
                    {
                        return intValue;
                    }
                    return value;
                }
            }

            // Fallback: try to get the value directly using the key
            string directValue = _configuration[key];
            if (directValue != null)
            {
                if (int.TryParse(directValue, out int intValue))
                {
                    return intValue;
                }
                return directValue;
            }

            // Return empty string if not found
            return "";
        }

        public static string get_connection_string()
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Configuration not set. Call set_configuration() first.");
            }

            string databaseType = _configuration["Budoco:DatabaseType"] ?? "SqlServer";
            return _configuration.GetConnectionString(databaseType);
        }

        public static string get_database_type()
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Configuration not set. Call set_configuration() first.");
            }

            return _configuration["Budoco:DatabaseType"] ?? "SqlServer";
        }

    }
}
