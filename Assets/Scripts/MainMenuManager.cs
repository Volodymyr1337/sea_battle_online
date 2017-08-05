using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    public void LoadSinglePlayer()
    {
        PlayerNetwork.Instance.isMultiplayerGame = false;
        SceneManager.LoadScene("Play");
    }

    public void LoadMultiplayer()
    {
        PlayerNetwork.Instance.isMultiplayerGame = true;
        SceneManager.LoadScene("Lobby");
    }
}
