﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UnityCommon
{
    /// <summary>
    /// リストの方向
    /// </summary>
    public enum ListDirection { Horizontal, Vertical }

    /// <summary>
    /// 必要最低限の要素を再利用する軽量なリストビュー
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class ListView : UIBehaviour
    {
        [SerializeField] private ListItemView _ItemViewTemplate;
        [SerializeField] private float _ItemSpacing;
        [SerializeField] private float _Margin;
        [SerializeField] private ListDirection _Direction;

        private readonly List<IListItemData> _ItemDatas = new();
        private readonly LinkedList<ListItemView> _VisibleItemViews = new();
        private ObjectPool<ListItemView> _ItemViewPool;
        private bool _NeedToRefresh;

        private RectTransform _Rect;
        /// <summary>
        /// リストビューの矩形情報
        /// </summary>
        protected RectTransform Rect
        {
            get {
                if (_Rect == null) { _Rect = GetComponent<RectTransform>(); }
                return _Rect;
            }
        }

        private ScrollRect _ScrollRect;
        /// <summary>
        /// リストビューを構築するScrollRect
        /// </summary>
        protected internal ScrollRect ScrollRect
        {
            get {
                if (_ScrollRect == null) { _ScrollRect = GetComponent<ScrollRect>(); }
                return _ScrollRect;
            }
        }

        /// <summary>
        /// 無限スクロールかどうか
        /// </summary>
        protected internal bool IsInfiniteScroll => ScrollRect.movementType == ScrollRect.MovementType.Unrestricted;

        /// <summary>
        /// 要素のサイズ
        /// </summary>
        protected internal float ItemSize => _Direction == ListDirection.Horizontal ?
            _ItemViewTemplate.Rect.sizeDelta.x :
            _ItemViewTemplate.Rect.sizeDelta.y;

        /// <summary>
        /// 要素ごとの間隔
        /// </summary>
        protected float ItemSpacing => _ItemSpacing;

        /// <summary>
        /// 余白のサイズ
        /// </summary>
        protected float Margin => IsInfiniteScroll ? _ItemSpacing * 0.5f : _Margin;

        /// <summary>
        /// リストの方向
        /// </summary>
        protected internal ListDirection Direction => _Direction;

        private ReadOnlyCollection<IListItemData> _ReadOnlyItemDatas;
        /// <summary>
        /// 格納しているデータのコレクション
        /// </summary>
        public ReadOnlyCollection<IListItemData> ItemDatas
        {
            get {
                _ReadOnlyItemDatas ??= new ReadOnlyCollection<IListItemData>(_ItemDatas);
                return _ReadOnlyItemDatas;
            }
        }

        /// <summary>
        /// 表示している要素のシーケンス
        /// </summary>
        public IEnumerable<ListItemView> VisibleItemViews
        {
            get {
                foreach (ListItemView itemView in _VisibleItemViews) {
                    yield return itemView;
                }
            }
        }

        /// <summary>
        /// リストビューのサイズ
        /// </summary>
        protected float ViewSize => _Direction == ListDirection.Horizontal ?
            Rect.sizeDelta.x :
            Rect.sizeDelta.y;

        /// <summary>
        /// コンテンツ領域のサイズ
        /// </summary>
        protected float ContentSize
        {
            get => _Direction == ListDirection.Horizontal ?
                ScrollRect.content.sizeDelta.x :
                ScrollRect.content.sizeDelta.y;
            private set {
                Vector2 temp = ScrollRect.content.sizeDelta;
                if (_Direction == ListDirection.Horizontal) { temp.x = value; } else { temp.y = value; }
                ScrollRect.content.sizeDelta = temp;
            }
        }

        /// <summary>
        /// コンテンツ領域の位置
        /// </summary>
        protected float ContentPosition
        {
            get => _Direction == ListDirection.Horizontal ?
                ScrollRect.content.anchoredPosition.x :
                -ScrollRect.content.anchoredPosition.y;
            set {
                Vector2 temp = ScrollRect.content.anchoredPosition;
                if (_Direction == ListDirection.Horizontal) { temp.x = value; } else { temp.y = -value; }
                ScrollRect.content.anchoredPosition = temp;
            }
        }

        /// <summary>
        /// 末尾にデータを追加する
        /// </summary>
        /// <param name="itemData"></param>
        public void AddItemData(IListItemData itemData)
        {
            if (itemData == null || _ItemDatas.Contains(itemData)) {
                return;
            }

            LinkedListNode<ListItemView> node = _VisibleItemViews.First;
            while (node != null) {
                if (node.Value.Index < 0) {
                    node.Value.Index -= 1 + (-node.Value.Index - 1) / _ItemDatas.Count;
                } else {
                    node.Value.Index += node.Value.Index / _ItemDatas.Count;
                }
                node = node.Next;
            }

            _ItemDatas.Add(itemData);

            _NeedToRefresh = true;
        }

        /// <summary>
        /// 指定した位置にデータを挿入する
        /// </summary>
        /// <param name="dataIndex"></param>
        /// <param name="itemData"></param>
        public void InsertItemData(int dataIndex, IListItemData itemData)
        {
            if (dataIndex < 0 || dataIndex > _ItemDatas.Count ||
                    itemData == null || _ItemDatas.Contains(itemData)) {
                return;
            }

            if (dataIndex == _ItemDatas.Count) {
                AddItemData(itemData);
                return;
            }

            LinkedListNode<ListItemView> node = _VisibleItemViews.First;
            while (node != null) {
                if (node.Value.Index < 0) {
                    if (dataIndex <= CalcItemDataIndex(node.Value.Index)) {
                        node.Value.Index -= (-node.Value.Index - 1) / _ItemDatas.Count;
                    } else {
                        node.Value.Index -= 1 + (-node.Value.Index - 1) / _ItemDatas.Count;
                    }
                } else {
                    if (dataIndex <= CalcItemDataIndex(node.Value.Index)) {
                        node.Value.Index += 1 + node.Value.Index / _ItemDatas.Count;
                    } else {
                        node.Value.Index += node.Value.Index / _ItemDatas.Count;
                    }
                }
                node = node.Next;
            }

            _ItemDatas.Insert(dataIndex, itemData);

            _NeedToRefresh = true;
        }

        /// <summary>
        /// 指定した位置のデータを削除する
        /// </summary>
        /// <param name="dataIndex"></param>
        public void RemoveItemData(int dataIndex)
        {
            if (dataIndex < 0 || dataIndex >= _ItemDatas.Count) {
                return;
            }

            IListItemData itemData = _ItemDatas[dataIndex];

            LinkedListNode<ListItemView> node = _VisibleItemViews.First;
            while (node != null) {
                LinkedListNode<ListItemView> nextNode = node.Next;
                if (node.Value.Data == itemData) {
                    RemoveVisibleItemView(node.Value);
                } else {
                    if (node.Value.Index < 0) {
                        if (dataIndex < CalcItemDataIndex(node.Value.Index)) {
                            node.Value.Index += (-node.Value.Index - 1) / _ItemDatas.Count;
                        } else {
                            node.Value.Index += 1 + (-node.Value.Index - 1) / _ItemDatas.Count;
                        }
                    } else {
                        if (dataIndex < CalcItemDataIndex(node.Value.Index)) {
                            node.Value.Index -= 1 + node.Value.Index / _ItemDatas.Count;
                        } else {
                            node.Value.Index -= node.Value.Index / _ItemDatas.Count;
                        }
                    }
                }
                node = nextNode;
            }

            _ItemDatas.Remove(itemData);

            _NeedToRefresh = true;
        }

        /// <summary>
        /// 指定したデータを削除する
        /// </summary>
        /// <param name="itemData"></param>
        public void RemoveItemData(IListItemData itemData)
        {
            int dataIndex = _ItemDatas.IndexOf(itemData);
            RemoveItemData(dataIndex);
        }

        /// <summary>
        /// すべてのデータを削除する
        /// </summary>
        public void RemoveAllItemDatas()
        {
            LinkedListNode<ListItemView> node = _VisibleItemViews.First;
            while (node != null) {
                LinkedListNode<ListItemView> nextNode = node.Next;
                RemoveVisibleItemView(node.Value);
                node = nextNode;
            }

            _ItemDatas.Clear();

            _NeedToRefresh = true;
        }

        protected override void Start()
        {
            base.Start();

            ScrollRect.horizontal = _Direction == ListDirection.Horizontal;
            ScrollRect.vertical = _Direction == ListDirection.Vertical;

            if (_Direction == ListDirection.Horizontal) {
                ScrollRect.content.anchorMin = new Vector2(0, 0);
                ScrollRect.content.anchorMax = new Vector2(0, 1);
                ScrollRect.content.pivot = new Vector2(0, 1);
            } else {
                ScrollRect.content.anchorMin = new Vector2(0, 1);
                ScrollRect.content.anchorMax = new Vector2(1, 1);
                ScrollRect.content.pivot = new Vector2(0, 1);
            }

            _ItemViewTemplate.gameObject.SetActive(false);

            _ItemViewPool = new ObjectPool<ListItemView>(
                createFunc: () => {
                    ListItemView itemView = Instantiate(_ItemViewTemplate, ScrollRect.content);
                    if (_Direction == ListDirection.Horizontal) {
                        itemView.Rect.anchorMin = new Vector2(0, 0.5f);
                        itemView.Rect.anchorMax = new Vector2(0, 0.5f);
                        itemView.Rect.pivot = new Vector2(0, 0.5f);
                    } else {
                        itemView.Rect.anchorMin = new Vector2(0.5f, 1);
                        itemView.Rect.anchorMax = new Vector2(0.5f, 1);
                        itemView.Rect.pivot = new Vector2(0.5f, 1);
                    }
                    return itemView;
                },
                actionOnGet: itemView => itemView.gameObject.SetActive(true),
                actionOnRelease: itemView => itemView.gameObject.SetActive(false),
                actionOnDestroy: itemView => itemView.gameObject.Destroy(),
                collectionCheck: true
            );
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ScrollRect.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ScrollRect.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(Vector2 value)
        {
            _NeedToRefresh = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ItemViewPool.Clear();
            _ScrollRect = null;
            _Rect = null;
        }

        public override bool IsActive()
        {
            return base.IsActive() && ScrollRect.IsActive();
        }

        protected virtual void LateUpdate()
        {
            if (!IsActive() || !_NeedToRefresh) {
                return;
            }
            _NeedToRefresh = false;

            RefreshContentSize();
            RefreshListView();
        }

        private void RefreshContentSize()
        {
            ContentSize = Margin * 2 + ItemSize * _ItemDatas.Count + _ItemSpacing * Mathf.Max(_ItemDatas.Count - 1, 0);
        }

        private void RefreshListView()
        {
            if (_ItemDatas.Count == 0) {
                return;
            }

            int startIndex = (int)((-ContentPosition - Margin) / (ItemSize + _ItemSpacing));
            if (IsInfiniteScroll) {
                if (startIndex <= 0) { startIndex -= 1; }
            } else {
                if (startIndex < 0) { startIndex = 0; }
            }

            int endIndex;
            for (endIndex = startIndex; ; endIndex++) {
                float itemPosition = CalcItemPosition(endIndex);
                if (itemPosition <= -(ItemSize + _ItemSpacing)) { startIndex++; continue; }
                if (ViewSize + _ItemSpacing <= itemPosition) { break; }
                if (!IsInfiniteScroll && endIndex == _ItemDatas.Count) { break; }
            }

            LinkedListNode<ListItemView> node = _VisibleItemViews.First;
            while (node != null) {
                LinkedListNode<ListItemView> nextNode = node.Next;
                if (!(startIndex <= node.Value.Index && node.Value.Index < endIndex)) {
                    RemoveVisibleItemView(node.Value);
                }
                node = nextNode;
            }

            for (int itemIndex = startIndex; itemIndex < endIndex; itemIndex++) {
                float itemPosition = CalcItemPosition(itemIndex);
                SetVisibleItemView(itemIndex, itemPosition);
            }
        }

        private void SetVisibleItemView(int itemIndex, float itemPosition)
        {
            ListItemView itemView = _VisibleItemViews.FirstOrDefault(itemView => itemView.Index == itemIndex);
            if (itemView != null) {
                itemView.Position = itemPosition;
            } else {
                itemView = _ItemViewPool.Get();
                itemView.Position = itemPosition;
                itemView.Index = itemIndex;
                itemView.Data = _ItemDatas[CalcItemDataIndex(itemIndex)];
                itemView.OnVisible();
                LinkedListNode<ListItemView> prevNode = _VisibleItemViews.Nodes()
                    .Where(node => node.Value.Index < itemIndex)
                    .OrderBy(node => node.Value.Index)
                    .LastOrDefault();
                if (prevNode != null) {
                    _VisibleItemViews.AddAfter(prevNode, itemView);
                } else {
                    _VisibleItemViews.AddFirst(itemView);
                }
            }
        }

        private void RemoveVisibleItemView(ListItemView itemView)
        {
            itemView.OnInvisible();
            _ItemViewPool.Release(itemView);
            _VisibleItemViews.Remove(itemView);
        }

        private int CalcItemDataIndex(int itemIndex)
        {
            if (IsInfiniteScroll) {
                return itemIndex < 0 ?
                    _ItemDatas.Count - 1 + ((itemIndex + 1) % _ItemDatas.Count) :
                    itemIndex % _ItemDatas.Count;
            } else {
                return Mathf.Clamp(itemIndex, 0, _ItemDatas.Count - 1);
            }
        }

        private float CalcItemPosition(int itemIndex)
        {
            return Margin + (ItemSize + _ItemSpacing) * itemIndex + ContentPosition;
        }
    }
}
