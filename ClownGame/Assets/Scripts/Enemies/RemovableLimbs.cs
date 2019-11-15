using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovableLimbs : MonoBehaviour
{
    public RemovableLimbInformation[] removableLimbs;
    public List<Rigidbody> ragdollBones;
    public float health;

    public void Start()
    {
        ToggleRagdoll(false);
    }

    public void ToggleRagdoll(bool toggle)
    {
        foreach (Rigidbody rig in ragdollBones)
            rig.isKinematic = !toggle;
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
                    info.obj.SetActive(false);
                break;
            }

        if (normalDamage)
            health -= damage;

        if (health <= 0)
            ToggleRagdoll(true);
    }

    [System.Serializable]
    public class RemovableLimbInformation
    {
        public Collider col;
        public GameObject obj;
        public float damageMultiplier = 1;
    }
}
