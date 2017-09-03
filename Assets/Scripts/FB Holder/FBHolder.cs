using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FBHolder : MonoBehaviour
{
    

    public void FacebookInit()
    {
        FB.Init(SetInit, OnHideUnity);
    }

    private void SetInit()
    {
        Debug.Log("FB init done.");

        if (FB.IsLoggedIn)
        {
            Debug.Log("logged in!");
        }
        else
        {
            FBLogin();
        }
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

    private void FBLogin()
    {
        FB.LogInWithPublishPermissions(new string[] { "public_profile" }, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("Login success!");
            FB.API("/me?fields=name", HttpMethod.GET, GetName);
            // та хз надо ли?
            //FB.API("me/picture?type=square&height=128&width=128", HttpMethod.GET, GetPicture);
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
            Debug.Log("Hi there: " + result.ResultDictionary["name"]);
            PlayerNetwork.Instance.PlayerName = result.ResultDictionary["name"].ToString();
            PlayerPrefs.SetString("userName", result.ResultDictionary["name"].ToString());
        }
    }
    private void GetPicture(IGraphResult result)
    {
        if (result.Error == null && result.Texture != null)
        {
            //Avatarka.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2());
        }
    }
}
