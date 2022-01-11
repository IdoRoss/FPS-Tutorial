using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This class is a singletone responsible of giving players a spawn point
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Transform[] spawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        // Dont show the spawnpoints
        foreach(Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0,spawnPoints.Length)];
    }

    
}
