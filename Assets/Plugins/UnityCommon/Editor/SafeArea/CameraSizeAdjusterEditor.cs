using UnityEditor;
using UnityEngine;

namespace UnityCommon
{
    [CustomEditor(typeof(CameraSizeAdjuster))]
    public class CameraSizeAdjusterEditor : Editor
    {
        private SerializedProperty _BaseAspectRatio;
        private SerializedProperty _BaseOrthographicSize;
        private SerializedProperty _BaseFieldOfView;
        private SerializedProperty _ShowBaseAspectArea;
        private SerializedProperty _BaseAspectAreaColor;

        private Camera _Camera;

        private void OnEnable()
        {
            _BaseAspectRatio = serializedObject.FindProperty("_BaseAspectRatio");
            _BaseOrthographicSize = serializedObject.FindProperty("_BaseOrthographicSize");
            _BaseFieldOfView = serializedObject.FindProperty("_BaseFieldOfView");
            _ShowBaseAspectArea = serializedObject.FindProperty("_ShowBaseAspectArea");
            _BaseAspectAreaColor = serializedObject.FindProperty("_BaseAspectAreaColor");

            var cameraSizeAdjuster = target as CameraSizeAdjuster;
            _Camera = cameraSizeAdjuster.GetComponent<Camera>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_BaseAspectRatio);

            if (_Camera.orthographic) {
                EditorGUILayout.PropertyField(_BaseOrthographicSize);
            } else {
                EditorGUILayout.PropertyField(_BaseFieldOfView);
            }

            EditorGUILayout.PropertyField(_ShowBaseAspectArea);
            if (_ShowBaseAspectArea.boolValue) {
                EditorGUILayout.PropertyField(_BaseAspectAreaColor);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
