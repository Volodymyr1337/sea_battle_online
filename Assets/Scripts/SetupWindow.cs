using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupWindow : MonoBehaviour
{
    public Slider expBar;
    public InputField userName;
    public Text lvlLabel;
    public Text soundLabel;

    private void Start()
    {
        userName.text = PlayerPrefs.HasKey("userName") ? PlayerPrefs.GetString("userName") : "";
        expBar.value = PlayerPrefs.HasKey("usrExpirience") ? PlayerPrefs.GetInt("usrExpirience") % 100 : 0;
        lvlLabel.text = "R a n k: " + (PlayerPrefs.HasKey("usrExpirience") ? (PlayerPrefs.GetInt("usrExpirience") / 100).ToString() : "0");
        soundLabel.text = "S o u n d s : " + (PlayerPrefs.GetInt("Sound") == 1 ? "o f f" : "o n");
    }

    public void SetUserName()
    {
        PlayerPrefs.SetString("userName", userName.text);
        PlayerNetwork.Instance.PlayerName = PlayerPrefs.GetString("userName");
    }
    
    public void EnableSound()
    {
        PlayerPrefs.SetInt("Sound", PlayerPrefs.GetInt("Sound") == 1 ? 0:1);
        soundLabel.text = "S o u n d s : " + (PlayerPrefs.GetInt("Sound") == 1 ? "o f f" : "o n");
    }
}
