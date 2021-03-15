using System.Collections.ObjectModel;

namespace PuyoTextEditor.Collections
{
    public static class OrderedDictionaryExtensions
    {
        /// <summary>
        /// Searches for the specified key and returns the zero-based index.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The <see cref="OrderedDictionary{TKey, TValue}"/> to search.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The zero-based index of the key, if found; otherwise, –1.</returns>
        public static int IndexOfKey<TKey, TValue>(this OrderedDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
        {
            // We know Keys is of type ReadOnlyCollection<TKey>.
            return ((ReadOnlyCollection<TKey>)dictionary.Keys).IndexOf(key);
        }

        /// <summary>
        /// Searches for the specified value and returns the zero-based index.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The <see cref="OrderedDictionary{TKey, TValue}"/> to search.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>The zero-based value of the key, if found; otherwise, –1.</returns>
        public static int IndexOfValue<TKey, TValue>(this OrderedDictionary<TKey, TValue> dictionary, TValue value)
            where TKey : notnull
        {
            // We know Values is of type ReadOnlyCollection<TValue>.
            return ((ReadOnlyCollection<TValue>)dictionary.Values).IndexOf(value);
        }
    }
}
