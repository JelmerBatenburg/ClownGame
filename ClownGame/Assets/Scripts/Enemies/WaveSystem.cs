using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveSystem : Photon.MonoBehaviour
{
    public WaveInformation[] waves;
    public AreaInformation[] areas;
    public Vector2Int waveHordeSize;
    public float spawnDelay;
    public float hordeDelay;
    public float waveDelay;
    public Text waveDisplay;
    public Text infoDisplay;
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
            photonView.RPC("DisplayInfo", PhotonTargets.All, currentWave + 1);
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
                        if (currentlySpawned[enemy].GetComponent<BaseEnemy>().health <= 0)
                        {
                            currentlySpawned.RemoveAt(enemy);
                            enemy--;
                            photonView.RPC("DisplayInfo", PhotonTargets.All, currentlySpawned.Count.ToString() + " Alive");
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
                photonView.RPC("DisplayInfo", PhotonTargets.All, currentlySpawned.Count.ToString() + " Alive");
                yield return new WaitForSeconds(spawnDelay);
            }

            while(currentlySpawned.Count > 0)
            {
                for (int enemy = 0; enemy < currentlySpawned.Count; enemy++)
                    if (currentlySpawned[enemy].GetComponent<BaseEnemy>().health <= 0)
                    {
                        currentlySpawned.RemoveAt(enemy);
                        enemy--;
                        photonView.RPC("DisplayInfo", PhotonTargets.All, currentlySpawned.Count.ToString() + " Alive");
                    }
                yield return null;
            }

            for (float i = 0; i < waveDelay; i += Time.deltaTime)
            {
                Vector2Int returnTime = RecalculateTime(waveDelay - i);
                photonView.RPC("DisplayInfo", PhotonTargets.All, returnTime.x + ":" +((returnTime.y < 10)? "0" : "") + returnTime.y);
                yield return null;
            }
        }
    }

    public Vector2Int RecalculateTime(float time)
    {
        int newTime = Mathf.RoundToInt(time);

        Vector2Int returnValue = new Vector2Int();

        returnValue.x = Mathf.FloorToInt(newTime / 60);
        returnValue.y = newTime - (returnValue.x * 60);
        return returnValue;
    }

    [PunRPC,HideInInspector]
    public void DisplayInfo(int round)
    {
        waveDisplay.text = "Wave: " + round.ToString();
    }

    [PunRPC,HideInInspector]
    public void DisplayInfo(string message)
    {
        infoDisplay.text = message;
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
