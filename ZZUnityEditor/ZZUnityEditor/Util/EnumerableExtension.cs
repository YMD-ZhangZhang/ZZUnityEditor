using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class IEnumerableExtension
{
    public static void ZForeach<T>(this IEnumerable<T> source, Action<T> func)
    {
        foreach (T x in source)
        {
            func(x);
        }
    }

    public static void ZForeachWithIndex<T>(this IEnumerable<T> source, Action<T, int> func)
    {
        int i = 0;
        foreach (T x in source)
        {
            func(x, i++);
        }
    }
}
