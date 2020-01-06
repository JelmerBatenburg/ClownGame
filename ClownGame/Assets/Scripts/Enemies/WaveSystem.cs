using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : Photon.MonoBehaviour
{
    public WaveInformation[] waves;
    public AreaInformation[] areas;
    public Vector2Int waveHordeSize;
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
        for (int currentWave = 0; currentWave < waves.Length; currentWave++)
        {
            List<GameObject> currentlySpawned = new List<GameObject>();
            AreaInformation area = areas[waves[currentWave].area];
            int currentspawner = Random.Range(0, area.spawnpoints.Length);
            int spawned = 0;
            int spawnAmount = Random.Range(waveHordeSize.x,waveHordeSize.y);
            for (int i = 0; i < waves[currentWave].spawnAmount; i++)
            {
                while(currentlySpawned.Count >= areas[waves[currentWave].area].maxEnemies)
                {
                    for (int enemy = 0; enemy < currentlySpawned.Count; enemy++)
                        if(currentlySpawned[enemy] == null)
                        {
                            currentlySpawned.RemoveAt(enemy);
                            enemy--;
                        }
                    yield return null;
                }

                if(spawned >= spawnAmount)
                {
                    currentspawner = Random.Range(0, area.spawnpoints.Length);
                    spawned = 0;
                    spawnAmount = Random.Range(waveHordeSize.x, waveHordeSize.y);
                    yield return new WaitForSeconds(hordeDelay);
                }

                GameObject g = PhotonNetwork.Instantiate(enemy, area.spawnpoints[currentspawner].position, Quaternion.identity, 0);
                currentlySpawned.Add(g);
                spawned++;
                yield return new WaitForSeconds(spawnDelay);
            }

            while(currentlySpawned.Count > 0)
            {
                for (int enemy = 0; enemy < currentlySpawned.Count; enemy++)
                    if (currentlySpawned[enemy] == null)
                    {
                        currentlySpawned.RemoveAt(enemy);
                        enemy--;
                    }
                yield return null;
            }

            yield return new WaitForSeconds(waveDelay);
        }
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
