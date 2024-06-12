using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
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
    public class ListView : ScrollRect
    {
        [SerializeField] private ListItemView _ItemViewTemplate;
        [SerializeField] private float _ItemSpacing;
        [SerializeField] private float _Margin;
        [SerializeField] private ListDirection _Direction;

        private readonly List<IListItemData> _ItemDatas = new();
        private readonly LinkedList<ListItemView> _VisibleItemViews = new();
        private ObjectPool<ListItemView> _ItemViewPool;
        private bool _NeedToRefresh;

        /// <summary>
        /// テンプレートとなる要素の位置
        /// </summary>
        protected float ItemSize
        {
            get {
                var rt = _ItemViewTemplate.transform as RectTransform;
                return _Direction == ListDirection.Horizontal ? rt.sizeDelta.x : rt.sizeDelta.y;
            }
        }

        /// <summary>
        /// 要素間の距離
        /// </summary>
        protected float ItemSpacing => _ItemSpacing;

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
        /// 無限スクロールかどうか
        /// </summary>
        protected internal bool IsInfiniteScroll => movementType == MovementType.Unrestricted;

        /// <summary>
        /// ビューポートのサイズ
        /// </summary>
        protected float ViewportSize => _Direction == ListDirection.Horizontal ? viewport.rect.width : viewport.rect.height;

        /// <summary>
        /// コンテンツ領域の位置
        /// </summary>
        protected float ContentPosition
        {
            get => _Direction == ListDirection.Horizontal ? content.anchoredPosition.x : -content.anchoredPosition.y;
            set {
                Vector2 temp = content.anchoredPosition;
                if (_Direction == ListDirection.Horizontal) { temp.x = value; } else { temp.y = -value; }
                content.anchoredPosition = temp;
            }
        }

        private float ContentSize
        {
            get => _Direction == ListDirection.Horizontal ? content.sizeDelta.x : content.sizeDelta.y;
            set {
                Vector2 temp = content.sizeDelta;
                if (_Direction == ListDirection.Horizontal) { temp.x = value; } else { temp.y = value; }
                content.sizeDelta = temp;
            }
        }

        private float Margin => IsInfiniteScroll ? _ItemSpacing * 0.5f : _Margin;

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

            horizontal = _Direction == ListDirection.Horizontal;
            vertical = _Direction == ListDirection.Vertical;

            _ItemViewTemplate.gameObject.SetActive(false);

            _ItemViewPool = new ObjectPool<ListItemView>(
                createFunc: () => {
                    ListItemView itemView = Instantiate(_ItemViewTemplate, content);
                    var rt = itemView.transform as RectTransform;
                    if (_Direction == ListDirection.Horizontal) {
                        rt.anchorMin = new Vector2(0, 0.5f);
                        rt.anchorMax = new Vector2(0, 0.5f);
                        rt.pivot = new Vector2(0, 0.5f);
                    } else {
                        rt.anchorMin = new Vector2(0.5f, 1);
                        rt.anchorMax = new Vector2(0.5f, 1);
                        rt.pivot = new Vector2(0.5f, 1);
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

            onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(Vector2 value)
        {
            _NeedToRefresh = true;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (!IsActive() || ViewportSize == 0 || !_NeedToRefresh) {
                return;
            }
            _NeedToRefresh = false;

            RefreshContentSize();
            RefreshListView();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ItemViewPool?.Clear();
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
                if (itemPosition <= -(ItemSize + _ItemSpacing)) { continue; }
                if (ViewportSize + _ItemSpacing <= itemPosition) { break; }
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
