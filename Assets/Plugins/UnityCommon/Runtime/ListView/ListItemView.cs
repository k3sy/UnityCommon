using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    /// <summary>
    /// ListViewの要素を表す
    /// </summary>
    public class ListItemView : UIBehaviour
    {
        private RectTransform _Rect;
        /// <summary>
        /// 要素の矩形情報
        /// </summary>
        protected internal RectTransform Rect
        {
            get {
                if (_Rect == null) { _Rect = GetComponent<RectTransform>(); }
                return _Rect;
            }
        }

        private ListView _ListView;
        /// <summary>
        /// 要素を保持するListView
        /// </summary>
        /// <value></value>
        protected ListView ListView
        {
            get {
                if (_ListView == null) { _ListView = GetComponentInParent<ListView>(); }
                return _ListView;
            }
        }

        /// <summary>
        /// 要素のインデックス
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// 要素に紐づくデータ
        /// </summary>
        public IListItemData Data { get; internal set; }

        /// <summary>
        /// ListView上の行方向の位置
        /// </summary>
        public float RowPosition => ListView.IsHorizontalScroll
            ? Rect.anchoredPosition.x + ListView.ScrollRect.content.anchoredPosition.x
            : -(Rect.anchoredPosition.y + ListView.ScrollRect.content.anchoredPosition.y);

        /// <summary>
        /// ListView上の列方向の位置
        /// </summary>
        public float ColumnPosition => ListView.IsHorizontalScroll
            ? -(Rect.anchoredPosition.y + ListView.ScrollRect.content.anchoredPosition.y)
            : Rect.anchoredPosition.x + ListView.ScrollRect.content.anchoredPosition.x;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ListView = null;
            _Rect = null;
        }

        /// <summary>
        /// 要素の位置を更新する
        /// </summary>
        /// <param name="rowPosition">ListView上の行方向の位置</param>
        /// <param name="columnPosition">ListView上の列方向の位置</param>
        protected internal virtual void UpdatePosition(float rowPosition, float columnPosition)
        {
            Vector2 temp = Rect.anchoredPosition;
            if (ListView.IsHorizontalScroll) {
                temp.x = rowPosition - ListView.ScrollRect.content.anchoredPosition.x;
                temp.y = -columnPosition - ListView.ScrollRect.content.anchoredPosition.y;
            } else {
                temp.x = columnPosition - ListView.ScrollRect.content.anchoredPosition.x;
                temp.y = -rowPosition - ListView.ScrollRect.content.anchoredPosition.y;
            }
            Rect.anchoredPosition = temp;
        }

        /// <summary>
        /// 要素の表示時に呼び出される
        /// </summary>
        protected internal virtual void OnVisible() { }

        /// <summary>
        /// 要素の非表示時に呼び出される
        /// </summary>
        protected internal virtual void OnInvisible() { }
    }
}
