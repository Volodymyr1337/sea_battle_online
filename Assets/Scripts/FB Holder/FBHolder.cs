using UnityEngine.UI;
using UnityEngine;
using Facebook.Unity;
using System;
using System.Collections.Generic;

public class FBHolder : MonoBehaviour
{
    public Image Avatar;                // мой аватар

    public GameObject FriendGrid;       // контейнер с друзьями
    public GameObject FriendPrefab;     // префаб друга
    
    object data;                        // данные о друзьях, полученные гет запросом

    private const int FRIENDCOUNT = 100; // макс кол-во отображаемых пользователей

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
            FB.API("/me/friends?fields=id,name", HttpMethod.GET, GetFriends);
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
        if (!FB.IsLoggedIn)
        {
            FB.LogInWithReadPermissions(new string[] { "public_profile", "user_friends" }, AuthCallback);
        }
        else
        {
            FB.ShareLink(
                contentURL: new Uri("https://apps.facebook.com/1523369521039804"),
                contentTitle: "Let's play",
                contentDescription: "Play online with your friends",
                photoURL: new Uri("https://drive.google.com/open?id=0B3P-N9XGUZvQQlZjbkQ1b2pRMzA"),
                callback: delegate(IShareResult result) 
                {
                    if (result.Error != null)
                    {
                        Debug.LogError(result.Error);
                    }
                });
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
            // записываем имя, если абонент ещё не определился какой логин придумать
            if (PlayerPrefs.GetString("userName") == null || PlayerPrefs.GetString("userName") == "unnamed")
            {
                PlayerNetwork.Instance.PlayerName = result.ResultDictionary["name"].ToString();
                PlayerPrefs.SetString("userName", result.ResultDictionary["name"].ToString());
            }
                
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
        List<object> dataList = new List<object>();
        if (result.ResultDictionary.TryGetValue("data", out data))
            dataList = (List<object>)data;

        int i = FRIENDCOUNT;

        foreach (Dictionary<string, object> obj in dataList)
        {
            if (i < 0)
                break;
            i++;

            GameObject friend = Instantiate(FriendPrefab, FriendGrid.transform);
            FB.API("https" + "://graph.facebook.com/" + obj["id"].ToString() + "/picture?width=128&height=128", HttpMethod.GET, delegate (IGraphResult res)
            {
                friend.GetComponent<FBFriend>().Initialization(
                    Sprite.Create(res.Texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100), 
                    obj["name"].ToString(), 
                    obj["id"].ToString() );
            });
        }
    }
}
