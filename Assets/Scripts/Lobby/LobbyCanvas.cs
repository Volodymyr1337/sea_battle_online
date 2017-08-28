using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCanvas : MonoBehaviour
{
    [SerializeField]
    private RoomLayoutGroup _roomLayoutGroup;
    private RoomLayoutGroup RoomLayoutGroup
    {
        get { return _roomLayoutGroup; }
    }

    public void OnClickJoinRoom(string roomName)
    {
        if (PlayerPrefs.GetInt("Sound") == 1) SoundManager.Instance.ClickButton();

        if (PhotonNetwork.JoinRoom(roomName))
            print("join room success!");
        else
            print("join room failed!");
    }
}
