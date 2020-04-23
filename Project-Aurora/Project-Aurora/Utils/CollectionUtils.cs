using System;
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
    }

}
