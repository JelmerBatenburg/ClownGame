using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : Photon.MonoBehaviour
{
    [Header("PlayerSpawning")]
    public string player;
    public Transform[] spawnPoints;
    public LayerMask playerMask;
    public float notAllowedSpawnPointRange;
    public GameObject currentPlayer;

    [Header("ParticleSpawner")]
    public GameObject[] particles;
    public float particleLifetime;

    public void Start()
    {
        StartCoroutine(SpawnPlayer());
    }

    [PunRPC]
    public void SpawnParticle(int particleIndex, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        Destroy(Instantiate(particles[particleIndex], position, rotation, parent), particleLifetime);
    }

    public IEnumerator SpawnPlayer(float time = 0)
    {
        yield return new WaitForSeconds(time);
        if (currentPlayer)
            Destroy(currentPlayer);
        foreach (Transform spawnSpot in spawnPoints)
            if (!Physics.CheckSphere(spawnSpot.position, notAllowedSpawnPointRange, playerMask))
            {
                PhotonNetwork.Instantiate(player, spawnSpot.position, spawnSpot.rotation, 0);
                break;
            }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        foreach (Transform spawnSpot in spawnPoints)
            Gizmos.DrawWireSphere(spawnSpot.position, notAllowedSpawnPointRange);
    }
}
