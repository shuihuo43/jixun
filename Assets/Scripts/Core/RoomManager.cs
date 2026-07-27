using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("区块")]
    [SerializeField] private int chunkSize = 8;

    [Header("房间")]
    [SerializeField] private int minRoomChunks = 2;
    [SerializeField] private int maxRoomChunks = 4;
    [SerializeField] private int roomCount = 8;

    [Header("连接")]
    [SerializeField] private int doorWidth = 3;
    [SerializeField] private int corridorWidth = 3;

    [Header("墙")]
    [SerializeField] private RoomWall roomWall;

    [Header("门")]
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private Transform doorRoot;


    private List<Room> rooms = new();
    private HashSet<Vector2Int> floor = new();


    private class Room
    {
        public RectInt chunkRect;
        public RectInt tileRect;

        public Room parent;

        public Vector2Int door;
        public Vector2Int parentDoor;

        public Vector2Int doorDir;
    }


    [ContextMenu("Generate")]
    public void Generate()
    {
        if (roomWall == null || roomWall.tilemap == null || roomWall.wallTile == null)
            return;

        roomWall.tilemap.ClearAllTiles();

        rooms.Clear();
        floor.Clear();

        CreateFirstRoom();

        int fail = 0;

        while (rooms.Count < roomCount && fail < 200)
        {
            if (CreateRoom())
                fail = 0;
            else
                fail++;
        }

        BuildTileMap();
        SpawnDoors();

        roomWall.tilemap.RefreshAllTiles();
    }



    private void CreateFirstRoom()
    {
        int w = Random.Range(minRoomChunks, maxRoomChunks + 1);
        int h = Random.Range(minRoomChunks, maxRoomChunks + 1);

        RectInt rect = new RectInt(-w / 2, -h / 2, w, h);

        AddRoom(rect, null, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero);
    }



    private bool CreateRoom()
    {
        Room parent = rooms[Random.Range(0, rooms.Count)];

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2Int dir = dirs[Random.Range(0, dirs.Length)];

        int w = Random.Range(minRoomChunks, maxRoomChunks + 1);
        int h = Random.Range(minRoomChunks, maxRoomChunks + 1);

        int x = parent.chunkRect.x;
        int y = parent.chunkRect.y;


        if (dir == Vector2Int.right)
            x = parent.chunkRect.xMax;

        if (dir == Vector2Int.left)
            x = parent.chunkRect.x - w;

        if (dir == Vector2Int.up)
            y = parent.chunkRect.yMax;

        if (dir == Vector2Int.down)
            y = parent.chunkRect.y - h;


        RectInt rect = new RectInt(x, y, w, h);


        foreach (Room r in rooms)
        {
            if (rect.Overlaps(r.chunkRect))
                return false;
        }


        if (!CreateDoor(parent, rect, dir, out Vector2Int parentDoor, out Vector2Int childDoor))
            return false;


        AddRoom(rect, parent, childDoor, parentDoor, dir);

        return true;
    }




    private void AddRoom(RectInt rect, Room parent, Vector2Int door, Vector2Int parentDoor, Vector2Int dir)
    {
        Room room = new();

        room.chunkRect = rect;

        room.tileRect = new RectInt(
            rect.x * chunkSize + 1,
            rect.y * chunkSize + 1,
            rect.width * chunkSize - 1,
            rect.height * chunkSize - 1
        );

        room.parent = parent;
        room.door = door;
        room.parentDoor = parentDoor;
        room.doorDir = dir;

        rooms.Add(room);
    }




    private bool CreateDoor(Room parent, RectInt child, Vector2Int dir, out Vector2Int parentDoor, out Vector2Int childDoor)
    {
        RectInt a = parent.tileRect;

        RectInt b = new RectInt(
            child.x * chunkSize + 1,
            child.y * chunkSize + 1,
            child.width * chunkSize - 1,
            child.height * chunkSize - 1
        );


        parentDoor = Vector2Int.zero;
        childDoor = Vector2Int.zero;


        if (dir.x != 0)
        {
            int yMin = Mathf.Max(a.yMin, b.yMin);
            int yMax = Mathf.Min(a.yMax, b.yMax);

            if (yMax <= yMin + doorWidth)
                return false;


            int y = Random.Range(yMin + 1, yMax - 1);


            if (dir.x > 0)
            {
                parentDoor = new Vector2Int(a.xMax - 1, y);
                childDoor = new Vector2Int(b.xMin, y);
            }
            else
            {
                parentDoor = new Vector2Int(a.xMin, y);
                childDoor = new Vector2Int(b.xMax - 1, y);
            }
        }
        else
        {
            int xMin = Mathf.Max(a.xMin, b.xMin);
            int xMax = Mathf.Min(a.xMax, b.xMax);

            if (xMax <= xMin + doorWidth)
                return false;


            int x = Random.Range(xMin + 1, xMax - 1);


            if (dir.y > 0)
            {
                parentDoor = new Vector2Int(x, a.yMax - 1);
                childDoor = new Vector2Int(x, b.yMin);
            }
            else
            {
                parentDoor = new Vector2Int(x, a.yMin);
                childDoor = new Vector2Int(x, b.yMax - 1);
            }
        }

        return true;
    }

    private void BuildTileMap()
    {
        floor.Clear();


        foreach (Room r in rooms)
        {
            for (int x = r.tileRect.xMin; x < r.tileRect.xMax; x++)
            {
                for (int y = r.tileRect.yMin; y < r.tileRect.yMax; y++)
                {
                    floor.Add(new Vector2Int(x, y));
                }
            }
        }



        foreach (Room r in rooms)
        {
            if (r.parent == null)
                continue;


            CarveDoor(r.parentDoor, r.doorDir);
            CarveDoor(r.door, -r.doorDir);
            // 打通门中间的墙
            Vector2Int wallCell = (r.parentDoor + r.door) / 2;
            floor.Add(wallCell);

            CarveCorridor(r.parentDoor, r.door, r.doorDir);
        }



        HashSet<Vector2Int> walls = new();

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1,1),
            new Vector2Int(-1,1),
            new Vector2Int(1,-1),
            new Vector2Int(-1,-1)
        };


        foreach (Vector2Int p in floor)
        {
            foreach (Vector2Int d in dirs)
            {
                Vector2Int check = p + d;

                if (!floor.Contains(check))
                    walls.Add(check);
            }
        }



        foreach (Vector2Int w in walls)
        {
            roomWall.tilemap.SetTile(new Vector3Int(w.x, w.y, 0), roomWall.wallTile);
        }
    }





    private void CarveDoor(Vector2Int pos, Vector2Int dir)
    {
        Vector2Int side;

        if (dir.x != 0)
            side = Vector2Int.up;
        else
            side = Vector2Int.right;


        int half = doorWidth / 2;


        for (int i = -half; i <= half; i++)
        {
            floor.Add(pos + side * i);
        }
    }





    private void CarveCorridor(Vector2Int start, Vector2Int end, Vector2Int dir)
    {
        Vector2Int p = start;


        while (p.x != end.x)
        {
            p.x += p.x < end.x ? 1 : -1;
            AddCorridorArea(p);
        }


        while (p.y != end.y)
        {
            p.y += p.y < end.y ? 1 : -1;
            AddCorridorArea(p);
        }
    }





    private void AddCorridorArea(Vector2Int p)
    {
        int half = corridorWidth / 2;


        for (int x = -half; x <= half; x++)
        {
            for (int y = -half; y <= half; y++)
            {
                floor.Add(new Vector2Int(p.x + x, p.y + y));
            }
        }
    }





    void SpawnDoors()
    {
        if (doorPrefab == null || roomWall?.tilemap == null) return;

        if (doorRoot != null)
            for (int i = doorRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(doorRoot.GetChild(i).gameObject);

        foreach (var r in rooms)
        {
            if (r.parent == null) continue;

            Vector2Int mid = (r.parentDoor + r.door) / 2;
            Vector3 worldPos = roomWall.tilemap.GetCellCenterWorld(new Vector3Int(mid.x, mid.y, 0));
            float angle = r.doorDir.x != 0 ? 90f : 0f;
            Instantiate(doorPrefab, worldPos, Quaternion.Euler(0, 0, angle), doorRoot);
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        if (roomWall != null && roomWall.tilemap != null)
            roomWall.tilemap.ClearAllTiles();

        if (doorRoot != null)
            for (int i = doorRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(doorRoot.GetChild(i).gameObject);
    }
}