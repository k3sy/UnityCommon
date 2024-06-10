using System.Collections.Generic;

namespace UnityCommon
{
    /// <summary>
    /// LinkedList<T>に対する拡張メソッド
    /// </summary>
    public static class LinkedListExtensions
    {
        /// <summary>
        /// 列挙可能なLinkedListNode<T>のシーケンスを取得する
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<LinkedListNode<T>> Nodes<T>(this LinkedList<T> list)
        {
            LinkedListNode<T> node = list.First;
            while (node != null) {
                yield return node;
                node = node.Next;
            }
        }
    }
}
