using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aurora.Utils {

    public static class CollectionUtils {

        /// <summary>
        /// Creates a deep clone of an observable collection by cloning all individual items in the collection also.
        /// </summary>
        public static ObservableCollection<T> Clone<T>(this ObservableCollection<T> source) where T : ICloneable =>
            new ObservableCollection<T>(
                source.Select(c => (T)c.Clone())
            );


        /// <summary>
        /// Gets the element at the given index in the non-generic <see cref="IEnumerable"/>.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException" />
        /// <remarks>Not called 'ElementAt' as this causes issues with <see cref="Enumerable.ElementAt{TSource}(System.Collections.Generic.IEnumerable{TSource}, int)"/>,
        /// where this method usually takes priority over the better generic method if the generic type parameter is missing.</remarks>
        public static object ElementAtIndex(this IEnumerable source, int index) {
            var i = 0;
            foreach (var item in source)
                if (index == i++)
                    return item;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Deconstructor for a KeyValuePair.
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value) {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
