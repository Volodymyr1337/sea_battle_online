using UnityEngine;
using UnityEngine.UI;

public class FBFriend : MonoBehaviour
{
    [SerializeField] private Image fAvatar;
    [SerializeField] private Text fName;

    public void Initialization(Sprite fsprite, string fname)
    {
        fAvatar.sprite = fsprite;
        fAvatar.preserveAspect = true;
        fName.text = fname;
    }
}
