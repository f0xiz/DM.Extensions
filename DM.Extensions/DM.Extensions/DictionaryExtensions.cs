using System;
using System.Collections.Generic;

namespace DM.Extensions
{
    /// <summary>
    /// Represents set of extension method for work with dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Get or safety add value to dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key</typeparam>
        /// <typeparam name="TValue">The type of value</typeparam>
        /// <param name="collection">The collection to get or add</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="defaultValue">The default value to return if key will no be found</param>
        public static TValue TryGetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> collection,
            TKey key,
            TValue defaultValue = default(TValue))
        {
            TValue value;

            return collection.TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <summary>
        /// Get or safety add value to dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of key</typeparam>
        /// <typeparam name="TValue">The type of value</typeparam>
        /// <param name="collection">The collection to get or add</param>
        /// <param name="key">The key to lookup</param>
        /// <param name="valueProvider">The delegate which will be called in case key will not be found</param>
        /// <param name="isThreadSafe">Call valueProvider delegate with locking of origin collection or not</param>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> collection,
            TKey key,
            Func<TValue> valueProvider,
            bool isThreadSafe = true)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "Unable execute GetOrAdd over collection which is null.");
            }

            TValue value;

            if (!collection.TryGetValue(key, out value))
            {
                if (isThreadSafe)
                {
                    lock (collection)
                    {
                        if (!collection.TryGetValue(key, out value))
                        {
                            value = valueProvider();
                            collection[key] = value;
                        }
                    }
                }
                else
                {
                    value = valueProvider();
                    collection[key] = value;
                }
            }

            return value;
        }

        /// <summary>
        /// Converts collection of single instances into dictionary via key/value selectors.
        /// </summary>
        /// <typeparam name="TSource">Type of input instance.</typeparam>
        /// <typeparam name="TKey">Type of key.</typeparam>
        /// <typeparam name="TValue">Type of value.</typeparam>
        /// <param name="source">Source collection to convert.</param>
        /// <param name="keySelector">Key selector.</param>
        /// <param name="valueSelector">Value selector.</param>
        /// <param name="isOverrideExistingKey">Defines if first or last value will be used in case of duplicated keys.</param>
        public static Dictionary<TKey, TValue> ToDistinctDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, bool isOverrideExistingKey = false)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            foreach (TSource element in source)
            {
                var key = keySelector(element);
                var value = valueSelector(element);

                if (result.ContainsKey(key))
                {
                    if (isOverrideExistingKey)
                    {
                        result[key] = value;
                    }
                }
                else
                {
                    result[key] = value;
                }
            }

            return result;
        }
    }
}