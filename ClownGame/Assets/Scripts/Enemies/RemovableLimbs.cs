using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovableLimbs : Photon.MonoBehaviour
{
    public RemovableLimbInformation[] removableLimbs;
    public List<Rigidbody> ragdollBones;
    public float health;
    public float ragdollpartRemovalForce;
    public Animator animator;
    public AudioSource source;
    public float bodyLifeTime;
    public bool died;

    public void Start()
    {
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

    public void DoDamage(Collider col, float damage, Vector3 damagePoint)
    {
        bool normalDamage = true;
        for (int i = 0; i < removableLimbs.Length; i++)
            if(col == removableLimbs[i].col)
            {
                normalDamage = false;
                health -= damage * removableLimbs[i].damageMultiplier;
                if (health <= 0)
                    photonView.RPC("RemoveLimb", PhotonTargets.All, i);
                break;
            }

        if (normalDamage)
            health -= damage;

        photonView.RPC("ChangeHealth", PhotonTargets.All, health, damagePoint);
    }

    [PunRPC,HideInInspector]
    public void ChangeHealth(float currentHealth, Vector3 damagePoint)
    {
        health = currentHealth;

        if (health <= 0)
        {
            ToggleRagdoll(true);
            foreach (Rigidbody rig in ragdollBones)
                rig.AddExplosionForce(ragdollpartRemovalForce, damagePoint, Mathf.Infinity);
            if (photonView.isMine)
                Destroy(gameObject, bodyLifeTime);
        }
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
