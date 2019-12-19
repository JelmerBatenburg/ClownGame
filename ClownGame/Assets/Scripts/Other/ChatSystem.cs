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
    public bool openWithEnter;
    private bool enterDelay;
    public Manager ingameManager;

    public void Update()
    {
        if (openWithEnter && !enterDelay && !inputField.isFocused && Input.GetButtonDown("Submit"))
        {
            inputField.ActivateInputField();
            if (ingameManager && ingameManager.currentPlayer)
                ingameManager.currentPlayer.GetComponent<CharacterMovement>().allowMovment = false;
        }
    }

    public void SendMessage()
    {
        if (Input.GetButtonDown("Submit") && inputField.text != "")
        {
            photonView.RPC("MessageRecieve", PhotonTargets.All, PhotonNetwork.playerName + ": " + inputField.text);
            inputField.text = "";
            if (continueTyping)
                inputField.ActivateInputField();
            else if (openWithEnter)
                StartCoroutine(EnterDelay());
        }
    }

    public IEnumerator EnterDelay()
    {
        enterDelay = true;
        yield return null;
        enterDelay = false;
        if (ingameManager && ingameManager.currentPlayer)
            ingameManager.currentPlayer.GetComponent<CharacterMovement>().allowMovment = true;
    }

    [PunRPC,HideInInspector]
    public void MessageRecieve(string message)
    {
        GameObject g = Instantiate(chatMessage, chatLayout);
        g.GetComponent<Text>().text = message;
        Destroy(g, chatMessageLifetime);
    }
}