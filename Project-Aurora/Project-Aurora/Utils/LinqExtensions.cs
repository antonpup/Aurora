using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Utils {
    public static class LinqExtensions {

        /// <summary>
        /// Performs an action on each item in the given collection.
        /// </summary>
        /// <param name="each">The action to perform once on each item in the collection.</param>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> each) {
            foreach (var item in self)
                each(item);
        }

        /// <summary>
        /// Similar to the Linq Select method but wraps each call in a try...catch statement.
        /// If the selector throws an error, it will not be included in the result. For this reason, the resulting enumerable from this method may
        /// be shorter than the given enumerable it was called on.
        /// </summary>
        /// <param name="selector">The method that will perform the selection function.</param>
        /// <param name="errorHandler">An optional action that will be called for each exception encountered.</param>
        public static IEnumerable<TResult> TrySelect<TIn, TResult>(this IEnumerable<TIn> self, Func<TIn, TResult> selector, Action<Exception, TIn> errorHandler = null) {
            // We use SelectMany so that we can return an empty enumerable on exception to have the element removed from the collection.
            return self.SelectMany(val => {
                try {
                    // Since we're using SelectMany, we must return an IEnumerable<TOut>, so wrap the result of selector in an array.
                    return new[] { selector(val) };
                } catch (Exception ex) {
                    errorHandler?.Invoke(ex, val);
                    return new TResult[0];
                }
            });
        }

        /// <summary>
        /// Similar to the Linq Distinct method but will allow for ensuring a particular value of the items are distinct, rather than the items themselves.
        /// When multiple items with the same key exist, the first one will be the one returned in the out collection.
        /// </summary>
        /// <param name="keySelector">The method that will return a value that will be used to filter the collection so that only one item with this value exists.</param>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> self, Func<T, TKey> keySelector) {
            return self.GroupBy(keySelector).Select(group => group.First());
        }
    }
}
