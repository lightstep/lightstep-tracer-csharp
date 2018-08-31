using System;
using System.Collections.Generic;

namespace LightStep
{
    /// <summary>
    ///     Baggage is a dictionary of key:value information that is propagated via the SpanContext.
    /// </summary>
    public class Baggage
    {
        private readonly IDictionary<string, string> _items =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     Sets key to value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            _items[key] = value;
        }

        /// <summary>
        ///     Gets value for key; Will return null if key does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            string value;
            return _items.TryGetValue(key, out value) ? value : null;
        }

        /// <summary>
        ///     Gets all <see cref="KeyValuePair{TKey,TValue}" /> from baggage.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            return _items;
        }

        /// <summary>
        ///     Combines two baggage into one.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(Baggage other)
        {
            Merge(other?.GetAll());
        }

        private void Merge(IEnumerable<KeyValuePair<string, string>> other)
        {
            if (other == null)
                return;

            // Copy entries into local dictionary instead of setting it directly
            // to make sure the case insensitive comparer is used.
            foreach (var kvp in other) Set(kvp.Key, kvp.Value);
        }
    }
}