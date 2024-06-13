using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// アスペクト比に応じてカメラサイズを調整する
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraSizeAdjuster : MonoBehaviour
    {
        [SerializeField] private Vector2Int _BaseAspectRatio = new(9, 16);
        [SerializeField] private float _BaseOrthographicSize = 5;
        [SerializeField, Range(1, 179)] private float _BaseFieldOfView = 60;

        private Camera _Camera;
        private float _LastAspect;

        private float BaseAspect => _BaseAspectRatio.x / (float)_BaseAspectRatio.y;

        private void Start()
        {
            _Camera = GetComponent<Camera>();
            AdjustCameraSize();
        }

        private void Update()
        {
#if !UNITY_EDITOR
            if (_Camera.aspect == _LastAspect) {
                return;
            }
#endif
            AdjustCameraSize();
        }

        private void AdjustCameraSize()
        {
            _LastAspect = _Camera.aspect;

            if (_Camera.orthographic) {
                AdjustOrthographicCameraSize();
            } else {
                AdjustPerspectiveCameraSize();
            }
        }

        private void AdjustOrthographicCameraSize()
        {
            if (_Camera.aspect < BaseAspect) {
                // letterboxing
                float baseHorizontalSize = _BaseOrthographicSize * BaseAspect;
                float verticalSize = baseHorizontalSize / _Camera.aspect;
                _Camera.orthographicSize = verticalSize;
            } else {
                // pillarboxing
                _Camera.orthographicSize = _BaseOrthographicSize;
            }
        }

        private void AdjustPerspectiveCameraSize()
        {
            if (_Camera.aspect < BaseAspect) {
                // letterboxing
                float baseVerticalSize = Mathf.Tan(_BaseFieldOfView * 0.5f * Mathf.Deg2Rad);
                float baseHorizontalSize = baseVerticalSize * BaseAspect;
                float verticalSize = baseHorizontalSize / _Camera.aspect;
                float verticalFOV = Mathf.Atan(verticalSize) * Mathf.Rad2Deg * 2;
                _Camera.fieldOfView = verticalFOV;
            } else {
                // pillarboxing
                _Camera.fieldOfView = _BaseFieldOfView;
            }
        }

#if UNITY_EDITOR
        [SerializeField] private bool _ShowBaseAspectArea = false;
        [SerializeField] private Color _BaseAspectAreaColor = new(0, 1, 0, 0.3f);

        private Material _BaseAspectAreaMaterial;

        private void OnPostRender()
        {
            if (!_ShowBaseAspectArea) {
                return;
            }

            if (_BaseAspectAreaMaterial == null) {
                _BaseAspectAreaMaterial = new Material(Shader.Find("Hidden/Internal-Colored")) {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            GL.PushMatrix();
            GL.LoadOrtho();

            _BaseAspectAreaMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(_BaseAspectAreaColor);
            Rect viewport = GetBaseAspectViewportRect();
            GL.Vertex3(viewport.xMin, viewport.yMin, 0);
            GL.Vertex3(viewport.xMax, viewport.yMin, 0);
            GL.Vertex3(viewport.xMax, viewport.yMax, 0);
            GL.Vertex3(viewport.xMin, viewport.yMax, 0);
            GL.End();

            GL.PopMatrix();
        }

        private Rect GetBaseAspectViewportRect()
        {
            if (_Camera.aspect < BaseAspect) {
                // letterboxing
                float height = Screen.width / BaseAspect;
                float y = (Screen.height - height) * 0.5f;
                return new Rect(0, y / Screen.height, 1, height / Screen.height);
            } else {
                // pillarboxing
                float width = Screen.height * BaseAspect;
                float x = (Screen.width - width) * 0.5f;
                return new Rect(x / Screen.width, 0, width / Screen.width, 1);
            }
        }
#endif
    }
}
