using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RoomSpawner : MonoBehaviour
{
    public ExitPortal exitPortal;
    [Header("敌人预制体")]
    public GameObject[] enemyPrefabs;


    [Header("刷怪点")]
    public Transform[] spawnPoints;



    [Header("波数")]
    public int minWave = 2;
    public int maxWave = 5;


    [Header("每波数量")]
    public int minEnemyCount = 3;
    public int maxEnemyCount = 8;


    [Header("生成间隔")]
    public float spawnInterval = 0.5f;



    private bool started = false;

    private List<GameObject> aliveEnemies = new List<GameObject>();


    void Start()
    {
        StartRoom();
    }

    public void StartRoom()
    {
        if (started) return;
        started = true;
        StartCoroutine(SpawnWaves());
    }



    IEnumerator SpawnWaves()
    {

        int waves = Random.Range(
            minWave,
            maxWave + 1
        );


        for (int w = 0; w < waves; w++)
        {

            int enemyCount = Random.Range(
                minEnemyCount,
                maxEnemyCount + 1
            );



            for (int i = 0; i < enemyCount; i++)
            {

                SpawnEnemy();

                yield return new WaitForSeconds(
                    spawnInterval
                );
            }



            //等待这一波全部死亡（兼容被直接 Destroy 的敌人）
            yield return new WaitUntil(
                () =>
                {
                    aliveEnemies.RemoveAll(e => e == null);
                    return aliveEnemies.Count == 0;
                }
            );


        }


        RoomClear();

    }




    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (prefab == null) return;

        GameObject enemy = Instantiate(prefab, GetRandomSpawnPoint(), Quaternion.identity);
        aliveEnemies.Add(enemy);

        var e = enemy.GetComponent<Enemy>();
        if (e != null && e.resource != null)
            e.resource.OnDeath += () => aliveEnemies.Remove(enemy);
    }




    Vector3 GetRandomSpawnPoint()
    {

        return spawnPoints[
            Random.Range(
                0,
                spawnPoints.Length
            )
        ].position;

    }



    void RemoveEnemy(GameObject enemy)
    {

        aliveEnemies.Remove(enemy);

    }





    void RoomClear()
    {

        Debug.Log("房间完成");


        //墙保持存在


        // room complete

    }


}