using Facebook.Unity;
using UnityEngine;
using UnityEngine.UI;

public class FBFriend : MonoBehaviour
{
    [SerializeField] private Image fAvatar;
    [SerializeField] private Text fName;
    private string userId;
    [SerializeField] private Button inviteAction;
    
    /// <summary>
    /// Инициализация френда
    /// </summary>
    /// <param name="fsprite">его фейс</param>
    /// <param name="fname">имя</param>
    /// <param name="userfId">его фейсбук айди</param>
    public void Initialization(Sprite fsprite, string fname, string userfId)
    {
        fAvatar.sprite = fsprite;
        fAvatar.preserveAspect = true;
        fName.text = fname;
        userId = userfId;
        inviteAction.onClick.AddListener(FBShare);
    }
    
    public void FBShare()
    {
        if (FB.IsLoggedIn)
        {
            FB.FeedShare(
                 toId: userId,
                 link: new System.Uri("https://apps.facebook.com/1523369521039804"),
                 linkName: "Play online with your friends",
                 picture: new System.Uri("https://scontent-frx5-1.xx.fbcdn.net/v/t39.2081-0/20991182_1526094697433953_6495329009390321664_n.jpg?oh=f11ba19281f8ffa522b54bb806ce6598&oe=5A604FE6"),
                 callback: delegate (IShareResult result)
                 {
                     if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
                     {
                         Debug.Log("ShareLink Error: " + result.Error);
                     }
                 });
        }
    }
}
