using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : Photon.MonoBehaviour
{
    public CharacterMovement character;
    public int[] weaponAmmo;
    public int currentWeapon;
    public bool disableInteraction;
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
        if (photonView.isMine)
            photonView.RPC("DisplayWeapon", PhotonTargets.All, currentWeapon);
        weaponAmmo = new int[character.currentClass.weapons.Length];
        for (int i = 0; i < weaponAmmo.Length; i++)
            weaponAmmo[i] = character.currentClass.weapons[i].clipSize;
    }

    [PunRPC, HideInInspector]
    public void DisplayWeapon(int index)
    {
        foreach (Transform child in weapon)
            Destroy(child.gameObject);
        GameObject weaponObject = Instantiate(character.currentClass.weapons[index].weaponObject, weapon.transform.position, weapon.transform.rotation, weapon);
        info = weaponObject.GetComponent<WeaponObjectInformation>();
    }

    public void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
    {
        if (photonView.isMine)
            photonView.RPC("DisplayWeapon", PhotonTargets.All, currentWeapon);
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
        if (photonView.isMine)
        {
            if (!disableInteraction && Input.GetButtonDown("Fire1") && weaponAmmo[currentWeapon] > 0)
                StartCoroutine(Shooting());
            if (!disableInteraction && Input.GetButtonDown(reloadInput) && weaponAmmo[currentWeapon] != character.currentClass.weapons[currentWeapon].clipSize)
                StartCoroutine(Reload());
            if (Input.GetAxis("Mouse ScrollWheel") != 0 && !disableInteraction)
                Swap(-1, Input.GetAxis("Mouse ScrollWheel"));
        }
        WeaponSway();
    }

    public void Swap(int index = -1, float scrollValue = 0)
    {
        if (index == -1)
        {
            if (scrollValue > 0)
                if (currentWeapon == character.currentClass.weapons.Length - 1)
                    currentWeapon = 0;
                else
                    currentWeapon++;
            else if (currentWeapon == 0)
                currentWeapon = character.currentClass.weapons.Length - 1;
            else
                currentWeapon--;
            photonView.RPC("DisplayWeapon", PhotonTargets.All, currentWeapon);
        }
        else
        {
            currentWeapon = index;
            photonView.RPC("DisplayWeapon", PhotonTargets.All, currentWeapon);
        }
    }

    public IEnumerator Reload()
    {
        disableInteraction = true;
        photonView.RPC("StartAnimation", PhotonTargets.All, "Reload");
        yield return new WaitForSeconds(character.currentClass.weapons[currentWeapon].reloadSpeed);
        disableInteraction = false;
        weaponAmmo[currentWeapon] = character.currentClass.weapons[currentWeapon].clipSize;
    }

    [PunRPC,HideInInspector]
    public void StartAnimation(string animation)
    {
        info.animator.SetTrigger(animation);
    }

    public IEnumerator Shooting()
    {
        disableInteraction = true;
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
        if (Input.GetButton("Fire1") && stats.fireType == WeaponStatsScriptableObject.FireType.auto && weaponAmmo[currentWeapon] > 0) 
            StartCoroutine(Shooting());
        else
            disableInteraction = false;
    }

    public void Shoot()
    {
        WeaponStatsScriptableObject stats = character.currentClass.weapons[currentWeapon];
        source.PlayOneShot(stats.clip);
        int bulletAmount = stats.multipleShots ? stats.shotAmount : 1;
        switch (stats.projectileType)
        {
            case WeaponStatsScriptableObject.ProjectileType.rayCast:
                RaycastHit hit = new RaycastHit();
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
            case WeaponStatsScriptableObject.ProjectileType.projectile:
                source.PlayOneShot(stats.clip);
                for (int i = 0; i < bulletAmount; i++)
                {
                    GameObject g = PhotonNetwork.Instantiate(stats.projectileName, info.firePoint.position, info.firePoint.rotation, 0);
                    g.GetPhotonView().RPC("SetInformation", PhotonTargets.All, stats.explosionTimer, getSpreadDirection(Camera.main.transform.forward, stats.spread), stats.fireStrenght, stats.explosionDamage, stats.explosionRadius, stats.explosionParticleIndex, stats.force);
                }
                break;
        }
        weaponAmmo[currentWeapon]--;
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
