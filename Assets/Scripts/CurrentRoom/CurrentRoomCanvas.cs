using UnityEngine;

public class CurrentRoomCanvas : MonoBehaviour
{
    //
    // Кнопка старт (доступена только создателю комнаты)
    //
    public void OnCLickStart()
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.room.IsVisible = false;
        PhotonNetwork.LoadLevel("Play");
    }
}
