using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// MonoBehaviourを継承したシングルトン
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _Instance;

        /// <summary>
        /// シングルトンのインスタンスを取得する。
        /// </summary>
        public static T Instance
        {
            get {
                if (_Instance == null) {
                    _Instance = (T)FindObjectOfType(typeof(T));
                    if (_Instance == null) {
                        var go = new GameObject();
                        _Instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                        go.hideFlags = HideFlags.HideInHierarchy;
                    } else {
                        Debug.Log($"[Singleton] {typeof(T).Name} instance already created: {_Instance.gameObject.name}");
                    }
                }
                return _Instance;
            }
        }

        /// <summary>
        /// シングルトンのインスタンスが存在するかどうかを判定する。
        /// </summary>
        public static bool Exists => _Instance != null;

        protected virtual void Awake()
        {
            if (_Instance == null) {
                _Instance = this as T;
            } else if (Instance != this) {
                gameObject.Destroy();
            }
        }

        protected virtual void OnDestroy()
        {
            if (_Instance == this) {
                _Instance = null;
            }
        }
    }
}
