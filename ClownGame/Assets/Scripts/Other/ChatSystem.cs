using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem : Photon.MonoBehaviour
{
    public InputField inputField;
    public GameObject chatMessage;
    public Transform chatLayout;
    public float chatMessageLifetime;

    public void SendMessage()
    {
        photonView.RPC("MessageRecieve", PhotonTargets.All, inputField.text);
        inputField.text = "";
    }

    [PunRPC,HideInInspector]
    public void MessageRecieve(string message)
    {
        GameObject g = Instantiate(chatMessage, chatLayout);
        g.GetComponent<Text>().text = message;
        Destroy(g, chatMessageLifetime);
    }
}