using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuNetworking : Photon.MonoBehaviour
{
    public string connectionVersion;
    public string playerName;
    public RoomInfo[] rooms;
    [Header("Connecting")]
    public GameObject failedLoadingUI;
    public GameObject connectingOptions;
    [Header("CreateRoom")]
    public InputField nameInput;
    public Slider slider;
    [Header("CurrentRoomInformation")]
    public Transform playerPanelLayout;
    public GameObject playerPanel;
    public List<PlayerReadyInfo> playersInformation = new List<PlayerReadyInfo>();
    [Header("Rooms")]
    public Transform roomPanelLayout;
    public GameObject roomPanel;
    public GameObject roomsUIObject, roomUI;

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        StartCoroutine(DisplayPlayers());
    }

    public void LoadRoom(int roomIndex)
    {
        roomsUIObject.SetActive(false);
        roomUI.SetActive(true);
        PhotonNetwork.JoinRoom(rooms[roomIndex].Name);
    }

    public void DisplayAvailableRooms()
    {
        foreach (Transform child in roomPanelLayout)
            Destroy(child);

        for (int i = 0; i < rooms.Length; i++)
            if(rooms[i].MaxPlayers != rooms[i].PlayerCount)
            {
                GameObject g = Instantiate(roomPanel, roomPanelLayout);
                g.GetComponent<RoomInfoPanel>().SetInformation(rooms[i].Name, rooms[i].PlayerCount, rooms[i].MaxPlayers, i, this);
            }
    }

    public IEnumerator DisplayPlayers()
    {
        yield return null;
        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            bool found = false;
            for (int i = 0; i < playersInformation.Count; i++)
                if (playersInformation[i].player.NickName == player.NickName)
                {
                    playersInformation[i].panel.DisplayInfo(playersInformation[i].isReady, player.NickName, 0);
                    found = true;
                }
            if (!found)
            {
                GameObject g = Instantiate(playerPanel, playerPanelLayout);
                g.GetComponent<PlayerInfoPanel>().DisplayInfo(false, player.NickName, 0);
                playersInformation.Add(new PlayerReadyInfo(player, false, g.GetComponent<PlayerInfoPanel>()));
            }
        }
    }

    public void SaveName(InputField input)
    {
        playerName = input.text;
        if (playerName == "")
            playerName = "Didn't put in a name";
    }

    public void OnJoinedRoom()
    {
        foreach (Transform child in playerPanelLayout)
            Destroy(child.gameObject);
        StartCoroutine(DisplayPlayers());
        if (!PhotonNetwork.isMasterClient)
            photonView.RPC("AskReadyInfo", PhotonTargets.MasterClient);
    }

    [PunRPC,HideInInspector]
    public void AskReadyInfo()
    {
        StartCoroutine(DelayedReturnInfo());
    }

    public IEnumerator DelayedReturnInfo()
    {
        yield return null;
        List<bool> readyInfo = new List<bool>();
        foreach (PlayerReadyInfo player in playersInformation)
            readyInfo.Add(player.isReady);
        photonView.RPC("SendReadyPLayerInformation", PhotonTargets.All, readyInfo.ToArray());
    }

    public void OnReceivedRoomListUpdate()
    {
        rooms = PhotonNetwork.GetRoomList();
        if (roomPanelLayout.gameObject.activeInHierarchy)
            DisplayAvailableRooms();
    }

    public void ConnectToPhoton()
    {
        PhotonNetwork.ConnectUsingSettings(connectionVersion);
    }

    public void OnConnectedToPhoton()
    {
        connectingOptions.SetActive(true);
        PhotonNetwork.player.NickName = playerName;
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

    public void SetReady()
    {
        foreach (PlayerReadyInfo player in playersInformation)
            if (player.player.NickName == PhotonNetwork.player.NickName)
            {
                photonView.RPC("SetAPlayerReady", PhotonTargets.All, !player.isReady, PhotonNetwork.player.NickName);
                break;
            }
    }

    [PunRPC,HideInInspector]
    public void SetAPlayerReady(bool ready, string playerName)
    {
        foreach (PlayerReadyInfo player in playersInformation)
            if (player.player.NickName == playerName)
                player.isReady = ready;
        StartCoroutine(DisplayPlayers());
    }

    [PunRPC,HideInInspector]
    public void SendReadyPLayerInformation(bool[] readyPlayerInfo)
    {
        for (int i = 0; i < playersInformation.Count; i++)
            playersInformation[i].isReady = readyPlayerInfo[i];
    }

    [System.Serializable]
    public class PlayerReadyInfo
    {
        public PhotonPlayer player;
        public bool isReady;
        public PlayerInfoPanel panel;

        public PlayerReadyInfo(PhotonPlayer _player, bool _isReady, PlayerInfoPanel _panel)
        {
            player = _player;
            isReady = _isReady;
            panel = _panel;
        }
    }
}
