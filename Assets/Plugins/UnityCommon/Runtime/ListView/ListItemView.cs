using UnityEngine.EventSystems;

namespace UnityCommon
{
    /// <summary>
    /// ListViewの要素を表す
    /// </summary>
    public class ListItemView : UIBehaviour
    {
        internal int Index { get; set; }

        /// <summary>
        /// 要素に紐づくデータ
        /// </summary>
        public IListItemData Data { get; internal set; }

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
