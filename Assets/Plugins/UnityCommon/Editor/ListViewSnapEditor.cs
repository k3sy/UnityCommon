using UnityEditor;

namespace UnityCommon
{
    [CustomEditor(typeof(ListViewSnap), true)]
    [CanEditMultipleObjects]
    public class ListViewSnapEditor : ListViewEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false)) {
                if (iterator.name == "_MaxColumnCount") { continue; }
                EditorGUILayout.PropertyField(iterator, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
