using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityCommon
{
    /// <summary>
    /// スナップスクロールするリストビュー
    /// </summary>
    public class ListViewSnap : ListView, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private float _LerpDuration = 0.2f;

        [Serializable]
        public class ItemDataEvent : UnityEvent<IListItemData> { }
        /// <summary>
        /// スナップする要素を変更するときに発生するイベント
        /// </summary>
        public ItemDataEvent OnChangeItemData = new();

        private enum State { Idle, JumpTo, MoveTo, Lerping, Dragging }
        private State _State;
        private IListItemData _DestItemData;
        private ListItemView _NearestItemView;
        private float _LerpStartTime;
        private float _LerpStartPosition;
        private float _LerpEndPosition;

        /// <summary>
        /// スナップしている要素のデータ
        /// </summary>
        public IListItemData SnappedItemData { get; private set; }

        /// <summary>
        /// 指定したデータの要素を即時にスナップする
        /// </summary>
        /// <param name="itemData"></param>
        public void JumpTo(IListItemData itemData)
        {
            if (!ItemDatas.Contains(itemData)) {
                return;
            }
            _DestItemData = itemData;
            _State = State.JumpTo;
        }

        /// <summary>
        /// 指定したデータの要素を補間でスナップする
        /// </summary>
        /// <param name="itemData"></param>
        public void MoveTo(IListItemData itemData)
        {
            if (!ItemDatas.Contains(itemData)) {
                return;
            }
            _DestItemData = itemData;
            _State = State.MoveTo;
        }

        protected override void Start()
        {
            base.Start();

            _State = State.JumpTo;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _State = State.Idle;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && IsActive()) {
                _State = State.Dragging;
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && _State == State.Dragging) {
                _State = State.Idle;
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (!IsActive()) {
                return;
            }

            if (!VisibleItemViews.Any()) {
                if (SnappedItemData != null) {
                    OnChangeItemData.Invoke(null);
                    SnappedItemData = null;
                }
                return;
            }

            float snapPosition = GetSnapPosition();
            _NearestItemView = GetNearestItemView(snapPosition);

            float snapVector = snapPosition - _NearestItemView.RowCenter;
            float snapDistance = Mathf.Abs(snapVector);
            if (_NearestItemView.Data != SnappedItemData && snapDistance < ItemRowSize) {
                OnChangeItemData.Invoke(_NearestItemView.Data);
                SnappedItemData = _NearestItemView.Data;
            }

            switch (_State) {
                case State.Idle:
                    if (IsBouncing()) { break; }
                    if (snapDistance <= 0.01f) { break; }
                    if (ScrollRect.velocity.magnitude > snapDistance / _LerpDuration) { break; }
                    ScrollRect.StopMovement();
                    StartLerping(ContentRowPosition + snapVector);
                    break;
                case State.JumpTo: {
                    ScrollRect.StopMovement();
                    int snapItemIndex = GetSnapItemIndex(_DestItemData);
                    ContentRowPosition = GetSnappedContentPosition(snapPosition, snapItemIndex);
                    _State = State.Idle;
                    break;
                }
                case State.MoveTo: {
                    ScrollRect.StopMovement();
                    int snapItemIndex = GetSnapItemIndex(_DestItemData);
                    float endPosition = GetSnappedContentPosition(snapPosition, snapItemIndex);
                    StartLerping(endPosition);
                    break;
                }
                case State.Lerping:
                    UpdateLerping();
                    break;
            }
        }

        private bool IsBouncing()
        {
            if (ScrollRect.movementType == ScrollRect.MovementType.Elastic) {
                if (ContentRowPosition > 0.01f
                    || ContentRowPosition + Mathf.Max(ContentRowSize - ViewRowSize, 0) < -0.01f) {
                    return true;
                }
            }
            return false;
        }

        private void StartLerping(float endPosition)
        {
            _LerpStartTime = Time.time;
            _LerpStartPosition = ContentRowPosition;
            _LerpEndPosition = endPosition;
            _State = State.Lerping;
        }

        private void UpdateLerping()
        {
            float t = (Time.time - _LerpStartTime) / _LerpDuration;
            if (t < 1) {
                ContentRowPosition = Mathf.Lerp(_LerpStartPosition, _LerpEndPosition, t);
            } else {
                ContentRowPosition = _LerpEndPosition;
                _State = State.Idle;
            }
        }

        private float GetSnapPosition()
        {
            return ItemRowSize * 0.5f + (ViewRowSize - ItemRowSize) * 0.5f;
        }

        private ListItemView GetNearestItemView(float snapPosition)
        {
            return VisibleItemViews
                .OrderBy(itemView => Mathf.Abs(itemView.RowCenter - snapPosition))
                .First();
        }

        private int GetSnapItemIndex(IListItemData itemData)
        {
            if (itemData == _NearestItemView.Data) { return _NearestItemView.Index; }
            int diff = ItemDatas.IndexOf(itemData) - ItemDatas.IndexOf(_NearestItemView.Data);
            int sign = diff < 0 ? -1 : 1;
            diff *= sign;
            if (diff <= ItemDatas.Count / 2) { return _NearestItemView.Index + diff * sign; }
            return _NearestItemView.Index - (ItemDatas.Count - diff) * sign;
        }

        private float GetSnappedContentPosition(float snapPosition, int snapItemIndex)
        {
            return ContentRowPosition - (ItemRowSize + ItemSpacing) * (snapItemIndex - _NearestItemView.Index)
                + (snapPosition - _NearestItemView.RowCenter);
        }
    }
}
