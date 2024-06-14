using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    /// <summary>
    /// ListViewの要素を表す
    /// </summary>
    public class ListItemView : UIBehaviour
    {
        private ListView _ListView;

        /// <summary>
        /// 要素のインデックス
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// 要素に紐づくデータ
        /// </summary>
        public IListItemData Data { get; internal set; }

        /// <summary>
        /// ListView上の位置
        /// </summary>
        public float Position
        {
            get {
                var rt = transform as RectTransform;
                return _ListView.Direction == ListDirection.Horizontal ?
                    rt.anchoredPosition.x + _ListView.content.anchoredPosition.x :
                    -(rt.anchoredPosition.y + _ListView.content.anchoredPosition.y);
            }
            internal set {
                var rt = transform as RectTransform;
                Vector2 temp = rt.anchoredPosition;
                if (_ListView.Direction == ListDirection.Horizontal) {
                    temp.x = value - _ListView.content.anchoredPosition.x;
                } else {
                    temp.y = -value - _ListView.content.anchoredPosition.y;
                }
                rt.anchoredPosition = temp;
            }
        }

        /// <summary>
        /// ListView上の中心位置
        /// </summary>
        public float Center => Position + _ListView.ItemSize * 0.5f;

        protected override void Awake()
        {
            base.Awake();

            _ListView = GetComponentInParent<ListView>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _ListView = null;
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
