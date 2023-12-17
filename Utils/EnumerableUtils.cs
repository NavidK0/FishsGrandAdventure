namespace FishsGrandAdventure.Utils;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

[PublicAPI]
public static class EnumerableUtils
{
    private static readonly Random Rng = new Random();

    /// <summary>
    /// Randomizes an array.
    /// </summary>
    /// <param name="array"></param>
    /// <typeparam name="T"></typeparam>
    public static List<T> Shuffle<T>(this T[] array)
    {
        List<T> ts = array.ToList();
        Shuffle(ts);
        return ts;
    }

    /// <summary>
    /// Randomizes an IEnumerable.
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    public static List<T> Shuffle<T>(this IEnumerable<T> list)
    {
        List<T> ts = list.ToList();
        Shuffle(ts);
        return ts;
    }

    public static List<T> Shuffle<T>(this IEnumerable<T> list, Random random)
    {
        List<T> ts = list.ToList();
        Shuffle(ts, random);
        return ts;
    }

    /// <summary>
    /// Randomizes a List.
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i > 1; i--)
        {
            int rnd = Rng.Next(i + 1);

            T oldI = list[i];
            T oldRng = list[rnd];
            list[i] = oldRng;
            list[rnd] = oldI;
        }
    }

    public static void Shuffle<T>(this IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 1; i--)
        {
            int rnd = random.Next(i + 1);

            T oldI = list[i];
            T oldRng = list[rnd];
            list[i] = oldRng;
            list[rnd] = oldI;
        }
    }

    /// <summary>
    /// Picks a random element from a list.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <param name="ignoreNull"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandomElement<T>(this IEnumerable<T> enumerable, bool ignoreNull = false)
    {
        return GetRandomElement(enumerable.ToList());
    }

    /// <summary>
    /// Picks a random element from an IEnumerable.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="noRepeats"></param>
    /// <param name="ignoreNull"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandomElement<T>(this List<T> list, bool noRepeats = false, bool ignoreNull = false)
    {
        if (ignoreNull)
        {
            var noNulls = new List<T>(list);
            noNulls.RemoveAll(x => x == null);
            return noNulls[Rng.Next(noNulls.Count)];
        }

        try
        {
            return list[Rng.Next(list.Count)];
        }
        catch (Exception)
        {
            return default;
        }
    }

    public static T GetRandomElement<T>(this List<T> list, Random random)
    {
        try
        {
            return list[random.Next(list.Count)];
        }
        catch (Exception)
        {
            return default;
        }
    }

    public static T GetRandomElementByWeight<T>(this List<T> sequence, Func<T, float> weightSelector)
    {
        List<T> enumerable = sequence.ToList();
        float totalWeight = enumerable.Sum(weightSelector);
        float itemWeightIndex = (float)Rng.NextDouble() * totalWeight;
        float currentWeightIndex = 0;

        foreach (var item in enumerable.Select(weightedItem =>
                     new { Value = weightedItem, Weight = weightSelector(weightedItem) }))
        {
            currentWeightIndex += item.Weight;

            if (currentWeightIndex >= itemWeightIndex)
                return item.Value;
        }

        return default;
    }

    public static T[][] CreateJaggedArray<T>(this IList<T[]> arrays)
    {
        int minorLength = arrays[0].Length;
        var ret = new T[arrays.Count][];
        for (var i = 0; i < minorLength; i++)
        {
            ret[i] = new T[arrays[i].Length];
        }

        for (var i = 0; i < arrays.Count; i++)
        {
            T[] array = arrays[i];
            if (array.Length != minorLength)
            {
                throw new ArgumentException("All arrays must be the same length");
            }

            for (var j = 0; j < minorLength; j++)
            {
                ret[i][j] = array[j];
            }
        }

        return ret;
    }

    public static string ConvertToString<T>(this IList<T> list)
    {
        return $"[{string.Join(",", list)}]";
    }

    /// <summary>
    /// Returns <c>true</c> if the list is either null or empty. Otherwise <c>false</c>.
    /// </summary>
    /// <param name="list">The list.</param>
    public static bool IsEmpty<T>(this IList<T> list)
    {
        return list.Count == 0;
    }

    /// <summary>
    /// Returns <c>true</c> if the list is either null or empty. Otherwise <c>false</c>.
    /// </summary>
    /// <param name="list">The list.</param>
    public static bool IsNullOrEmpty<T>(this IList<T> list)
    {
        if (list != null)
            return list.Count == 0;
        return true;
    }

    /// <summary>Calls an action on each item before yielding them.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="action">The action to call for each item.</param>
    public static IEnumerable<T> Examine<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T obj in source)
        {
            action(obj);
            yield return obj;
        }
    }

    /// <summary>Perform an action on each item.</summary>
    /// <param name="source">The source.</param>
    /// <param name="action">The action to perform.</param>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        List<T> forEach = source.ToList();
        foreach (T obj in forEach)
            action(obj);
        return forEach;
    }

    /// <summary>Perform an action on each item.</summary>
    /// <param name="source">The source.</param>
    /// <param name="action">The action to perform.</param>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        var num = 0;
        T[] forEach = source as T[] ?? source.ToArray();
        foreach (T obj in forEach)
            action(obj, num++);
        return forEach;
    }

    /// <summary>Convert each item in the collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="converter">Func to convert the items.</param>
    public static IEnumerable<T> Convert<T>(this IEnumerable source, Func<object, T> converter)
    {
        foreach (object obj in source)
            yield return converter(obj);
    }

    /// <summary>Convert a collection to a HashSet.</summary>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
        return new HashSet<T>(source);
    }

    /// <summary>Convert a collection to a HashSet.</summary>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
    {
        return new HashSet<T>(source, comparer);
    }

    /// <summary>Add an item to the beginning of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, Func<T> prepend)
    {
        yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>Add an item to the beginning of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T prepend)
    {
        yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, IEnumerable<T> prepend)
    {
        foreach (T obj in prepend)
            yield return obj;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, bool condition, Func<T> prepend)
    {
        if (condition)
            yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, bool condition, T prepend)
    {
        if (condition)
            yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, bool condition, IEnumerable<T> prepend)
    {
        if (condition)
        {
            foreach (T obj in prepend)
                yield return obj;
        }

        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<bool> condition, Func<T> prepend)
    {
        if (condition())
            yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<bool> condition, T prepend)
    {
        if (condition())
            yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<bool> condition,
        IEnumerable<T> prepend)
    {
        if (condition())
        {
            foreach (T obj in prepend)
                yield return obj;
        }

        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, bool> condition,
        Func<T> prepend)
    {
        List<T> enumerable = source.ToList();
        if (condition(enumerable))
            yield return prepend();
        foreach (T obj in enumerable)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, bool> condition,
        T prepend)
    {
        List<T> enumerable = source.ToList();
        if (condition(enumerable))
            yield return prepend;
        foreach (T obj in enumerable)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(this IEnumerable<T> source, Func<IEnumerable<T>, bool> condition,
        IEnumerable<T> prepend)
    {
        List<T> enumerable = source.ToList();
        if (condition(enumerable))
        {
            foreach (T obj in prepend)
                yield return obj;
        }

        foreach (T obj in enumerable)
            yield return obj;
    }

    /// <summary>Add an item to the end of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="append">Func to create the item to append.</param>
    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, Func<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        yield return append();
    }

    /// <summary>Add an item to the end of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="append">The item to append.</param>
    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T append)
    {
        foreach (T obj in source)
            yield return obj;
        yield return append;
    }

    /// <summary>Add a collection to the end of another collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="append">The collection to append.</param>
    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, IEnumerable<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        foreach (T obj in append)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">Func to create the item to append.</param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, Func<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition)
            yield return append();
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The item to append.</param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, T append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition)
            yield return append;
    }

    /// <summary>
    /// Add a collection to the end of another collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The collection to append.</param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, bool condition, IEnumerable<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition)
        {
            foreach (T obj in append)
                yield return obj;
        }
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">Func to create the item to append.</param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, Func<bool> condition, Func<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition())
            yield return append();
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The item to append.</param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, Func<bool> condition, T append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition())
            yield return append;
    }

    /// <summary>
    /// Add a collection to the end of another collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The collection to append.</param>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> source, Func<bool> condition,
        IEnumerable<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition())
        {
            foreach (T obj in append)
                yield return obj;
        }
    }

    /// <summary>
    /// Returns and casts only the items of type <typeparamref name="T" />.
    /// </summary>
    /// <param name="source">The collection.</param>
    public static IEnumerable<T> FilterCast<T>(this IEnumerable source)
    {
        foreach (object obj in source)
        {
            if (obj is T variable)
                yield return variable;
        }
    }

    /// <summary>Adds a collection to a hashset.</summary>
    /// <param name="hashSet">The hashset.</param>
    /// <param name="range">The collection.</param>
    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
    {
        foreach (T obj in range)
            hashSet.Add(obj);
    }

    /// <summary>Sets all items in the list to the given value.</summary>
    /// <param name="list">The list.</param>
    /// <param name="item">The value.</param>
    public static void Populate<T>(this IList<T> list, T item)
    {
        int count = list.Count;
        for (var index = 0; index < count; ++index)
            list[index] = item;
    }

    public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
    {
        if (list is List<T> list1)
        {
            list1.AddRange(collection);
        }
        else
        {
            foreach (T obj in collection)
                list.Add(obj);
        }
    }

    /// <summary>Sorts an IList</summary>
    public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
    {
        if (list is List<T> list1)
        {
            list1.Sort(comparison);
        }
        else
        {
            var objList = new List<T>(list);
            objList.Sort(comparison);
            for (var index = 0; index < list.Count; ++index)
                list[index] = objList[index];
        }
    }

    /// <summary>Sorts an IList</summary>
    public static void Sort<T>(this IList<T> list)
    {
        if (list is List<T> list1)
        {
            list1.Sort();
        }
        else
        {
            var objList = new List<T>(list);
            objList.Sort();
            for (var index = 0; index < list.Count; ++index)
                list[index] = objList[index];
        }
    }

    ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
    ///<param name="items">The enumerable to search.</param>
    ///<param name="predicate">The expression to test the items against.</param>
    ///<returns>The index of the first matching item, or -1 if no items match.</returns>
    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var retVal = 0;
        foreach (T item in items)
        {
            if (predicate(item)) return retVal;
            retVal++;
        }

        return -1;
    }

    ///<summary>Finds the index of the first occurrence of an item in an enumerable.</summary>
    ///<param name="items">The enumerable to search.</param>
    ///<param name="item">The item to find.</param>
    ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
    public static int IndexOf<T>(this IEnumerable<T> items, T item)
    {
        return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
    }

    public static T Find<T>(this T[] items, Predicate<T> predicate) => Array.Find(items, predicate);

    public static T[] FindAll<T>(this T[] items, Predicate<T> predicate) => Array.FindAll(items, predicate);

    /// <summary>
    ///   Checks whether or not collection is null or empty. Assumes collection can be safely enumerated multiple times.
    /// </summary>
    public static bool IsNullOrEmpty(this IEnumerable @this) => @this == null || !@this.GetEnumerator().MoveNext();
}