using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    [SerializeField]
    private Text _roomName;
    private Text RoomName
    {
        get { return _roomName; }
    }

    public void OnClick_CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2 };

        if (PhotonNetwork.CreateRoom(RoomName.text, roomOptions, TypedLobby.Default))
            print("create room successfully sent");
        else
            print("create room failed tp send");
    }

    private void OnPhotonCreateRoomFiled(object[] codeAndMessage)
    {
        print("Create room failed #" + codeAndMessage[1]);
    }

    private void OnCreatedRoom()
    {
        print("room created successfully");
    }

}
