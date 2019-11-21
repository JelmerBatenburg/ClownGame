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
    public Transform cam;
    public string enemyTag;
    public AudioSource source;
    public LayerMask enemyMask;
    [Header("WeaponSway")]
    public float weaponSwayStrenght;
    public float weaponLerpSpeed;
    public float weaponJumpWeight;
    public Rigidbody playerRig;
    [Header("Reload")]
    public WeaponObjectInformation info;
    public string reloadInput;

    public void WeaponSway()
    {
        if (photonView.isMine)
            weapon.Translate(new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y") - (playerRig.velocity.y * weaponJumpWeight)) * weaponSwayStrenght * Time.deltaTime);
        weapon.position = Vector3.Lerp(weapon.position, weapon.parent.position, Time.deltaTime * weaponLerpSpeed);
        weapon.rotation = Quaternion.Lerp(weapon.rotation, weapon.parent.rotation, Time.deltaTime * weaponLerpSpeed);
    }

    public void Update()
    {
        if (!activeFire && Input.GetButtonDown("Fire1") && photonView.isMine && currentAmmo > 0)
            StartCoroutine(Shooting());
        if (!activeFire && Input.GetButtonDown(reloadInput) && photonView.isMine && currentAmmo != weapons[currentWeapon].clipSize)
            StartCoroutine(Reload());
        WeaponSway();
    }

    public IEnumerator Reload()
    {
        activeFire = true;
        info.animator.SetTrigger("Reload");
        yield return new WaitForSeconds(weapons[currentWeapon].reloadSpeed);
        activeFire = false;
        currentAmmo = weapons[currentWeapon].clipSize;
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
                if (Physics.Raycast(Camera.main.transform.position, getSpreadDirection(Camera.main.transform.forward,weapons[currentWeapon].spread), out hit, weapons[currentWeapon].raycastFireLength))
                    if(hit.transform.tag == enemyTag && !weapons[currentWeapon].explodingBullets)
                    {
                        GameObject currentObject = hit.transform.gameObject;
                        while (!currentObject.GetComponent<RemovableLimbs>())
                            currentObject = currentObject.transform.parent.gameObject;
                        currentObject.GetComponent<RemovableLimbs>().DoDamage(hit.collider, weapons[currentWeapon].damage, hit.point - Camera.main.transform.forward, weapons[currentWeapon].force, PhotonNetwork.playerName);
                    }
                    else if(weapons[currentWeapon].explodingBullets)
                    {
                        Collider[] enemyParts = Physics.OverlapSphere(hit.point, weapons[currentWeapon].explosionRadius, enemyMask);
                        GameObject.FindWithTag("Manager").GetPhotonView().RPC("CallScreenShake", PhotonTargets.All, weapons[currentWeapon].explosionScreenShakeTime, weapons[currentWeapon].explosionScreenShakeIntensity, hit.point);
                        foreach(Collider col in enemyParts)
                        {
                            GameObject currentObject = col.gameObject;
                            while (!currentObject.GetComponent<RemovableLimbs>())
                                currentObject = currentObject.transform.parent.gameObject;
                            Vector3 explosionPoint = hit.point + (Vector3.down / 3) - (Camera.main.transform.forward / 4);
                            currentObject.GetComponent<RemovableLimbs>().DoDamage(col, weapons[currentWeapon].explosionDamage, explosionPoint, weapons[currentWeapon].force, PhotonNetwork.playerName);
                        }
                        GameObject.FindWithTag("Manager").GetPhotonView().RPC("SpawnParticle", PhotonTargets.All, weapons[currentWeapon].explosionParticleIndex, hit.point, Quaternion.identity, null);
                    }
                break;
        }
        currentAmmo--;
        photonView.RPC("Recoil", PhotonTargets.All, weapons[currentWeapon].backwardsRecoil, weapons[currentWeapon].horizontalRotationRecoil);
    }

    public static Vector3 getSpreadDirection(Vector3 dir, float spread)
    {
        Vector3 direction = Camera.main.transform.forward;
        float x = spread * Random.Range(1f, -1f);
        float y = spread * Random.Range(1f, -1f);
        float z = spread * Random.Range(1f, -1f);

        direction = Quaternion.Euler(x, y, z) * direction;
        return direction;
    }

    [PunRPC]
    public void Recoil(float backwardsRecoil,float horizontalRecoil)
    {
        float verticalRecoil = weapons[currentWeapon].cameraRecoil * Random.Range(-0.5f, -1f);
        float horizontalCamRecoil = weapons[currentWeapon].camereHorizontalRecoil * Random.Range(-1f, 1f);

        cam.parent.Rotate(Vector3.right * verticalRecoil);
        cam.Rotate(-Vector3.right * verticalRecoil * 0.35f);

        transform.Rotate(Vector3.up * horizontalCamRecoil);
        cam.Rotate(-Vector3.up * horizontalCamRecoil * 0.35f, Space.World);


        weapon.Translate(-Vector3.forward * backwardsRecoil);
        weapon.Rotate(-Vector3.right * Random.Range(horizontalRecoil / 2f, horizontalRecoil));
    }
}
