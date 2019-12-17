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
    public bool continueTyping;

    public void SendMessage()
    {
        if (Input.GetButtonDown("Submit") && inputField.text != "")
        {
            photonView.RPC("MessageRecieve", PhotonTargets.All, PhotonNetwork.playerName + ": " + inputField.text);
            inputField.text = "";
            if (continueTyping)
                inputField.ActivateInputField();
        }
    }

    [PunRPC,HideInInspector]
    public void MessageRecieve(string message)
    {
        GameObject g = Instantiate(chatMessage, chatLayout);
        g.GetComponent<Text>().text = message;
        Destroy(g, chatMessageLifetime);
    }
}