using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEnemy : Photon.MonoBehaviour
{
    public string enemy;
    public Transform spawnspot;

    public string input;

    public void Update()
    {
        if (Input.GetButtonDown(input))
            PhotonNetwork.InstantiateSceneObject(enemy, spawnspot.position, spawnspot.rotation, 0, new object[0]);
    }
}
