using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon
{
    /// <summary>
    /// Unityオブジェクトに対する拡張メソッド
    /// </summary>
    public static class UnityObjectExtensions
    {
        /// <summary>
        /// Unityオブジェクトを編集モードと再生モードのどちらで実行しているかに応じて適切に破棄する
        /// </summary>
        /// <param name="obj">破棄するUnityオブジェクト</param>
        /// <param name="t">オブジェクトを破棄するまでの時間（秒単位）</param>
        public static void Destroy(this Object obj, float t = 0)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) {
                Object.DestroyImmediate(obj);
                return;
            }
#endif
            Object.Destroy(obj, t);
        }
    }
}
