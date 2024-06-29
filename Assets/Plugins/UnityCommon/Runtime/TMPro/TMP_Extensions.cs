using TMPro;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// TextMeshProに対する拡張メソッド
    /// </summary>
    public static class TMP_Extensions
    {
        /// <summary>
        /// テキストがRectに収まるようフォントサイズを調整する
        /// </summary>
        /// <param name="text"></param>
        /// <param name="minFontSize"></param>
        /// <param name="maxFontSize"></param>
        public static void EasyBestFit(this TMP_Text text, float minFontSize, float maxFontSize)
        {
            float textWidth = text.preferredWidth;
            float textHeight = text.preferredHeight;
            float rectWidth = text.rectTransform.rect.width;
            float rectHeight = text.rectTransform.rect.height;
            if (Mathf.Approximately(textWidth, 0)
                || Mathf.Approximately(textHeight, 0)
                || Mathf.Approximately(rectWidth, 0)
                || Mathf.Approximately(rectHeight, 0)) {
                text.fontSize = minFontSize;
                return;
            }

            float rateX = textWidth / rectWidth;
            float rateY = textHeight / rectHeight;
            float fontScale = 1 / (rateX > rateY ? rateX : rateY);
            text.fontSize = Mathf.Clamp(text.fontSize * fontScale, minFontSize, maxFontSize);
        }
    }
}
