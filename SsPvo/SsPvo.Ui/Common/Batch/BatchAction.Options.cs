using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SsPvo.Ui.Common.Batch
{
    partial class BatchAction
    {
        public class Options
        {
            private readonly IDictionary<string, object> _options;

            public Options()
            {
                _options = new ConcurrentDictionary<string, object>();
            }

            public object this[string key] => _options[key];

            public bool HasKey(string key) => _options.ContainsKey(key);

            public TValue GetValue<TValue>(string key) => (TValue)_options[key];

            public TValue GetValueOrDefault<TValue>(string key) =>
                HasKey(key) ? GetValue<TValue>(key) : default(TValue);

            public IEnumerable<string> GetAvailableOptions() => _options.Keys;

            public void AddOrUpdate<TValue>(string key, TValue value)
            {
                if (_options.ContainsKey(key))
                {
                    _options[key] = value;
                }
                else
                {
                    _options.Add(key, value);
                }
            }

            public enum CommonOptions
            {
                StopAllOnError,
                StopAllOnCancel,
                Log,
                HandleItemsInSeparateThread
            }
        }
    }
}
