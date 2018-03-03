namespace MoverSoft.Common.Configuration
{
    using System;
    using System.Linq;
    using SystemConfigManager = System.Configuration;
    using MoverSoft.Common.Extensions;
    using MoverSoft.Common.Utilities;

    public static class ConfigurationManager
    {
        public static string GetConfigurationValue(string settingName)
        {
            return ConfigurationManager.GetConfiguration<string>(
                settingName,
                (value) => value);
        }

        public static string GetConfigurationValue(string settingName, string defaultValue)
        {
            return ConfigurationManager.GetConfiguration<string>(
                settingName,
                (value) => value,
                defaultValue);
        }

        public static int GetConfigurationNumber(string settingName)
        {
            return ConfigurationManager.GetConfiguration<int>(
                settingName,
                (value) => Int32.Parse(value));
        }

        public static int GetConfigurationNumber(string settingName, int defaultValue)
        {
            return ConfigurationManager.GetConfiguration<int>(
                settingName,
                (value) => Int32.Parse(value),
                defaultValue);
        }

        public static TimeSpan GetConfigurationTimeSpan(string settingName)
        {
            return ConfigurationManager.GetConfiguration<TimeSpan>(
                settingName,
                (value) => TimeSpan.Parse(value));
        }

        public static TimeSpan GetConfigurationTimeSpan(string settingName, TimeSpan defaultValue)
        {
            return ConfigurationManager.GetConfiguration<TimeSpan>(
                settingName,
                (value) => TimeSpan.Parse(value),
                defaultValue);
        }

        public static bool GetConfigurationBoolean(string settingName)
        {
            return ConfigurationManager.GetConfiguration<bool>(
                settingName,
                (value) => Boolean.Parse(value));
        }

        public static bool GetConfigurationBoolean(string settingName, bool defaultValue)
        {
            return ConfigurationManager.GetConfiguration<bool>(
                settingName,
                (value) => Boolean.Parse(value),
                defaultValue);
        }

        public static string GetConnectionString(string keyName, string defaultValue = null)
        {
            var rawValue = SystemConfigManager.ConfigurationManager.ConnectionStrings[keyName];
            if (rawValue != null)
            {
                return ConfigurationManager.ConvertConfigurationValue<string>(
                    keyName: keyName,
                    rawValue: rawValue.ConnectionString,
                    converter: (connString) => connString,
                    defaultValue: defaultValue);
            }

            return defaultValue;
        }

        private static TResult GetConfiguration<TResult>(
            string keyName, 
            Func<string, TResult> converter, 
            TResult defaultValue = default(TResult))
        {
            if (SystemConfigManager.ConfigurationManager.AppSettings.AllKeys.Contains(keyName))
            {
                var rawValue = SystemConfigManager.ConfigurationManager.AppSettings[keyName];

                return ConfigurationManager.ConvertConfigurationValue<TResult>(keyName, rawValue, converter, defaultValue);
            }

            return defaultValue;
        }

        private static TResult ConvertConfigurationValue<TResult>(
            string keyName,
            string rawValue,
            Func<string, TResult> converter,
            TResult defaultValue = default(TResult))
        {
            if (!string.IsNullOrEmpty(rawValue))
            {
                if (keyName.StartsWithInsensitively("encrypted."))
                {
                    var decryptedValue = EncryptionUtility.DecryptSecrets(rawValue);
                    return !string.IsNullOrEmpty(decryptedValue) ? converter(decryptedValue) : defaultValue;
                }

                return converter(rawValue);
            }

            return defaultValue;
        }
    }
}
