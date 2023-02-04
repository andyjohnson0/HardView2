using System;
using System.Collections.Generic;


namespace uk.andyjohnson.HardView2
{
    /// <summary>
    /// Extnesions to List<> class.
    /// </summary>
    public static class ListExt
    {
        /// <summary>
        /// Add individual key and value to a list of KeyValuePair.
        /// </summary>
        /// <typeparam name="Tkey">Key type</typeparam>
        /// <typeparam name="Tval">Value type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="val">Value</param>
        /// <returns>The updated list, to allow for method chanining</returns>
        public static List<KeyValuePair<Tkey, Tval>> Add<Tkey, Tval>(this List<KeyValuePair<Tkey, Tval>> self, Tkey key, Tval val)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            self.Add(new KeyValuePair<Tkey, Tval>(key, val));
            return self;
        }
    }
}