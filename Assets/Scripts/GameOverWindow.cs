using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverWindow : MonoBehaviour
{
    public Text PlayerWonLabel;

    public Image currUserAvatar;    // пока не используется
    public Slider expLine;       // пока не используется

    public Text currPlayerLabel;
    
    public Text player1GridPlayerName;
    public Text player2GridPlayerName;

    public Text currShipsKilled;
    public Text enemyShipsKilled;

    public Text currExpRecived;
    public Text enemyExpRecived;

    public Text currPlayerLvl;
    public Text enemyPlayerLvl;

   

    private static GameOverWindow instance;
    public static GameOverWindow Instance
    {
        get
        {
            if (instance == null)
                return FindObjectOfType<GameOverWindow>();
            else
                return instance;
        }
    }

    public void GameOver(Hashtable hashparams)
    {
        PlayerWonLabel.text = hashparams["winner"] as string + " won!";
        
        player2GridPlayerName.text = hashparams["looser"] as string;
        currShipsKilled.text = hashparams["usrKills"] as string;
        enemyShipsKilled.text = hashparams["aiKills"] as string;
        currExpRecived.text = hashparams["userExp"] as string;
        enemyExpRecived.text = hashparams["aiExp"] as string;

       // expLine.value = PlayerPrefs.GetInt("usrExpirience") % 100;

        currPlayerLvl.text = (PlayerPrefs.GetInt("usrExpirience") / 100).ToString();
        enemyPlayerLvl.text = (PlayerPrefs.GetInt("AiExpirience") / 100).ToString();
    }

    public void GameOver(string scndUser, int shipsKilled, int scndUsrExp)
    {
        PlayerWonLabel.text = ShipSortingScene.Instance.ShipListing.Count > 0 ? PlayerNetwork.Instance.PlayerName + " won!" : scndUser + " won!";
        player2GridPlayerName.text = scndUser;
        currShipsKilled.text = (10 - shipsKilled).ToString();
        enemyShipsKilled.text = (10 - ShipSortingScene.Instance.ShipListing.Count).ToString();

        currExpRecived.text = ShipSortingScene.Instance.ShipListing.Count > 0 ? ((10 - shipsKilled) * 3).ToString() : ((10 - shipsKilled) * 1.3).ToString();
        enemyExpRecived.text = ShipSortingScene.Instance.ShipListing.Count > 0 ? ((10 - ShipSortingScene.Instance.ShipListing.Count) * 1.3).ToString() : ((10 - ShipSortingScene.Instance.ShipListing.Count) * 3).ToString();

       // expLine.value = PlayerPrefs.GetInt("usrExpirience") % 100;
        currPlayerLvl.text = (PlayerPrefs.GetInt("usrExpirience") / 100).ToString();
        enemyPlayerLvl.text = (scndUsrExp / 100).ToString();

        if (ShipSortingScene.Instance.ShipListing.Count > 0)
        {
            PlayerPrefs.SetInt("usrExpirience", PlayerPrefs.GetInt("usrExpirience") + 30);
        }
        else
            PlayerPrefs.SetInt("usrExpirience", PlayerPrefs.GetInt("usrExpirience") + (int)((10 - shipsKilled) * 1.3));

    }

    private void OnEnable()
    {
        currPlayerLabel.text = PlayerNetwork.Instance.PlayerName;
        player1GridPlayerName.text = PlayerNetwork.Instance.PlayerName;
    }
}
