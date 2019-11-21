using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovableLimbs : Photon.MonoBehaviour
{
    public RemovableLimbInformation[] removableLimbs;
    public List<Rigidbody> ragdollBones;
    public float ragdollpartRemovalForce;
    public Animator animator;
    public AudioSource source;
    public float bodyLifeTime;
    public bool died;
    public BaseEnemy enemyBase;

    public void Start()
    {
        enemyBase = GetComponent<BaseEnemy>();
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool toggle)
    {
        foreach (Rigidbody rig in ragdollBones)
            rig.isKinematic = !toggle;
        animator.enabled = !toggle;
    }

    [PunRPC,HideInInspector]
    public void RemoveLimb(int index)
    {
        RemovableLimbInformation info = removableLimbs[index];
        info.col.enabled = false;
        info.obj.SetActive(false);
        if (info.clip)
            source.PlayOneShot(info.clip);
        foreach (GoreInstantiatables gore in info.instantiatables)
        {
            GameObject g = Instantiate(gore.obj, gore.parentedObject.position, gore.parentedObject.rotation);
            Destroy(g, gore.lifeTime);
            if (gore.parented)
                g.transform.parent = gore.parentedObject;
        }
    }

    public void DoDamage(Collider col, float damage, Vector3 damagePoint, float force, string damager)
    {
        bool normalDamage = true;
        for (int i = 0; i < removableLimbs.Length; i++)
            if(col == removableLimbs[i].col)
            {
                normalDamage = false;
                damage = damage * removableLimbs[i].damageMultiplier;
                enemyBase.health -= damage;
                if (enemyBase.health <= 0)
                    photonView.RPC("RemoveLimb", PhotonTargets.All, i);
                break;
            }

        if (normalDamage)
            enemyBase.health -= damage;
        photonView.RPC("DamagedAggroSafe", PhotonTargets.All, damage, damager);
        photonView.RPC("ChangeHealth", PhotonTargets.All, enemyBase.health, damagePoint, force);
    }

    [PunRPC,HideInInspector]
    public void ChangeHealth(float currentHealth, Vector3 damagePoint, float force)
    {
        enemyBase.health = currentHealth;

        if (enemyBase.health <= 0)
        {
            ToggleRagdoll(true);
            foreach (Rigidbody rig in ragdollBones)
                rig.AddExplosionForce(force, damagePoint, Mathf.Infinity);
            if (photonView.isMine)
                StartCoroutine(DelayedPhotonDestroy(bodyLifeTime));
            GetComponent<BaseEnemy>().DisableNavMesh();
        }
    }

    public IEnumerator DelayedPhotonDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        PhotonNetwork.Destroy(gameObject);
    }

    [System.Serializable]
    public class RemovableLimbInformation
    {
        public Collider col;
        public GameObject obj;
        public float damageMultiplier = 1;
        public GoreInstantiatables[] instantiatables;
        public AudioClip clip;
    }

    [System.Serializable]
    public class GoreInstantiatables
    {
        public GameObject obj;
        public float lifeTime;
        public bool parented;
        public Transform parentedObject;
    }
}
