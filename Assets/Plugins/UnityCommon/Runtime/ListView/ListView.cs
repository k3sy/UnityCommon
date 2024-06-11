using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UnityCommon
{
    /// <summary>
    /// 必要最低限の要素を再利用する軽量なリストビュー
    /// </summary>
    public class ListView : ScrollRect
    {
        [SerializeField] private ListItemView _ItemViewTemplate;
        [SerializeField] private float _ItemSpacing;
        [SerializeField] private float _Margin;

        private enum Direction { Horizontal, Vertical }
        [SerializeField] private Direction _Direction;

        private readonly List<IListItemData> _ItemDatas = new();
        private readonly LinkedList<ListItemView> _VisibleItemViews = new();
        private ObjectPool<ListItemView> _ItemViewPool;
        private bool _NeedToRefreshListView;

        private float ContentPosition => _Direction == Direction.Horizontal ? content.anchoredPosition.x : -content.anchoredPosition.y;

        private bool IsInfiniteScroll => movementType == MovementType.Unrestricted;

        private float ViewSize
        {
            get {
                var rt = transform as RectTransform;
                return _Direction == Direction.Horizontal ? rt.sizeDelta.x : rt.sizeDelta.y;
            }
        }

        private float ItemSize
        {
            get {
                var rt = _ItemViewTemplate.transform as RectTransform;
                return _Direction == Direction.Horizontal ? rt.sizeDelta.x : rt.sizeDelta.y;
            }
        }

        private float Margin => IsInfiniteScroll ? _ItemSpacing * 0.5f : _Margin;

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

            RefreshContentSize();
            _NeedToRefreshListView = true;
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

            RefreshContentSize();
            _NeedToRefreshListView = true;
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

            RefreshContentSize();
            _NeedToRefreshListView = true;
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

            RefreshContentSize();
            _NeedToRefreshListView = true;
        }

        protected override void Start()
        {
            base.Start();

            horizontal = _Direction == Direction.Horizontal;
            vertical = _Direction == Direction.Vertical;

            _ItemViewTemplate.gameObject.SetActive(false);

            _ItemViewPool = new ObjectPool<ListItemView>(
                createFunc: () => Instantiate(_ItemViewTemplate, content),
                actionOnGet: itemView => itemView.gameObject.SetActive(true),
                actionOnRelease: itemView => itemView.gameObject.SetActive(false),
                actionOnDestroy: itemView => itemView.gameObject.Destroy(),
                collectionCheck: true
            );
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(Vector2 value)
        {
            _NeedToRefreshListView = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ItemViewPool?.Clear();
        }

        private void Update()
        {
            if (!IsActive() || !_NeedToRefreshListView) {
                return;
            }
            _NeedToRefreshListView = false;

            RefreshListView();
        }

        private void RefreshContentSize()
        {
            float contentSize = Margin * 2 + ItemSize * _ItemDatas.Count + _ItemSpacing * Mathf.Max(_ItemDatas.Count - 1, 0);
            if (_Direction == Direction.Horizontal) {
                content.sizeDelta = new Vector2(contentSize, content.sizeDelta.y);
            } else {
                content.sizeDelta = new Vector2(content.sizeDelta.x, contentSize);
            }
        }

        private void RefreshListView()
        {
            if (_ItemDatas.Count == 0) {
                return;
            }

            int startIndex = CalcStartIndex();
            int endIndex;
            for (endIndex = startIndex; ; endIndex++) {
                if (!IsInfiniteScroll && endIndex == _ItemDatas.Count) { break; }
                float itemPosition = CalcItemPosition(endIndex);
                int itemSide = GetItemPositionSide(itemPosition);
                if (itemSide < 0) { continue; }
                if (itemSide > 0) { break; }
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
                SetItemViewPosition(itemView, itemPosition);
            } else {
                itemView = _ItemViewPool.Get();
                SetItemViewPosition(itemView, itemPosition);
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

        private float GetItemViewPosition(ListItemView itemView)
        {
            var rt = itemView.transform as RectTransform;
            if (_Direction == Direction.Horizontal) {
                return ContentPosition + rt.anchoredPosition.x;
            } else {
                return ContentPosition - rt.anchoredPosition.y;
            }
        }

        private void SetItemViewPosition(ListItemView itemView, float itemPosition)
        {
            var rt = itemView.transform as RectTransform;
            if (_Direction == Direction.Horizontal) {
                rt.anchoredPosition = new Vector2(itemPosition - ContentPosition, rt.anchoredPosition.y);
            } else {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -(itemPosition - ContentPosition));
            }
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

        private int CalcStartIndex()
        {
            int startIndex = (int)((-ContentPosition - Margin) / (ItemSize + _ItemSpacing));
            if (IsInfiniteScroll) {
                if (startIndex <= 0) { startIndex -= 1; }
            } else {
                if (startIndex < 0) { startIndex = 0; }
            }
            return startIndex;
        }

        private float CalcItemPosition(int itemIndex)
        {
            return ContentPosition + Margin + (ItemSize + _ItemSpacing) * itemIndex;
        }

        private int GetItemPositionSide(float itemPosition)
        {
            if (itemPosition <= -(ItemSize + _ItemSpacing)) { return -1; }
            if (ViewSize + _ItemSpacing <= itemPosition) { return 1; }
            return 0;
        }
    }
}
