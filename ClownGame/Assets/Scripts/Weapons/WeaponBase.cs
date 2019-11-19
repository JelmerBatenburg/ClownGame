using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : Photon.MonoBehaviour
{
    public WeaponStatsScriptableObject[] weapons;
    public int currentWeapon;
    public bool activeFire;
    public int currentAmmo;
    public Transform weapon;
    public string enemyTag;
    public AudioSource source;
    public LayerMask enemyMask;
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

        switch (weapons[currentWeapon].fireType)
        {
            case WeaponStatsScriptableObject.FireType.burst:
                for (int i = 0; i < weapons[currentWeapon].burstRounds; i++)
                {
                    Shoot();
                    yield return new WaitForSeconds(weapons[currentWeapon].burstFireDelay);
                }
                break;
            default:
                Shoot();
                break;
        }

        yield return new WaitForSeconds(weapons[currentWeapon].fireRate);
        if (Input.GetButton("Fire1") && weapons[currentWeapon].fireType == WeaponStatsScriptableObject.FireType.auto && currentAmmo > 0) 
            StartCoroutine(Shooting());
        else
            activeFire = false;
    }

    public void Shoot()
    {
        switch (weapons[currentWeapon].projectileType)
        {
            case WeaponStatsScriptableObject.ProjectileType.rayCast:
                RaycastHit hit = new RaycastHit();
                source.PlayOneShot(weapons[currentWeapon].clip);
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, weapons[currentWeapon].raycastFireLength))
                    if(hit.transform.tag == enemyTag && !weapons[currentWeapon].explodingBullets)
                    {
                        GameObject currentObject = hit.transform.gameObject;
                        while (!currentObject.GetComponent<RemovableLimbs>())
                            currentObject = currentObject.transform.parent.gameObject;
                        currentObject.GetComponent<RemovableLimbs>().DoDamage(hit.collider, weapons[currentWeapon].damage, hit.point - Camera.main.transform.forward, weapons[currentWeapon].force);
                    }
                    else if(weapons[currentWeapon].explodingBullets)
                    {
                        Collider[] enemyParts = Physics.OverlapSphere(hit.point, weapons[currentWeapon].explosionRadius, enemyMask);
                        GameObject.FindWithTag("Manager").GetPhotonView().RPC("CallScreenShake", PhotonTargets.All, weapons[currentWeapon].explosionScreenShakeTime, weapons[currentWeapon].explosionScreenShakeIntensity);
                        foreach(Collider col in enemyParts)
                        {
                            GameObject currentObject = col.gameObject;
                            while (!currentObject.GetComponent<RemovableLimbs>())
                                currentObject = currentObject.transform.parent.gameObject;
                            Vector3 explosionPoint = hit.point + (Vector3.down / 3) - (Camera.main.transform.forward / 4);
                            currentObject.GetComponent<RemovableLimbs>().DoDamage(col, weapons[currentWeapon].explosionDamage, explosionPoint, weapons[currentWeapon].force);
                        }
                    }
                break;
        }
        photonView.RPC("Recoil", PhotonTargets.All, weapons[currentWeapon].backwardsRecoil, weapons[currentWeapon].horizontalRotationRecoil);
    }

    [PunRPC]
    public void Recoil(float backwardsRecoil,float horizontalRecoil)
    {
        weapon.Translate(-Vector3.forward * backwardsRecoil);
        weapon.Rotate(-Vector3.right * Random.Range(horizontalRecoil / 2f, horizontalRecoil));
    }
}
