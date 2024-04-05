using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLAid.Extensions
{
    public static class EnumerableExtension
    {
        public static IEnumerable<TResult> ZipIt<TSource, TResult>(this IEnumerable<IEnumerable<TSource>> collection,
                                            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            var enumerators = collection.Select(c => c.GetEnumerator()).ToList();
            while (enumerators.All(e => e.MoveNext()))
            {
                yield return resultSelector(enumerators.Select(e => e.Current).ToList());
            }
        }
    }
}