using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    [SerializeField]
    private Text _roomNameText;
    private Text RoomNameText
    {
        get { return _roomNameText; }
    }

    public string RoomName { get; private set; }

    public bool Updated { get; set; }

    public Text RoomPlayersCountTxt;

    private void Start()
    {
        GameObject lobbyCanvasObj = MainCanvas.Instance.LobbyCanvas.gameObject;
        if (lobbyCanvasObj == null)
            return;

        LobbyCanvas lobbyCanvas = lobbyCanvasObj.GetComponent<LobbyCanvas>();
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() => lobbyCanvas.OnClickJoinRoom(RoomNameText.text));
    }

    private void OnDestroy()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
    }

    public void SetRoomNameText(string txt)
    {
        RoomName = txt;
        RoomNameText.text = RoomName;
    }
}
