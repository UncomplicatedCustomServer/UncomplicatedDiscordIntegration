using System;
using System.Collections.Generic;

namespace UncomplicatedDiscordIntegration.Extensions
{
    public static class DictionaryExtension
    {
        public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else 
                dictionary.Add(key, value);
        }
    }
}
