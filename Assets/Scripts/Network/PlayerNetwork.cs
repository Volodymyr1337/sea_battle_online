using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerNetwork : MonoBehaviour
{
    public static PlayerNetwork Instance;
    public string PlayerName { get; set; }
    private PhotonView PhotonView;
    private int PlayersInGame = 0;

    public bool isMultiplayerGame;      // флаг для проверки многопользовательская игра или нет

    public ShootingArea shootingArea;

    private void Awake()
    {
        Instance = this;

        isMultiplayerGame = false;

        shootingArea = new ShootingArea();
        PhotonView = GetComponent<PhotonView>();

        if (!PlayerPrefs.HasKey("userName"))
            PlayerPrefs.SetString("userName", "unnamed");

        if (!PlayerPrefs.HasKey("Sound"))
            PlayerPrefs.SetInt("Sound", 1);

        if (!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetInt("Music", 1);

        PlayerName = PlayerPrefs.GetString("userName");


        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (isMultiplayerGame && scene.name == "Play")
        {
            if (PhotonNetwork.isMasterClient)
                MasterLoadedGame();
            else
                NonMasterLoadedGame();
        }
    }

    private void MasterLoadedGame()
    {
        PhotonView.RPC("RPC_LoadedGameScene", PhotonTargets.MasterClient);

        PhotonView.RPC("RPC_LoadGameOthers", PhotonTargets.Others);
    }

    private void NonMasterLoadedGame()
    {
        PhotonView.RPC("RPC_LoadedGameScene", PhotonTargets.MasterClient);
    }

    [PunRPC]
    private void RPC_LoadGameOthers()
    {
        PhotonNetwork.LoadLevel("Play");
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        PlayersInGame++;

        if (PlayersInGame == PhotonNetwork.playerList.Length)
        {
            print("all users in the game scene!");
            PlayersInGame = 0;
            PhotonView.RPC("RPC_CreatePlayer", PhotonTargets.All);
        }
    }

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        Vector3 spawnPos = new Vector3(1f, -6f, 0f);
        PhotonNetwork.Instantiate("User", spawnPos, Quaternion.identity, 0);
    }

    public void RapidFire(int area)
    {
        int mask = 15;
        
        shootingArea.sizeX = (float)((area >> 4) & mask);
        shootingArea.sizeY = (float)(area & mask);
    }


}
