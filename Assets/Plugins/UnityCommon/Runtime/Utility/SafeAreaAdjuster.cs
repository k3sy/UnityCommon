using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    /// <summary>
    /// RectTransformをSafeArea内に収める
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdjuster : UIBehaviour
    {
        [Flags]
        private enum Edge
        {
            Top = 1,
            Bottom = 2,
            Left = 4,
            Right = 8,
        }

        [SerializeField] private Edge _IgnoreEdges = 0;
        [SerializeField] private bool _AutoScaling = false;

        private Rect _LastSafeArea = new(0, 0, 0, 0);
        private DrivenRectTransformTracker _DrivenRectTransformTracker;

        protected override void Start()
        {
            base.Start();
            ApplySafeArea();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _DrivenRectTransformTracker.Clear();
        }

        private void Update()
        {
#if !UNITY_EDITOR
            if (Screen.safeArea == _LastSafeArea) {
                return;
            }
#endif
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            var panel = transform as RectTransform;
            Rect safeArea = Screen.safeArea;
            _LastSafeArea = safeArea;
            _DrivenRectTransformTracker.Clear();

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x = _IgnoreEdges.HasFlag(Edge.Left) ? 0 : anchorMin.x / Screen.width;
            anchorMin.y = _IgnoreEdges.HasFlag(Edge.Bottom) ? 0 : anchorMin.y / Screen.height;
            anchorMax.x = _IgnoreEdges.HasFlag(Edge.Right) ? 1 : anchorMax.x / Screen.width;
            anchorMax.y = _IgnoreEdges.HasFlag(Edge.Top) ? 1 : anchorMax.y / Screen.height;
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = Vector2.zero;
            panel.localScale = Vector3.one;

            if (_AutoScaling) {
                float heightRate = anchorMax.y - anchorMin.y;
                var oldSize = new Vector3(panel.rect.width, panel.rect.height);
                var newSize = oldSize * heightRate;
                panel.sizeDelta = oldSize - newSize;
                panel.localScale = new Vector3(heightRate, heightRate, 1);
            }

            DrivenTransformProperties properties = DrivenTransformProperties.None;
            properties |= DrivenTransformProperties.AnchorMinX;
            properties |= DrivenTransformProperties.AnchorMinY;
            properties |= DrivenTransformProperties.AnchorMaxX;
            properties |= DrivenTransformProperties.AnchorMaxY;
            properties |= DrivenTransformProperties.AnchoredPositionX;
            properties |= DrivenTransformProperties.AnchoredPositionY;
            properties |= DrivenTransformProperties.SizeDeltaX;
            properties |= DrivenTransformProperties.SizeDeltaY;
            properties |= DrivenTransformProperties.Scale;
            _DrivenRectTransformTracker.Add(this, panel, properties);
        }
    }
}
