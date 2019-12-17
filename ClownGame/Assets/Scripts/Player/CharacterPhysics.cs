using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics : Photon.MonoBehaviour
{
    public Vector3 force;
    public LayerMask detectionMask;

    [Header("Floor Settings")]
    public float forceCheckLength;
    public float checkWidth;
    public float raycastDownLenght;
    public float heightOffset;
    public float groundCheckOffset;
    public float offsetExtraLenght;
    public float maxStepHeight;

    [Header("WallDetectSetting")]
    public float detectRadius;
    public float centerOffset;

    [Header("Gravity Settings")]
    public float gravity;
    public float airDrag, groundDrag;
    public bool onGround;
    public float fallMultiplier;

    public void Update()
    {
        if (photonView.isMine)
        {
            CheckIfOnGround();
            ApplyForces();
        }
    }

    public void CheckWalls()
    {
        Vector3 checkpos = transform.position + (Vector3.up * centerOffset);
        Collider[] nearbyCollisions = Physics.OverlapSphere(checkpos, detectRadius, detectionMask);
        RaycastHit hit = new RaycastHit();
        foreach(Collider col in nearbyCollisions)
        {
            Vector3 dir = (col.ClosestPoint(checkpos) - checkpos).normalized;
            Debug.DrawRay(checkpos, dir, Color.red);
            if (Physics.Raycast(checkpos, dir, out hit, detectionMask) && hit.transform.gameObject == col.gameObject && Vector3.Dot(force.normalized, dir) > 0)
                force -= dir * (Vector3.Distance(force, Vector3.zero) * Vector3.Dot(force.normalized, dir));
        }
    }

    public void ApplyForces()
    {
        if (!onGround)
            force += Vector3.down * Time.deltaTime * gravity * ((force.y <= 0)? fallMultiplier : 1);

        force = Vector3.Lerp(force, Vector3.zero, (onGround ? groundDrag : airDrag) * Time.deltaTime);
        if (force != Vector3.zero)
            CheckWalls();
        transform.Translate(force * Time.deltaTime, Space.World);
    }

    public void CheckIfOnGround()
    {
        RaycastHit hit = new RaycastHit();
        float checkHeight = 0;
        if (force.y <= 1f)
        {
            if (Physics.SphereCast(transform.position + (Vector3.up * heightOffset), checkWidth, Vector3.down, out hit, raycastDownLenght, detectionMask))
            {
                onGround = true;
                checkHeight = hit.point.y;
                force.y = 0;
            }
            else
                onGround = false;

            if (onGround && Physics.SphereCast(transform.position + (Vector3.up * heightOffset * 2) + (new Vector3(force.x, 0, force.z) * groundCheckOffset), checkWidth, Vector3.down, out hit, raycastDownLenght + offsetExtraLenght * 2, detectionMask))
            {
                if (Mathf.Abs(checkHeight - hit.point.y) <= maxStepHeight)
                {
                    Vector3 placeSpot = transform.position;
                    placeSpot.y = (checkHeight != 0) ? Mathf.Lerp(checkHeight, hit.point.y, 0.5f) : hit.point.y;
                    transform.position = placeSpot;
                }
            }
        }
        else
            onGround = false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position + (Vector3.up * heightOffset), Vector3.down * raycastDownLenght);
        Gizmos.DrawRay(transform.position + (Vector3.up * heightOffset * 2) + (new Vector3(force.x, 0, force.z) * groundCheckOffset), Vector3.down * (raycastDownLenght + offsetExtraLenght * 2));
        Gizmos.DrawWireSphere(transform.position, checkWidth);
        Gizmos.DrawWireSphere(transform.position + (Vector3.up * centerOffset), detectRadius);
    }
}
