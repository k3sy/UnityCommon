using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityCommon
{
    /// <summary>
    /// Inspector上で値を変更できないようにする
    /// </summary>
    public class DisableAttribute : PropertyAttribute
    {
        public readonly bool RuntimeOnly;

        public DisableAttribute(bool runtimeOnly = false)
        {
            RuntimeOnly = runtimeOnly;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableAttribute))]
    public class DisableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var disableAttribute = (DisableAttribute)attribute;
            EditorGUI.BeginDisabledGroup(!disableAttribute.RuntimeOnly || EditorApplication.isPlayingOrWillChangePlaymode);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif
}
