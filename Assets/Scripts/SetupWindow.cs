using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupWindow : MonoBehaviour
{
    public Slider expBar;
    public InputField userName;
    public Text lvlLabel;

    private void Start()
    {
        userName.text = PlayerPrefs.HasKey("userName") ? PlayerPrefs.GetString("userName") : "";
        expBar.value = PlayerPrefs.HasKey("usrExpirience") ? PlayerPrefs.GetInt("usrExpirience") % 100 : 0;
        lvlLabel.text = PlayerPrefs.HasKey("usrExpirience") ? (PlayerPrefs.GetInt("usrExpirience") / 100).ToString() : "0";
    }

    public void SetUserName()
    {
        PlayerPrefs.SetString("userName", userName.text);
        PlayerNetwork.Instance.PlayerName = PlayerPrefs.GetString("userName");
    }
    
}
