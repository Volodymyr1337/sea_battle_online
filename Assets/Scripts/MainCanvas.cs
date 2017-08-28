using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas Instance;

    [SerializeField]
    private LobbyCanvas _lobbyCanvas;
    public LobbyCanvas LobbyCanvas
    {
        get { return _lobbyCanvas; }
    }

    [SerializeField]
    private CurrentRoomCanvas _currentRoomCanvas;
    public CurrentRoomCanvas CurrentRoomCanvas
    {
        get { return _currentRoomCanvas; }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void LeaveToMenu()            // выход в меню
    {
        if (PlayerPrefs.GetInt("Sound") == 1) SoundManager.Instance.ClickButton();

        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }
}
