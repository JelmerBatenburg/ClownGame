using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Photon.MonoBehaviour
{
    public float explosionDamage;
    public float explosionRadius;
    public int explosionParticleIndex;
    public float explosionForce;
    public LayerMask enemyMask;
    public string managerString;
    public Transform visual;
    public Vector3 rotation;
    public float rotationSpeed;
    public float collisionCheckSize;

    [PunRPC,HideInInspector]
    public void SetInformation(float time, Vector3 direction, float power, float damage, float radius, int particleIndex, float force)
    {
        rotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        explosionForce = force;
        explosionDamage = damage;
        explosionRadius = radius;
        explosionParticleIndex = particleIndex;
        GetComponent<Rigidbody>().AddForce(direction * power);
        if (photonView.isMine)
            StartCoroutine(Explode(time));
    }

    public void Update()
    {
        visual.transform.Rotate(rotation * Time.deltaTime * rotationSpeed);
        if (Physics.CheckSphere(transform.position, collisionCheckSize, enemyMask) && photonView.isMine)
            StartCoroutine(Explode(0));
    }

    public IEnumerator Explode(float time)
    {
        yield return new WaitForSeconds(time);
        Collider[] enemyParts = Physics.OverlapSphere(transform.position, explosionRadius, enemyMask);
        GameObject.FindWithTag(managerString).GetPhotonView().RPC("SpawnParticle", PhotonTargets.All, explosionParticleIndex, transform.position, Quaternion.identity, null);
        foreach (Collider col in enemyParts)
        {
            GameObject currentObject = col.gameObject;
            while (!currentObject.GetComponent<RemovableLimbs>())
                currentObject = currentObject.transform.parent.gameObject;
            currentObject.GetComponent<RemovableLimbs>().DoDamage(col, explosionDamage, transform.position, explosionForce, PhotonNetwork.playerName);
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
