using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UnityCommon
{
    /// <summary>
    /// 必要最低限の要素を再利用する軽量なリストビュー
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect))]
    public class ListView : UIBehaviour
    {
        private enum Direction { Horizontal, Vertical }
        [SerializeField] private Direction _Direction;
        [SerializeField] private ListItemView _ItemViewTemplate;
        [SerializeField] private float _ItemSpacing;
        [SerializeField] private float _RowMargin;
        [SerializeField] private int _MaxColumns = 1;

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
        public bool IsInfiniteScroll => ScrollRect.movementType == ScrollRect.MovementType.Unrestricted;

        /// <summary>
        /// 横スクロールかどうか
        /// </summary>
        public bool IsHorizontalScroll => _Direction == Direction.Horizontal;

        /// <summary>
        /// 行方向の要素のサイズ
        /// </summary>
        public float ItemRowSize => IsHorizontalScroll ? _ItemViewTemplate.Rect.sizeDelta.x : _ItemViewTemplate.Rect.sizeDelta.y;

        /// <summary>
        /// 列方向の要素のサイズ
        /// </summary>
        public float ItemColumnSize => IsHorizontalScroll ? _ItemViewTemplate.Rect.sizeDelta.y : _ItemViewTemplate.Rect.sizeDelta.x;

        /// <summary>
        /// 要素ごとの間隔
        /// </summary>
        public float ItemSpacing => _ItemSpacing;

        /// <summary>
        /// 行方向の余白のサイズ
        /// </summary>
        public float RowMargin => IsInfiniteScroll ? _ItemSpacing * 0.5f : _RowMargin;

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
        /// リストビューの行方向のサイズ
        /// </summary>
        public float ViewRowSize => IsHorizontalScroll ? Rect.sizeDelta.x : Rect.sizeDelta.y;

        /// <summary>
        /// リストビューの列方向のサイズ
        /// </summary>
        public float ViewColumnSize => IsHorizontalScroll ? Rect.sizeDelta.y : Rect.sizeDelta.x;

        /// <summary>
        /// コンテンツ領域の行方向のサイズ
        /// </summary>
        public float ContentRowSize
        {
            get => IsHorizontalScroll ? ScrollRect.content.sizeDelta.x : ScrollRect.content.sizeDelta.y;
            private set {
                Vector2 temp = ScrollRect.content.sizeDelta;
                if (IsHorizontalScroll) { temp.x = value; } else { temp.y = value; }
                ScrollRect.content.sizeDelta = temp;
            }
        }

        /// <summary>
        /// コンテンツ領域の行方向の位置
        /// </summary>
        public float ContentRowPosition
        {
            get => IsHorizontalScroll ? ScrollRect.content.anchoredPosition.x : -ScrollRect.content.anchoredPosition.y;
            set {
                Vector2 temp = ScrollRect.content.anchoredPosition;
                if (IsHorizontalScroll) { temp.x = value; } else { temp.y = -value; }
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
            if (dataIndex < 0 || dataIndex > _ItemDatas.Count
                || itemData == null || _ItemDatas.Contains(itemData)) {
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

            ScrollRect.horizontal = IsHorizontalScroll;
            ScrollRect.vertical = !IsHorizontalScroll;

            if (IsHorizontalScroll) {
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
                    if (IsHorizontalScroll) {
                        itemView.Rect.anchorMin = new Vector2(0, 0.5f);
                        itemView.Rect.anchorMax = new Vector2(0, 0.5f);
                        itemView.Rect.pivot = new Vector2(0.5f, 0.5f);
                    } else {
                        itemView.Rect.anchorMin = new Vector2(0.5f, 1);
                        itemView.Rect.anchorMax = new Vector2(0.5f, 1);
                        itemView.Rect.pivot = new Vector2(0.5f, 0.5f);
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

            int numColumns = CalcColumnCount();
            if (_MaxColumns > 0 && numColumns > _MaxColumns) { numColumns = _MaxColumns; }
            int numRows = (_ItemDatas.Count + numColumns - 1) / numColumns;
            ContentRowSize = CalcContentRowSize(numRows);
            RefreshListView(numRows, numColumns);
        }

        private void RefreshListView(int numRows, int numColumns)
        {
            if (numRows == 0) {
                return;
            }

            int startRow = Mathf.FloorToInt((-ContentRowPosition - RowMargin) / (ItemRowSize + ItemSpacing));
            if (!IsInfiniteScroll && startRow < 0) { startRow = 0; }

            int endRow;
            for (endRow = startRow; ; endRow++) {
                float rowPosition = CalcItemRowPosition(endRow);
                if (rowPosition <= -(ItemRowSize + ItemSpacing) * 0.5f) { startRow++; continue; }
                if (rowPosition >= ViewRowSize + (ItemRowSize + ItemSpacing) * 0.5f) { break; }
                if (!IsInfiniteScroll && endRow == numRows) { break; }
            }

            LinkedListNode<ListItemView> node = _VisibleItemViews.First;
            while (node != null) {
                LinkedListNode<ListItemView> nextNode = node.Next;
                int row = node.Value.Index / numColumns;
                if (row < startRow || row >= endRow
                    || (!IsInfiniteScroll && node.Value.Index >= _ItemDatas.Count)) {
                    RemoveVisibleItemView(node.Value);
                }
                node = nextNode;
            }

            for (int row = startRow; row < endRow; row++) {
                for (int column = 0; column < numColumns; column++) {
                    int index = numColumns * row + column;
                    if (!IsInfiniteScroll && index == _ItemDatas.Count) { break; }
                    float rowPosition = CalcItemRowPosition(row);
                    float columnPosition = CalcItemColumnPosition(column, numColumns);
                    SetVisibleItemView(index, rowPosition, columnPosition);
                }
            }
        }

        private void SetVisibleItemView(int index, float rowPosition, float columnPosition)
        {
            ListItemView itemView = _VisibleItemViews.FirstOrDefault(itemView => itemView.Index == index);
            if (itemView != null) {
                itemView.UpdatePosition(rowPosition, columnPosition);
            } else {
                itemView = _ItemViewPool.Get();
                itemView.Index = index;
                itemView.Data = _ItemDatas[CalcItemDataIndex(index)];
                itemView.UpdatePosition(rowPosition, columnPosition);
                itemView.OnVisible();
                LinkedListNode<ListItemView> prevNode = _VisibleItemViews.Nodes()
                    .Where(node => node.Value.Index < index)
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

        private int CalcColumnCount()
        {
            float contentColumnSize = IsHorizontalScroll ? ScrollRect.content.rect.height : ScrollRect.content.rect.width;
            return 1 + Mathf.FloorToInt(Mathf.Max(contentColumnSize - ItemColumnSize, 0) / (ItemColumnSize + ItemSpacing));
        }

        private float CalcContentRowSize(int numRows)
        {
            return RowMargin * 2 + ItemRowSize * numRows + ItemSpacing * Mathf.Max(numRows - 1, 0);
        }

        private int CalcItemDataIndex(int index)
        {
            if (IsInfiniteScroll) {
                return index < 0 ? _ItemDatas.Count - 1 + ((index + 1) % _ItemDatas.Count) : index % _ItemDatas.Count;
            } else {
                return Mathf.Clamp(index, 0, _ItemDatas.Count - 1);
            }
        }

        private float CalcItemRowPosition(int row)
        {
            return RowMargin + (ItemRowSize + ItemSpacing) * row + ContentRowPosition + ItemRowSize * 0.5f;
        }

        private float CalcItemColumnPosition(int column, int numColumns)
        {
            return (ItemColumnSize + ItemSpacing) * (-0.5f * (1 - numColumns % 2) - (numColumns - 1) / 2 + column);
        }
    }
}
