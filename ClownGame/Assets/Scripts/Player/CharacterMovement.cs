using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : Photon.MonoBehaviour
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
    [Header("Networking")]
    public GameObject[] enableObjects;
    public GameObject[] disabledObjects;
    public float lerpSpeed;
    Vector3 position;
    Quaternion rotation;
    Quaternion camRotation;

    public void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (!photonView.isMine)
        {
            foreach (GameObject obj in enableObjects)
                obj.SetActive(true);
            foreach (GameObject obj in disabledObjects)
                obj.SetActive(false);
            StartCoroutine(LerpPosition());
        }
    }

    public IEnumerator LerpPosition()
    {
        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * lerpSpeed);
            cam.rotation = Quaternion.Lerp(cam.rotation, camRotation, Time.deltaTime * lerpSpeed);
            yield return null;
        }
    }

    public void Update()
    {
        if (photonView.isMine)
        {
            Move();
            Rotate();
        }
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

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(cam.rotation);
        }
        else
        {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
            camRotation = (Quaternion)stream.ReceiveNext();
        }

    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, Vector3.down);
    }
}
