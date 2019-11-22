using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : Photon.MonoBehaviour
{
    public CharacterMovement character;
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

    public void Start()
    {
        DisplayWeapon();
    }

    public void DisplayWeapon()
    {
        foreach (Transform child in weapon)
            Destroy(child.gameObject);
        GameObject weaponObject = Instantiate(character.currentClass.weapons[currentWeapon].weaponObject, weapon.transform.position, weapon.transform.rotation, weapon);
        info = weaponObject.GetComponent<WeaponObjectInformation>();
    }

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
        if (!activeFire && Input.GetButtonDown(reloadInput) && photonView.isMine && currentAmmo != character.currentClass.weapons[currentWeapon].clipSize)
            StartCoroutine(Reload());
        WeaponSway();
    }

    public IEnumerator Reload()
    {
        activeFire = true;
        info.animator.SetTrigger("Reload");
        yield return new WaitForSeconds(character.currentClass.weapons[currentWeapon].reloadSpeed);
        activeFire = false;
        currentAmmo = character.currentClass.weapons[currentWeapon].clipSize;
    }

    public IEnumerator Shooting()
    {
        activeFire = true;
        WeaponStatsScriptableObject stats = character.currentClass.weapons[currentWeapon];
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
        if (Input.GetButton("Fire1") && stats.fireType == WeaponStatsScriptableObject.FireType.auto && currentAmmo > 0) 
            StartCoroutine(Shooting());
        else
            activeFire = false;
    }

    public void Shoot()
    {
        WeaponStatsScriptableObject stats = character.currentClass.weapons[currentWeapon];
        switch (stats.projectileType)
        {
            case WeaponStatsScriptableObject.ProjectileType.rayCast:
                RaycastHit hit = new RaycastHit();
                source.PlayOneShot(stats.clip);
                int bulletAmount = stats.multipleShots ? stats.shotAmount : 1;
                for (int i = 0; i < bulletAmount; i++)
                {
                    if (Physics.Raycast(Camera.main.transform.position, getSpreadDirection(Camera.main.transform.forward, stats.spread), out hit, stats.raycastFireLength))
                        if (hit.transform.tag == enemyTag && !stats.explodingBullets)
                        {
                            GameObject currentObject = hit.transform.gameObject;
                            while (!currentObject.GetComponent<RemovableLimbs>())
                                currentObject = currentObject.transform.parent.gameObject;
                            currentObject.GetComponent<RemovableLimbs>().DoDamage(hit.collider, stats.damage, hit.point - Camera.main.transform.forward, stats.force, PhotonNetwork.playerName);
                        }
                        else if (stats.explodingBullets)
                        {
                            Collider[] enemyParts = Physics.OverlapSphere(hit.point, stats.explosionRadius, enemyMask);
                            GameObject.FindWithTag("Manager").GetPhotonView().RPC("CallScreenShake", PhotonTargets.All, stats.explosionScreenShakeTime, stats.explosionScreenShakeIntensity, hit.point);
                            foreach (Collider col in enemyParts)
                            {
                                GameObject currentObject = col.gameObject;
                                while (!currentObject.GetComponent<RemovableLimbs>())
                                    currentObject = currentObject.transform.parent.gameObject;
                                Vector3 explosionPoint = hit.point + (Vector3.down / 3) - (Camera.main.transform.forward / 4);
                                currentObject.GetComponent<RemovableLimbs>().DoDamage(col, stats.explosionDamage, explosionPoint, stats.force, PhotonNetwork.playerName);
                            }
                            GameObject.FindWithTag("Manager").GetPhotonView().RPC("SpawnParticle", PhotonTargets.All, stats.explosionParticleIndex, hit.point, Quaternion.identity, null);
                        }
                }
                break;
        }
        currentAmmo--;
        photonView.RPC("Recoil", PhotonTargets.All, stats.backwardsRecoil, stats.horizontalRotationRecoil);
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
        float verticalRecoil = character.currentClass.weapons[currentWeapon].cameraRecoil * Random.Range(-0.5f, -1f);
        float horizontalCamRecoil = character.currentClass.weapons[currentWeapon].camereHorizontalRecoil * Random.Range(-1f, 1f);

        cam.parent.Rotate(Vector3.right * verticalRecoil);
        cam.Rotate(-Vector3.right * verticalRecoil * 0.35f);

        transform.Rotate(Vector3.up * horizontalCamRecoil);
        cam.Rotate(-Vector3.up * horizontalCamRecoil * 0.35f, Space.World);


        weapon.Translate(-Vector3.forward * backwardsRecoil);
        weapon.Rotate(-Vector3.right * Random.Range(horizontalRecoil / 2f, horizontalRecoil));
    }
}
