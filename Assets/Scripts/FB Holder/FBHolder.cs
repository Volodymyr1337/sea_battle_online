using UnityEngine.UI;
using UnityEngine;
using Facebook.Unity;
using System;
using System.Collections.Generic;

public class FBHolder : MonoBehaviour
{
    public Image Avatar;    // мой аватар

    private string FriendsText;

    object data;            // данные о друзьях, полученные гет запросом

    private void Awake()
    {
        FBInitialize();
    }

    private void Start()
    {
        AlreadyLoggedIn();
    }

    #region FB initialization

    private void AlreadyLoggedIn()
    {
        if (FB.IsLoggedIn)
        {
            FB.API("me/picture?type=square&height=128&width=128", HttpMethod.GET, GetPicture);
        }
    }

    private void FBInitialize()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(SetInit, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void SetInit()
    {
        Debug.Log("FB init done.");        
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Debug.Log("Pause!");
        }
        else
        {
            Debug.Log("Resume!");
        }
    }

    #endregion

    public void FBLogin()
    {
        if (FB.IsLoggedIn)
        {
            FBShare();
        }
        else
        {
            FB.LogInWithReadPermissions(new string[] { "public_profile", "user_friends" }, AuthCallback);
        }        
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("Login success!");
            FB.API("/me?fields=name", HttpMethod.GET, GetName);
            FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, GetPicture);
            FB.API("/me/friends?fields=id,name", HttpMethod.GET, GetFriends);
            FBShare();
        }
        else
        {
            Debug.LogError("Log in failed!");
        }
    }

    private void GetName(IResult result)
    {
        if (result.Error != null)
        {
            Debug.LogError(result.Error);
        }
        else
        {
            PlayerNetwork.Instance.PlayerName = result.ResultDictionary["name"].ToString();
            PlayerPrefs.SetString("userName", result.ResultDictionary["name"].ToString());
        }
    }

    // Получить аватар
    private void GetPicture(IGraphResult result)
    {
        if (result.Error == null && result.Texture != null)
            Avatar.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
    }

    // Получить список друзей
    private void GetFriends(IGraphResult result)
    {
        FriendsText = string.Empty;
        
       
        List<object> dataList = new List<object>();
        if (result.ResultDictionary.TryGetValue("data", out data))
            dataList = (List<object>)data;
        
        foreach (Dictionary<string, object> obj in dataList)
        {
            FriendsText += obj["name"].ToString() + " | ";
        }
        Debug.Log(FriendsText + " " + dataList.Count);
    }

    #region FB Share

    public void FBShare()
    {
        if (FB.IsLoggedIn)
        {
            FB.ShareLink(
                contentURL: new Uri("https://apps.facebook.com/1523369521039804"), 
                contentTitle: "Let's play", 
                contentDescription: "Play online with your friends", 
                photoURL: new Uri("https://scontent-frx5-1.xx.fbcdn.net/v/t39.2081-0/20991182_1526094697433953_6495329009390321664_n.jpg?oh=f11ba19281f8ffa522b54bb806ce6598&oe=5A604FE6"), 
                callback: ShareCallback);
        }
    }
    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink Error: " + result.Error);
        }
        else if (!string.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        }
        else
        {
            // Share succeeded without postID
            Debug.Log("ShareLink success!");
        }
    }

    #endregion

    //FB.AppRequest("Custom message", null, null, null, null, "Data", "Challenge your friends!", ChallengeCallback);
    void ChallengeCallback(IAppRequestResult result)
    {
        if (result.Cancelled)
        {
            Debug.Log("Challenge cancelled.");
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("Error in challenge:" + result.Error);
        }
        else
        {
            Debug.Log("Challenge was successful:" + result.RawResult);
        }
    }
}
