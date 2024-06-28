using System.Text.RegularExpressions;

namespace UnityCommon
{
    /// <summary>
    /// stringに対する拡張メソッド
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 文字列に含まれる改行コードを削除する
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveNewLine(this string text)
        {
            return Regex.Replace(text, @"\r\n|\r|\n", string.Empty);
        }

        /// <summary>
        /// 文字列に含まれるリッチテキストタグを削除する
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveRichTextTags(this string text)
        {
            return Regex.Replace(text, "<[^>]*?>", string.Empty);
        }
    }
}
