using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed;
    Vector3 movementDirection;
    [Header("GroundDetection")]
    public float jumpHeight;
    public string jumpInput;
    public float downwardsRaycastRange;
    public LayerMask groundMask;
    public Rigidbody rig;
    [Header("Rotation")]
    public Transform cam;
    public float rotationSpeed;
    [Header("WeaponSway")]
    public float weaponSwayStrenght;
    public Transform weapon;
    public float weaponLerpSpeed;
    public float weaponJumpWeight;

    public void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        Move();
        Rotate();
        WeaponSway();
    }

    public void WeaponSway()
    {
        weapon.Translate(new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y") - (rig.velocity.y * weaponJumpWeight)) * weaponSwayStrenght * Time.deltaTime);
        weapon.position = Vector3.Lerp(weapon.position, weapon.parent.position, Time.deltaTime * weaponLerpSpeed);
        weapon.rotation = Quaternion.Lerp(weapon.rotation, weapon.parent.rotation, Time.deltaTime * weaponLerpSpeed);
    }

    public void Rotate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * Input.GetAxis("Mouse X"));
        cam.Rotate(Vector3.right * rotationSpeed * Time.deltaTime * -Input.GetAxis("Mouse Y"));
    }

    public void Move()
    {
        if (Physics.Raycast(transform.position, Vector3.down, downwardsRaycastRange, groundMask))
        {
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (Input.GetButtonDown(jumpInput))
                rig.velocity += Vector3.up * jumpHeight;
        }
        transform.Translate(movementDirection * Time.deltaTime * movementSpeed);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down);
    }
}
