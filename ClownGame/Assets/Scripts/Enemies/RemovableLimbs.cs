using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovableLimbs : MonoBehaviour
{
    public RemovableLimbInformation[] removableLimbs;
    public List<Rigidbody> ragdollBones;
    public float health;
    public float ragdollpartRemovalForce;
    public Animator animator;
    public AudioSource source;
    public float bodyLifeTime;

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

    public IEnumerator DelayedForce(Vector3 point)
    {
        yield return null;
        foreach (Rigidbody rig in ragdollBones)
            rig.AddExplosionForce(ragdollpartRemovalForce, point, Mathf.Infinity);
    }

    public void DoDamage(Collider col, float damage)
    {
        bool normalDamage = true;
        foreach(RemovableLimbInformation info in removableLimbs)
            if(col == info.col)
            {
                normalDamage = false;
                health -= damage *= info.damageMultiplier;
                if (health <= 0)
                {
                    info.col.enabled = false;
                    info.obj.SetActive(false);
                    if (info.clip)
                        source.PlayOneShot(info.clip);
                    foreach(GoreInstantiatables gore in info.instantiatables)
                    {
                        GameObject g = Instantiate(gore.obj, gore.parentedObject.position, gore.parentedObject.rotation);
                        Destroy(g, gore.lifeTime);
                        if (gore.parented)
                            g.transform.parent = gore.parentedObject;
                    }
                }
                break;
            }

        if (normalDamage)
            health -= damage;

        if (health <= 0)
        {
            ToggleRagdoll(true);
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
