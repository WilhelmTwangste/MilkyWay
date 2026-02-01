using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChinkPlacer : MonoBehaviour
{
    public Transform Player;
    public Chinks[] ChinksPref;
    public Chinks ChinkFirst;

    private List<Chinks> spawnedChinks = new List<Chinks>();

    private void Spawn()
    {
        Chinks newChink = Instantiate(ChinksPref[Random.Range(0, ChinksPref.Length)]);
        newChink.transform.position = spawnedChinks[spawnedChinks.Count - 1].End.position - newChink.Begin.localPosition;
        spawnedChinks.Add(newChink);

        if (spawnedChinks.Count >= 30)
        {
            Destroy(spawnedChinks[0].gameObject);
            spawnedChinks.RemoveAt(0);
        }
    }

    private void Start()
    {
        spawnedChinks.Add(ChinkFirst);
    }

    private void Update()
    {
        if (Player.position.y > spawnedChinks[spawnedChinks.Count - 1].End.position.y - 10)
        {
            Spawn();
        }
    }
}