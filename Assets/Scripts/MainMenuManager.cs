using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

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
        if (PlayerPrefs.GetInt("Sound") == 1) SoundManager.Instance.ClickButton();

        PlayerNetwork.Instance.isMultiplayerGame = false;
        SceneManager.LoadScene("Play");
    }

    public void LoadMultiplayer()
    {
        if (PlayerPrefs.GetInt("Sound") == 1) SoundManager.Instance.ClickButton();

        PlayerNetwork.Instance.isMultiplayerGame = true;
        SceneManager.LoadScene("Lobby");
    }
    
    public void EnableSetup()
    {
        if (PlayerPrefs.GetInt("Sound") == 1) SoundManager.Instance.ClickButton();

        menuButtons.SetActive(false);
        setupWindow.SetActive(true);
    }
    public void BackToMenu()
    {
        if (PlayerPrefs.GetInt("Sound") == 1) SoundManager.Instance.ClickButton();

        menuButtons.SetActive(true);
        setupWindow.SetActive(false);
    }
}
