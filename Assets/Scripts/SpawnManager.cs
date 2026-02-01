using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnEnemy;
    [SerializeField] private Transform[] spawnPoint;

    public float startSpawn;
    private float spawnTime;

    public int spawnCount;
    public static int spawnNow;

    private int randEnemy;
    private int randPoint;

    void Start()
    {
        spawnTime = startSpawn;
        Update();
        spawnNow = 0;
    }

    void Update()
    {
        if (spawnTime <= 0 && spawnNow < spawnCount)
        {
            randEnemy = Random.Range(0, spawnEnemy.Length);
            randPoint = Random.Range(0, spawnPoint.Length);

            GameObject enemy = Instantiate(spawnEnemy[randEnemy], spawnPoint[randPoint].transform.position, Quaternion.identity);
            Debug.Log("Enemy created at position: " + enemy.transform.position);

            spawnTime = startSpawn;
            spawnNow++;
        }
        else
        {
            spawnTime -= Time.deltaTime;
        }
    }
}