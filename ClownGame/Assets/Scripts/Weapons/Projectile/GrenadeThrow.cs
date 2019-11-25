using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrow : Photon.MonoBehaviour
{
    public string grenadeName;
    public Transform instantiationPoint;
    public float timer;
    public float throwStrenght;
    public float explosionRange;
    public int explosionParticle;
    public float force;
    public float damage;
    public string grenadeInput;
    public Vector2Int grenadeThrowSounds;
    public int throwSounds;
    public bool delay;
    public float throwDelay;

    void Update()
    {
        if (photonView.isMine && Input.GetButtonDown(grenadeInput) && !delay)
            ThrowGrenade();
    }

    public void ThrowGrenade()
    {
        delay = true;
        photonView.RPC("PlaySound", PhotonTargets.All, false, grenadeThrowSounds.x, grenadeThrowSounds.y);
        photonView.RPC("PlaySound", PhotonTargets.All, true, throwSounds, 0);
        GameObject g = PhotonNetwork.Instantiate(grenadeName, instantiationPoint.position, instantiationPoint.rotation, 0);
        g.GetPhotonView().RPC("SetInformation", PhotonTargets.All, timer,Camera.main.transform.forward, throwStrenght, damage, explosionRange, explosionParticle, force);
        StartCoroutine(Delay());
    }

    public IEnumerator Delay()
    {
        yield return new WaitForSeconds(throwDelay);
        delay = false;
    }
}
