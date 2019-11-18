﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : Photon.MonoBehaviour
{
    public WeaponStatsScriptableObject stats;
    public bool activeFire;
    public int currentAmmo;
    public Transform weapon;
    public string enemyTag;
    public AudioSource source;
    [Header("WeaponSway")]
    public float weaponSwayStrenght;
    public float weaponLerpSpeed;
    public float weaponJumpWeight;
    public Rigidbody playerRig;

    public void WeaponSway()
    {
        if (photonView.isMine)
            weapon.Translate(new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y") - (playerRig.velocity.y * weaponJumpWeight)) * weaponSwayStrenght * Time.deltaTime);
        weapon.position = Vector3.Lerp(weapon.position, weapon.parent.position, Time.deltaTime * weaponLerpSpeed);
        weapon.rotation = Quaternion.Lerp(weapon.rotation, weapon.parent.rotation, Time.deltaTime * weaponLerpSpeed);
    }

    public void Update()
    {
        if (!activeFire && Input.GetButtonDown("Fire1") && photonView.isMine)
            StartCoroutine(Shooting());
        WeaponSway();
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
                source.PlayOneShot(stats.clip);
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, stats.raycastFireLength))
                    if(hit.transform.tag == enemyTag)
                    {
                        GameObject currentObject = hit.transform.gameObject;
                        while (!currentObject.GetComponent<RemovableLimbs>())
                            currentObject = currentObject.transform.parent.gameObject;
                        currentObject.GetComponent<RemovableLimbs>().DoDamage(hit.collider, stats.damage, hit.point - Camera.main.transform.forward);
                    }
                break;
        }

        photonView.RPC("Recoil", PhotonTargets.All, stats.backwardsRecoil, stats.horizontalRotationRecoil);
    }

    [PunRPC]
    public void Recoil(float backwardsRecoil,float horizontalRecoil)
    {
        weapon.Translate(-Vector3.forward * backwardsRecoil);
        weapon.Rotate(-Vector3.right * Random.Range(horizontalRecoil / 2f, horizontalRecoil));
    }
}
