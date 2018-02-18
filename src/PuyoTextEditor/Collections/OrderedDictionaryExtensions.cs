using System.Collections.Generic;

namespace PuyoTextEditor.Collections
{
    public static class OrderedDictionaryExtensions
    {
        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="OrderedDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The types of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The <see cref="OrderedDictionary{TKey, TValue}"/> to search.</param>
        /// <param name="key">The object to locate in the System.Collections.Generic.List`1. The value can be null for reference types.</param>
        /// <returns></returns>
        public static int IndexOf<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            // We know Keys is of type List<TKey>
            return ((List<TKey>)dictionary.Keys).IndexOf(key);
        }
    }
}
