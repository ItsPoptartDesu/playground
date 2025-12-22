using UnityEngine;

public class GameEntry : MonoBehaviour {
    // Static instance for singleton pattern
    private static GameEntry _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting;
    [SerializeField] private LevelBuilder _levelBuilder;
    [SerializeField] private ObjectManager _objectManager;
    [SerializeField] private UIManager myUIManager;
    [SerializeField] private GridManager myGridManager;

    public LevelBuilder GetLevelBuilder() { return _levelBuilder; }

    public ObjectManager GetObjectManager() { return _objectManager; }
    public GridManager GetGridManager() { return myGridManager; }

    public Vector2Int MapSize = Vector2Int.zero;
    // Public property to access the singleton instance
    public static GameEntry Instance {
        get {
            if (_applicationIsQuitting) {
                Debug.LogWarning("GameEntry Instance access attempted while application is quitting. Returning null.");
                return null;
            }

            lock (_lock) {
                if (_instance == null) {
                    // Look for existing instance in the scene
                    _instance = FindFirstObjectByType<GameEntry>();

                    if (_instance == null) {
                        // Create new GameObject with GameEntry component
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<GameEntry>();
                        singletonObject.name = "GameEntry";

                        // Ensure it persists across scenes
                        DontDestroyOnLoad(singletonObject);

                        Debug.Log("GameEntry Singleton instance created.");
                    }
                }
                return _instance;
            }
        }
    }

    // Optional: Example initialization method
    private void Awake() {
        // Prevent duplicate instances
        if (_instance != null && _instance != this) {
            Debug.LogWarning("GameEntry Another instance already exists. Destroying this duplicate.");
            Destroy(gameObject);
            return;
        }

        // Initialize core systems or settings here
        Initialize();
    }

    private void Initialize() {
        Debug.Log("GameEntry Initialize system.");
        myUIManager.StartGame();
    }

    private GameEntry() { }

    private void OnDestroy() {
        if (_instance == this) {
            _applicationIsQuitting = true;
        }
    }
    public void BlowUp() {
        _objectManager.ShutDown();
        myGridManager.ShutDown();
        _levelBuilder.perlinHeightMapSettings.seed++;
        Build();
    }
    public void Build() {
        Debug.Log($"Game Entry passing W:{MapSize.x} - H:{MapSize.y}");
        myGridManager.GenerateMap(MapSize.x , MapSize.y);
        //_levelBuilder.DecorateMap(MapSize.x , MapSize.y);
    }
}