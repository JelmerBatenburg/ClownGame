using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : Photon.MonoBehaviour
{
    public WaveInformation[] waves;
    public AreaInformation[] areas;
    public List<GameObject> currentlySpawned = new List<GameObject>();
    public int currentWave;
    public int waveHordeSizeMax;
    public float spawnDelay;
    public float hordeDelay;
    public float waveDelay;
    public string enemy;

    public void Start()
    {
        if (photonView.isMine)
            StartCoroutine(SpawnEnemies());
    }

    public IEnumerator SpawnEnemies()
    {
        int spawned = 0;
        while (spawned < waves[currentWave].spawnAmount)
        {
            for (int enemyIndex = 0; enemyIndex < currentlySpawned.Count; enemyIndex++)
                        if(currentlySpawned[enemyIndex] == null)
                        {
                            currentlySpawned.RemoveAt(enemyIndex);
                            enemyIndex--;
                        }

            int spawnAmount = Random.Range(0, waveHordeSizeMax);
            if (spawned + spawnAmount >= waves[currentWave].spawnAmount)
                spawnAmount = waves[currentWave].spawnAmount - spawned;

            AreaInformation area = areas[waves[currentWave].area];
            int spawner = Random.Range(0, area.spawnpoints.Length);
            for (int i = 0; i < spawnAmount; i++)
            {
                while (currentlySpawned.Count >= area.maxEnemies)
                {
                    for (int enemyIndex = 0; enemyIndex < currentlySpawned.Count; enemyIndex++)
                        if(currentlySpawned[enemyIndex] == null)
                        {
                            currentlySpawned.RemoveAt(enemyIndex);
                            enemyIndex--;
                        }

                    yield return null;
                }
                currentlySpawned.Add(PhotonNetwork.Instantiate(enemy, area.spawnpoints[spawner].position, Quaternion.identity, 0));
                spawned++;
                yield return new WaitForSeconds(spawnDelay);
            }
            yield return new WaitForSeconds(hordeDelay);
        }
        while(currentlySpawned.Count > 0)
        {
            for (int enemyIndex = 0; enemyIndex < currentlySpawned.Count; enemyIndex++)
                if (currentlySpawned[enemyIndex] == null)
                {
                    currentlySpawned.RemoveAt(enemyIndex);
                    enemyIndex--;
                }

            yield return null;
        }
        Debug.Log("NextWave");
        yield return new WaitForSeconds(waveDelay);
        currentWave++;
        StartCoroutine(SpawnEnemies());
    }

    public void OnDrawGizmos()
    {
        foreach (AreaInformation area in areas)
            Gizmos.DrawWireSphere(area.center, area.size);
    }

    [System.Serializable]
    public class WaveInformation
    {
        public int spawnAmount;
        public int area;
    }

    [System.Serializable]
    public class AreaInformation
    {
        public Transform[] spawnpoints;
        public Vector3 center;
        public float size;
        public int maxEnemies;
    }
}
