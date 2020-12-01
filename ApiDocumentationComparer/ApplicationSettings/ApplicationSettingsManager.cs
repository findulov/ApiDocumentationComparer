using ApiDocumentationComparer.Infrastructure.Settings;
using ApiDocumentationComparer.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ApiDocumentationComparer.ApplicationSettings
{
    public class ApplicationSettingsManager : ISettingsManager
    {
        private readonly Dictionary<string, Tuple<Func<string>, Action<string>>> settings =
            new Dictionary<string, Tuple<Func<string>, Action<string>>>
        {
            {
                nameof(Settings.Default.FirstApiUrl), new Tuple<Func<string>, Action<string>>
                (
                    () => Settings.Default.FirstApiUrl,
                    (value) => Settings.Default.FirstApiUrl = value
                )
            },
            {
                nameof(Settings.Default.SecondApiUrl), new Tuple<Func<string>, Action<string>>
                (
                    () => Settings.Default.SecondApiUrl,
                    (value) => Settings.Default.SecondApiUrl = value
                )
            },
        };

        public string Get(string key)
        {
            return settings.ContainsKey(key)
                ? settings[key].Item1()
                : throw new ArgumentException("Unsupported setting");
        }

        public void Save(string key, string value)
        {
            if (!settings.ContainsKey(key))
            {
                throw new ArgumentException("Unsupported setting");
            }

            settings[key].Item2(value);
            Settings.Default.Save();
        }
    }
}
