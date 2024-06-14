using UnityEditor;

namespace UnityCommon
{
    [CustomEditor(typeof(ListViewSnap), true)]
    [CanEditMultipleObjects]
    public class ListViewSnapEditor : ListViewEditor
    {
        private SerializedProperty _LerpDuration;
        private SerializedProperty _OnChangeItemData;

        protected override void OnEnable()
        {
            base.OnEnable();

            _LerpDuration = serializedObject.FindProperty("_LerpDuration");
            _OnChangeItemData = serializedObject.FindProperty("OnChangeItemData");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUILayout.PropertyField(_LerpDuration);
            EditorGUILayout.PropertyField(_OnChangeItemData);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
