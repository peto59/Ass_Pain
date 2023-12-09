﻿using System;
using System.Collections.Generic;
using MWP.BackEnd;

namespace MWP
{
    /// <summary>
    /// Extensions to various custom objects
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Shuffles list
        /// </summary>
        /// <param name="list">list to be shuffled</param>
        /// <typeparam name="T">type</typeparam>
        public static void Shuffle<T>(this IList<T> list)
        {
            //TODO: figure out where one of the elements disappears to;
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = StateHandler.Rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        /// Removes and returns element at <paramref name="index"/>
        /// </summary>
        /// <param name="list">List to be modified</param>
        /// <param name="index">Index of removal</param>
        /// <typeparam name="T">type</typeparam>
        /// <returns>removed <typeparamref name="T"/></returns>
        public static T Pop<T>(this IList<T> list, int index = 0)
        {
            T value = list[index];
            list.RemoveAt(index);
            return value;
        }

        ///<summary>
		///If current number is more than <paramref name="max"/> it loops over to <paramref name="min"/>
        ///<br/>
        ///If current number is less than <paramref name="min"/> it loops over to <paramref name="max"/>
		///</summary>
        public static int LoopOver(this int current, int max, int min = 0)
        {
            if(current > max)
            {
                return min;
            }
            if(current < min)
            {
                return max;
            }
            return current;
        }

        ///<summary>
        ///If current number is less than 0, returns 0 else returns current number
		///</summary>
        public static int KeepPositive(this int current)
        {
            if (current < 0)
            {
                return 0;
            }
            return current;
        }
        
        ///<summary>
        ///Keeps integer between <paramref name="min"/> and <paramref name="max"/>
        ///</summary>
        public static int Constraint(this int current, int min, int max)
        {
            if(current > max)
            {
                return max;
            }
            if(current < min)
            {
                return min;
            }
            return current;
        }
        
        ///<summary>
        ///Case-Insensitive comparison
        ///</summary>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}