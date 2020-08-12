using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace GoogleSharp.Src.Utils
{
    public class Prettifier<T>
    {
        public const int MaxItems = 8;

        public static String Prettify(HashSet<T> inputSet)
        {
            var itemList = inputSet.ToList();
            var result = new StringBuilder();

            if (itemList.Count <= MaxItems)
                CollectItemStrings(itemList, result, 0, itemList.Count);
            else
            {
                CollectItemStrings(itemList, result, 0, MaxItems / 2);
                result.Append("\t    ...\n");
                CollectItemStrings(itemList, result, itemList.Count - MaxItems / 2, itemList.Count);
            }

            return result.ToString();
        }

        private static void CollectItemStrings(List<T> itemList, StringBuilder builder, int startIndex, int endIndex)
        {
            int maxSize = itemList.Count.ToString().Length;
            for (int idx = startIndex; idx < endIndex; idx++)
                builder.Append($"\t{LeftPad((idx + 1).ToString(), maxSize)}) {itemList[idx]}\n");
        }

        private static string LeftPad(string s, int length)
        {
            if (s.Length >= length)
                return s;
            return s + Multiply(" ", length - s.Length);
        }

        private static string Multiply(String s, int multiplier)
        {
            return String.Concat(Enumerable.Repeat(s, multiplier));
        }
    }
}