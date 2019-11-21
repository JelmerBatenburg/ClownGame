using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : Photon.MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform currentTarget;
    public LayerMask playerMask;
    public string playerTag;
    public float targetZone;
    public float attackZone;
    public float health;

    public float minimalTargetingDamage;
    public float targetingDamageDropOff;
    public List<TargetingDamageInfo> damageInfo = new List<TargetingDamageInfo>();

    Vector3 position = new Vector3();
    Quaternion rotation = new Quaternion();
    public float lerpSpeed = 9;

    public void Start()
    {
        FindNearestTarget();
        if (!photonView.isMine)
        {
            agent.enabled = false;
            StartCoroutine(LerpPosition());
        }
    }

    [PunRPC,HideInInspector]
    public void DamagedAggroSafe(float damage, string damager)
    {
        if (photonView.isMine)
        {
            bool found = false;
            for (int i = 0; i < damageInfo.Count; i++)
                if (damager == damageInfo[i].playerName)
                {
                    found = true;
                    damageInfo[i].damage += damage;
                    if(damageInfo[i].damage >= minimalTargetingDamage && !Physics.CheckSphere(transform.position, targetZone))
                    {
                        FindTargetPlayer(damageInfo[i].playerName);
                        damageInfo[i].damage = 0;
                    }
                    break;
                }
            if (!found)
                damageInfo.Add(new TargetingDamageInfo(damager, damage));
        }
    }

    public void FindTargetPlayer(string name)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject player in players)
            if (player.GetPhotonView().owner.NickName == name)
                currentTarget = player.transform;
    }

    public void DropDamage()
    {
        foreach (TargetingDamageInfo info in damageInfo)
            if (info.damage > 0)
                info.damage -= targetingDamageDropOff * Time.deltaTime;
    }

    public IEnumerator LerpPosition()
    {
        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * lerpSpeed);
            yield return null;
        }
    }

    public void Update()
    {
        if (photonView.isMine)
        {
            DropDamage();
            TargetPlayer();
        }
    }

    public void FindNearestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        currentTarget = players[0].transform;
        for (int i = 1; i < players.Length; i++)
            if (Vector3.Distance(transform.position, players[i].transform.position) <= Vector3.Distance(transform.position, currentTarget.position))
                currentTarget = players[i].transform;
    }

    public void DisableNavMesh()
    {
        agent.enabled = false;
        this.enabled = false;
    }

    public void TargetPlayer()
    {
        List<Collider> targetZonedPlayers = new List<Collider>(Physics.OverlapSphere(transform.position,targetZone,playerMask));
        List<Collider> damageZonedPlayers = new List<Collider>(Physics.OverlapSphere(transform.position, attackZone, playerMask));
        if (targetZonedPlayers.Count > 0 && !targetZonedPlayers.Contains(currentTarget.GetComponent<Collider>()))
            currentTarget = targetZonedPlayers[0].transform;
        
        agent.SetDestination(currentTarget.transform.position);
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetZone);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackZone);
    }

    [System.Serializable]
    public class TargetingDamageInfo
    {
        public string playerName;
        public float damage;
        public TargetingDamageInfo(string _playerName,float _damage)
        {
            playerName = _playerName;
            damage = _damage;
        }
    }
}
