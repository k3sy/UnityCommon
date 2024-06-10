using UnityEditor;
using UnityEditor.UI;

namespace UnityCommon
{
    [CustomEditor(typeof(ListView), true)]
    [CanEditMultipleObjects]
    public class ListViewEditor : ScrollRectEditor
    {
        private SerializedProperty _ItemViewTemplate;
        private SerializedProperty _ItemSpacing;
        private SerializedProperty _Margin;
        private SerializedProperty _Direction;

        protected override void OnEnable()
        {
            base.OnEnable();

            _ItemViewTemplate = serializedObject.FindProperty("_ItemViewTemplate");
            _ItemSpacing = serializedObject.FindProperty("_ItemSpacing");
            _Margin = serializedObject.FindProperty("_Margin");
            _Direction = serializedObject.FindProperty("_Direction");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_ItemViewTemplate);
            EditorGUILayout.PropertyField(_ItemSpacing);
            EditorGUILayout.PropertyField(_Margin);
            EditorGUILayout.PropertyField(_Direction);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
