using UnityEditor;

namespace UnityCommon
{
    [CustomEditor(typeof(ListView), true)]
    [CanEditMultipleObjects]
    public class ListViewEditor : Editor
    {
        private SerializedProperty _ItemViewTemplate;
        private SerializedProperty _ItemSpacing;
        private SerializedProperty _Margin;
        private SerializedProperty _Direction;

        private ListView _ListView;

        protected virtual void OnEnable()
        {
            _ItemViewTemplate = serializedObject.FindProperty("_ItemViewTemplate");
            _ItemSpacing = serializedObject.FindProperty("_ItemSpacing");
            _Margin = serializedObject.FindProperty("_Margin");
            _Direction = serializedObject.FindProperty("_Direction");

            _ListView = target as ListView;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_ItemViewTemplate);
            EditorGUILayout.PropertyField(_ItemSpacing);
            if (!_ListView.IsInfiniteScroll) {
                EditorGUILayout.PropertyField(_Margin);
            }
            EditorGUILayout.PropertyField(_Direction);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
