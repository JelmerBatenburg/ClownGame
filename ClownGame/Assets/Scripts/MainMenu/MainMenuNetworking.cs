using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuNetworking : Photon.MonoBehaviour
{
    public string connectionVersion;
    public RoomInfo[] rooms;
    [Header("Connecting")]
    public GameObject failedLoadingUI;
    public GameObject connectingOptions;
    [Header("CreateRoom")]
    public InputField nameInput;
    public Slider slider;

    public void OnReceivedRoomListUpdate()
    {
        rooms = PhotonNetwork.GetRoomList();
    }

    public void ConnectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings(connectionVersion);
    }

    public void OnConnectedToPhoton()
    {
        connectingOptions.SetActive(true);
    }

    public void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        failedLoadingUI.SetActive(true);
    }

    public void DisconnectFromPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    public void CreateRoom()
    {
        bool createRoom = true;
        foreach (RoomInfo room in rooms)
            if (room.Name == nameInput.text)
            {
                createRoom = false;
                break;
            }

        if (createRoom)
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)Mathf.RoundToInt(slider.value);
            PhotonNetwork.CreateRoom(nameInput.text, options, TypedLobby.Default);
        }
        else
            Debug.Log("Roomname is taken");
    }
}
