using UnityEditor;
using UnityEngine;

namespace UnityCommon
{
    [CustomEditor(typeof(TMP_RubySetter))]
    [CanEditMultipleObjects]
    public class TMP_RubySetterEditor : Editor
    {
        private SerializedProperty _UneditedText;
        private SerializedProperty _UseEasyBestFit;
        private SerializedProperty _FontSizeMin;
        private SerializedProperty _FontSizeMax;

        private void OnEnable()
        {
            _UneditedText = serializedObject.FindProperty("_UneditedText");
            _UseEasyBestFit = serializedObject.FindProperty("_UseEasyBestFit");
            _FontSizeMin = serializedObject.FindProperty("_FontSizeMin");
            _FontSizeMax = serializedObject.FindProperty("_FontSizeMax");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_UneditedText);
            EditorGUILayout.PropertyField(_UseEasyBestFit);
            if (_UseEasyBestFit.boolValue) {
                using (new EditorGUI.IndentLevelScope()) {
                    Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PrefixLabel(rect, new GUIContent("Font Size"));

                    int previousIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2;
                    rect.x += EditorGUIUtility.labelWidth;

                    EditorGUIUtility.labelWidth = 24;
                    using (var check = new EditorGUI.ChangeCheckScope()) {
                        EditorGUI.PropertyField(rect, _FontSizeMin, new GUIContent("Min"));
                        if (check.changed) {
                            float minSize = Mathf.Max(0, _FontSizeMin.floatValue);
                            _FontSizeMin.floatValue = Mathf.Min(minSize, _FontSizeMax.floatValue);
                        }
                    }
                    rect.x += rect.width;

                    EditorGUIUtility.labelWidth = 27;
                    using (var check = new EditorGUI.ChangeCheckScope()) {
                        EditorGUI.PropertyField(rect, _FontSizeMax, new GUIContent("Max"));
                        if (check.changed) {
                            float maxSize = Mathf.Clamp(_FontSizeMax.floatValue, 0, 32767);
                            _FontSizeMax.floatValue = Mathf.Max(_FontSizeMin.floatValue, maxSize);
                        }
                    }
                    rect.x += rect.width;

                    EditorGUI.indentLevel = previousIndent;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
