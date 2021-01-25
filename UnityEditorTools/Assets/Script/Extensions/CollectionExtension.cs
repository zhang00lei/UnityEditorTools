using System;
using System.Collections.Generic;

public static class CollectionExtension
{
    public static T GetLast<T>(this List<T> list)
    {
        if (list.IsNullOrEmpty())
        {
            return default;
        }

        return list[list.Count - 1];
    }

    public static bool IsNullOrEmpty<T>(this List<T> lst)
    {
        if (lst == null)
        {
            return true;
        }

        if (lst.Count == 0)
        {
            return true;
        }

        return false;
    }

    public static bool IsNullOrEmpty<T>(this T[] obj)
    {
        if (obj == null)
        {
            return true;
        }

        if (obj.Length == 0)
        {
            return true;
        }

        return false;
    }

    public static T ToFind<T>(this List<T> obj, Predicate<T> predicate)
    {
        for (int i = 0; i < obj.Count; i++)
        {
            if (predicate(obj[i]))
            {
                return obj[i];
            }
        }

        return default;
    }

    public static List<T> ToFindAll<T>(this List<T> obj, Predicate<T> predicate)
    {
        List<T> temp = new List<T>();
        for (int i = 0; i < obj.Count; i++)
        {
            if (predicate(obj[i]))
            {
                temp.Add(obj[i]);
            }
        }

        return temp;
    }


    public static T ToFind<T>(this T[] obj, Predicate<T> predicate)
    {
        for (int i = 0; i < obj.Length; i++)
        {
            if (predicate(obj[i]))
            {
                return obj[i];
            }
        }

        return default;
    }
    
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (action == null)
        {
            return;
        }

        foreach (T element in source)
        {
            action(element);
        }
    }
}