using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : Photon.MonoBehaviour
{
    [Header("Movement")]
    public ClassScriptableObject currentClass;
    Vector3 movementDirection;
    public float health;
    [Header("GroundDetection")]
    public string jumpInput;
    public float downwardsRaycastRange;
    public LayerMask groundMask;
    public CharacterPhysics physics;
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

    [PunRPC,HideInInspector]
    public void DoDamage(float damage)
    {
        if (photonView.isMine)
        {
            health -= damage;
            if (health <= damage)
            {
                PhotonNetwork.Destroy(gameObject);
                Manager manager = GameObject.FindWithTag("Manager").GetComponent<Manager>();
                manager.StartCoroutine(manager.SpawnPlayer());
            }
        }
    }

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
        if (physics.onGround)
        {
            movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            movementDirection = transform.TransformDirection(movementDirection);
            physics.force += movementDirection * Time.deltaTime * currentClass.movementSpeed;
            if (Input.GetButtonDown(jumpInput))
                physics.force += Vector3.up * currentClass.jumpHeight;
        }
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
