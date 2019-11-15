using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public WeaponStatsScriptableObject stats;
    public bool activeFire;
    public int currentAmmo;
    public Transform weapon;
    public string enemyTag;

    public void Update()
    {
        if (!activeFire && Input.GetButtonDown("Fire1"))
            StartCoroutine(Shooting());
    }

    public IEnumerator Shooting()
    {
        activeFire = true;

        switch (stats.fireType)
        {
            case WeaponStatsScriptableObject.FireType.burst:
                for (int i = 0; i < stats.burstRounds; i++)
                {
                    Shoot();
                    yield return new WaitForSeconds(stats.burstFireDelay);
                }
                break;
            default:
                Shoot();
                break;
        }

        yield return new WaitForSeconds(stats.fireRate);
        if (Input.GetButton("Fire1") && stats.fireType == WeaponStatsScriptableObject.FireType.auto && currentAmmo != 0) 
            StartCoroutine(Shooting());
        else
            activeFire = false;
    }

    public void Shoot()
    {
        switch (stats.projectileType)
        {
            case WeaponStatsScriptableObject.ProjectileType.rayCast:
                RaycastHit hit = new RaycastHit();
                Debug.Log("Fired");
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, stats.raycastFireLength))
                    if(hit.transform.tag == enemyTag)
                    {
                        Debug.Log("Hit Enemy");
                        GameObject currentObject = hit.transform.gameObject;
                        while (!currentObject.GetComponent<RemovableLimbs>())
                            currentObject = currentObject.transform.parent.gameObject;
                        currentObject.GetComponent<RemovableLimbs>().DoDamage(hit.collider, stats.damage);
                    }
                break;
        }
            
        weapon.Translate(-Vector3.forward * stats.backwardsRecoil);
        weapon.Rotate(-Vector3.right * Random.Range(stats.horizontalRotationRecoil / 2f, stats.horizontalRotationRecoil));
    }
}
