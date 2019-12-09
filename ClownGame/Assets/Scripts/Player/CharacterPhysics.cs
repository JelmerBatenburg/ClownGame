using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics : MonoBehaviour
{
    public Vector3 force;
    [Header("Detection Settings")]
    public float forceCheckLength;
    public float checkWidth;
    public float raycastDownLenght;
    public float heightOffset;
    public float groundCheckOffset;
    public LayerMask detectionMask;

    [Header("Gravity Settings")]
    public float gravity;
    public float airDrag, groundDrag;
    public bool onGround;

    public void Update()
    {
        ApplyForces();
        CheckIfOnGround();
    }

    public void ApplyForces()
    {
        if (!onGround)
            force += Vector3.down * Time.deltaTime * gravity;

        force = Vector3.Lerp(force, Vector3.zero, (onGround ? groundDrag : airDrag) * Time.deltaTime);
        transform.Translate(force * Time.deltaTime, Space.World);
    }

    public void CheckIfOnGround()
    {
        RaycastHit hit = new RaycastHit();
        float checkHeight = 0;
        if (Physics.SphereCast(transform.position + (Vector3.up * heightOffset), checkWidth, Vector3.down, out hit, raycastDownLenght, detectionMask))
        {
            onGround = true;
            checkHeight = hit.point.y;
            force.z = 0;
        }
        else
            onGround = false;

        if (onGround && Physics.SphereCast(transform.position + (Vector3.up * heightOffset) + new Vector3(force.x, 0, force.z), checkWidth, Vector3.down, out hit, raycastDownLenght, detectionMask))
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(checkHeight, hit.point.y, 0.5f), transform.position.z);
    }
}
