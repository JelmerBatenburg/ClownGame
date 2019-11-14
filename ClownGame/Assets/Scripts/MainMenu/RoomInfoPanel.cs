using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoPanel : MonoBehaviour
{
    public Text roomNameInput;
    public Text playerAmountInput;
    private int roomIndex;
    private MainMenuNetworking networking;

    public void SetInformation(string roomName,int players, int maxPlayers, int _roomIndex,MainMenuNetworking _networking)
    {
        roomNameInput.text = roomName;
        playerAmountInput.text = players.ToString() + "/" + maxPlayers.ToString() + " Players";
        roomIndex = _roomIndex;
    }

    public void OnButtonPress()
    {
        networking.LoadRoom(roomIndex);
    }
}
