using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEnemy : MonoBehaviour
{
    public GameObject enemy;
    public Transform spawnspot;

    public string input;

    public void Update()
    {
        if (Input.GetButtonDown(input))
            Instantiate(enemy, spawnspot.position, spawnspot.rotation);
    }
}
