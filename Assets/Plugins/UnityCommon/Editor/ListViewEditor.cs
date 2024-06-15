using UnityEditor;

namespace UnityCommon
{
    [CustomEditor(typeof(ListView), true)]
    [CanEditMultipleObjects]
    public class ListViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false)) {
                EditorGUILayout.PropertyField(iterator, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
