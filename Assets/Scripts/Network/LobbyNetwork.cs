using UnityEngine;

public class LobbyNetwork : MonoBehaviour
{
    private void Start()
    {
        print("connecting to server...");
        PhotonNetwork.ConnectUsingSettings("0.0.0");
        PhotonNetwork.autoJoinLobby = false;
    }

    private void OnConnectedToMaster()
    {
        print("Connected to Master.");
        PhotonNetwork.automaticallySyncScene = false;
        PhotonNetwork.playerName = PlayerNetwork.Instance.PlayerName;
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    private void OnJoinedLobby()
    {
        if (!PhotonNetwork.inRoom)      // если не в комнате - возврат на предыдущий канвас
            MainCanvas.Instance.LobbyCanvas.transform.SetAsLastSibling();

        print("joined lobby");
    }

}
