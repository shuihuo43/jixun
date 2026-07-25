using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private bool showDebugInfo = true;

    private HashSet<Vector3Int> blocked = new();
    private Tilemap tilemap;
    private bool baked;

    static readonly Vector3Int[] Dirs = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
        new(1,1), new(-1,1), new(1,-1), new(-1,-1)
    };

    void Awake() { Bake(); }

    [ContextMenu("Bake")]
    public void Bake()
    {
        blocked.Clear();
        tilemap = null;

        foreach (var tm in FindObjectsOfType<Tilemap>())
        {
            if (!tm.CompareTag("Wall")) continue;
            tilemap = tm;
            Debug.Log($"Pathfinding: using Tilemap '{tm.name}', cellSize={tm.cellSize}, cellBounds={tm.cellBounds}");

            var cb = tm.cellBounds;
            for (int x = cb.xMin; x < cb.xMax; x++)
                for (int y = cb.yMin; y < cb.yMax; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (tm.GetTile(cell) != null)
                        blocked.Add(cell);
                }
        }

        baked = tilemap != null;
        Debug.Log($"Pathfinding: baked={baked}, blocked cells={blocked.Count}");
    }

    public Vector2 CellToWorld(Vector3Int c) => tilemap.GetCellCenterWorld(c);

    Vector3Int W2C(Vector2 p) => tilemap.WorldToCell(p);

    bool Walkable(Vector3Int c) => !blocked.Contains(c);

    public List<Vector3Int> FindPath(Vector2 from, Vector2 to)
    {
        if (!baked) return null;

        var start = W2C(from);
        var end   = W2C(to);

        if (showDebugInfo)
            Debug.Log($"FindPath: from={from} start={start}, to={to} end={end}, endWalkable={Walkable(end)}");

        if (!Walkable(end))
        {
            // 目标格是墙，尝试找最近可走格
            end = NearestWalkable(end);
            if (end == default) return null;
        }

        if (start == end) return null;

        var open = new List<Vector3Int> { start };
        var closed = new HashSet<Vector3Int>();
        var parent = new Dictionary<Vector3Int, Vector3Int>();
        var gCost = new Dictionary<Vector3Int, float> { [start] = 0f };
        var fCost = new Dictionary<Vector3Int, float> { [start] = H(start, end) };

        int steps = 0;
        while (open.Count > 0 && steps < 1000)
        {
            steps++;
            var cur = Best(open, fCost);
            if (cur == end) { debugPath = Build(parent, cur); return debugPath; }

            open.Remove(cur);
            closed.Add(cur);

            foreach (var d in Dirs)
            {
                var n = cur + d;
                if (closed.Contains(n) || !Walkable(n)) continue;

                float cost = (d.x != 0 && d.y != 0) ? 1.414f : 1f;

                // 对角防止穿墙角
                if (d.x != 0 && d.y != 0)
                {
                    if (!Walkable(cur + new Vector3Int(d.x, 0, 0)) || !Walkable(cur + new Vector3Int(0, d.y, 0)))
                        continue;
                }

                float tg = gCost[cur] + cost;
                if (!gCost.TryGetValue(n, out var old) || tg < old)
                {
                    parent[n] = cur;
                    gCost[n] = tg;
                    fCost[n] = tg + H(n, end);
                    if (!open.Contains(n)) open.Add(n);
                }
            }
        }

        if (showDebugInfo)
            Debug.Log($"FindPath: no path found, steps={steps}, open={open.Count}");

        return null;
    }

    Vector3Int NearestWalkable(Vector3Int cell)
    {
        for (int r = 1; r < 10; r++)
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    var n = cell + new Vector3Int(dx, dy, 0);
                    if (Walkable(n)) return n;
                }
        return default;
    }

    float H(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
        return dx + dy + (1.414f - 2f) * Mathf.Min(dx, dy);
    }

    Vector3Int Best(List<Vector3Int> set, Dictionary<Vector3Int, float> f)
    {
        var best = set[0];
        float bestV = f.ContainsKey(best) ? f[best] : float.MaxValue;
        for (int i = 1; i < set.Count; i++)
        {
            float v = f.ContainsKey(set[i]) ? f[set[i]] : float.MaxValue;
            if (v < bestV) { bestV = v; best = set[i]; }
        }
        return best;
    }

    List<Vector3Int> Build(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int cur)
    {
        var path = new List<Vector3Int> { cur };
        while (parent.TryGetValue(cur, out cur))
            path.Add(cur);
        path.Reverse();
        return path;
    }

    public List<Vector3Int> debugPath = new();

    void OnDrawGizmosSelected()
    {
        if (!baked) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        foreach (var cell in blocked)
            Gizmos.DrawWireCube(CellToWorld(cell), tilemap.cellSize * 0.9f);

        if (debugPath == null || debugPath.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 1; i < debugPath.Count; i++)
            Gizmos.DrawLine(CellToWorld(debugPath[i - 1]), CellToWorld(debugPath[i]));
    }
}
