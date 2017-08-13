using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuButtons;
    public GameObject setupWindow;

    private void Start()
    {
        menuButtons.SetActive(true);
        setupWindow.SetActive(false);
    }
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
    
    public void EnableSetup()
    {
        menuButtons.SetActive(false);
        setupWindow.SetActive(true);
    }
    public void BackToMenu()
    {
        menuButtons.SetActive(true);
        setupWindow.SetActive(false);
    }
}
