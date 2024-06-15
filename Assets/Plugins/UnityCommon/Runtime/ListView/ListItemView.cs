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

        internal float Position
        {
            get {
                return ListView.IsHorizontalScroll ?
                    Rect.anchoredPosition.x + ListView.ScrollRect.content.anchoredPosition.x :
                    -(Rect.anchoredPosition.y + ListView.ScrollRect.content.anchoredPosition.y);
            }
            set {
                Vector2 temp = Rect.anchoredPosition;
                if (ListView.IsHorizontalScroll) {
                    temp.x = value - ListView.ScrollRect.content.anchoredPosition.x;
                } else {
                    temp.y = -value - ListView.ScrollRect.content.anchoredPosition.y;
                }
                Rect.anchoredPosition = temp;
            }
        }

        internal float Center => Position + ListView.ItemSize * 0.5f;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ListView = null;
            _Rect = null;
        }

        /// <summary>
        /// 要素の表示時に呼び出される
        /// </summary>
        public virtual void OnVisible() { }

        /// <summary>
        /// 要素の非表示時に呼び出される
        /// </summary>
        public virtual void OnInvisible() { }
    }
}
