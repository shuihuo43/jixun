using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PanelData panelData;

    [Serializable]
    public struct EntityEntry
    {
        public string key;
        public GameObject prefab;
    }

    /// <summary>实体字典（编辑器可编辑）</summary>
    public List<EntityEntry> entityList = new();

    [Header("房间")]
    public GameObject startRoomPrefab;
    public GameObject[] roomPrefabs;
    public GameObject[] level1BossRooms;

    private Room currentRoom;
    private int currentRoomIndex = -1;
    private int roomsCleared;
    public Player player;

    public enum GameState { Playing, Paused }
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (_, _) => Invoke(nameof(GameInit), 0.1f);
        GameInit();
    }

    void GameInit()
    {
        player = FindObjectOfType<Player>();
        if (player == null) { Invoke(nameof(GameInit), 0.1f); return; }

        // 清空前场景残留
        if (player.entityRoot != null)
            foreach (Transform t in player.entityRoot.transform)
                Destroy(t.gameObject);
        if (BloodCanvas.Instance != null)
            BloodCanvas.Instance.ClearStamps();

        SpawnRoom(startRoomPrefab);
        if (currentRoom != null) currentRoom.isStartRoom = true;
    }

    void SpawnRoom(GameObject prefab, int index = -1)
    {
        if (prefab == null) return;

        if (currentRoom != null)
        {
            currentRoom.OnExit -= OnRoomExit;
            Destroy(currentRoom.gameObject);
        }

        // 清空实体、敌人、血迹、元素实体
        if (player != null && player.entityRoot != null)
            foreach (Transform t in player.entityRoot.transform)
                Destroy(t.gameObject);
        var atkRoot = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (atkRoot != null)
            foreach (Transform t in atkRoot.transform)
                Destroy(t.gameObject);
        foreach (var e in FindObjectsOfType<Enemy>())
            Destroy(e.gameObject);
        BloodCanvas.Instance?.ClearStamps();

        currentRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();
        currentRoom.OnExit += OnRoomExit;
        currentRoomIndex = index;

        // 新房间的 Tilemap 路径重算
        foreach (var pf in FindObjectsOfType<EnemyPathfinding>())
            pf.Bake();

        if (player != null) currentRoom.Init(player);
    }

    void OnRoomExit()
    {
        Debug.Log("[GameManager] OnRoomExit fired");

        // 不统计起始房间和 Boss 房间
        if (currentRoom != null && !currentRoom.isStartRoom && !currentRoom.IsBoss)
            roomsCleared++;

        // 5 个房间后刷 Boss
        if (roomsCleared >= 5 && level1BossRooms != null && level1BossRooms.Length > 0)
        {
            roomsCleared = 0;
            int pick = UnityEngine.Random.Range(0, level1BossRooms.Length);
            Debug.Log($"[GameManager] Boss room: {level1BossRooms[pick]?.name}");
            SpawnRoom(level1BossRooms[pick], pick);
            return;
        }

        if (roomPrefabs == null || roomPrefabs.Length == 0) { Debug.LogWarning("roomPrefabs empty"); return; }

        int normalPick;
        do { normalPick = UnityEngine.Random.Range(0, roomPrefabs.Length); }
        while (roomPrefabs.Length > 1 && normalPick == currentRoomIndex);

        Debug.Log($"[GameManager] Spawning random room: {roomPrefabs[normalPick]?.name}");
        SpawnRoom(roomPrefabs[normalPick], normalPick);
    }

    public void ChangeRoom(int index, Player player)
    {
        if (currentRoom != null) Destroy(currentRoom.gameObject);
        if (index < 0 || index >= roomPrefabs.Length) return;
        var obj = Instantiate(roomPrefabs[index], Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(null); // 挂场景根
        obj.transform.localPosition = Vector3.zero;
        currentRoom = obj.GetComponent<Room>();
        currentRoom.Init(player);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing) Pause();
            else Resume();
        }

        // 调试：F7 跳到下一关
        if (Input.GetKeyDown(KeyCode.F7))
        {
            OnRoomExit();
        }
    }

    public void Pause()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>从列表获取实体预制体</summary>
    public GameObject GetEntity(string key)
    {
        foreach (var e in entityList)
            if (e.key == key) return e.prefab;
        return null;
    }

    public void Resume()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke(CurrentState);
    }
}
