// Source code is taken from CommonAPI (GPL-3.0) - https://github.com/limoka/CommonAPI
// Source code is indirectly taken from R2API (MIT) - https://github.com/risk-of-thunder/R2API
using System;
using System.Collections.Generic;
using System.Linq;

namespace YuanAPI {
    public static class EnumerableExtensions {

        /// <summary>
        /// ForEach but with a try catch in it.
        /// </summary>
        /// <param name="list">the enumerable object</param>
        /// <param name="action">the action to do on it</param>
        /// <param name="exceptions">the exception dictionary that will get filled, null by default if you simply want to silence the errors if any pop.</param>
        /// <typeparam name="T"></typeparam>
        public static void ForEachTry<T>(this IEnumerable<T> list, Action<T> action, IDictionary<T, Exception> exceptions = null!) {
            list.ToList().ForEach(element => {
                try {
                    action.Invoke(element);
                } catch (Exception exception) {
                    exceptions.Add(element, exception);
                }
            });
        }
    }
}
