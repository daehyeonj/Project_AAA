using UnityEngine;

[DisallowMultipleComponent]
public sealed class AppRoot : MonoBehaviour
{
    private static AppRoot _instance;

    [SerializeField] private GameSessionRoot _sessionRoot;

    public static AppRoot Instance => _instance;
    public GameSessionRoot SessionRoot => _sessionRoot;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSessionRoot();
        _sessionRoot.RefreshSceneBinding();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void EnsureSessionRoot()
    {
        if (_sessionRoot == null)
        {
            _sessionRoot = GetComponentInChildren<GameSessionRoot>(true);
        }

        if (_sessionRoot == null)
        {
            GameObject sessionObject = new GameObject("GameSessionRoot");
            sessionObject.transform.SetParent(transform, false);
            _sessionRoot = sessionObject.AddComponent<GameSessionRoot>();
        }
    }
}
