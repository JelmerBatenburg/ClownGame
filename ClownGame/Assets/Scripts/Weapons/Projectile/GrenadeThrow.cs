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

    void Update()
    {
        if (photonView.isMine && Input.GetButtonDown(grenadeInput))
            ThrowGrenade();
    }

    public void ThrowGrenade()
    {
        photonView.RPC("PlaySound", PhotonTargets.All, grenadeThrowSounds.x, grenadeThrowSounds.y);
        GameObject g = PhotonNetwork.Instantiate(grenadeName, instantiationPoint.position, instantiationPoint.rotation, 0);
        g.GetPhotonView().RPC("SetInformation", PhotonTargets.All, timer,Camera.main.transform.forward, throwStrenght, damage, explosionRange, explosionParticle, force);
    }
}
