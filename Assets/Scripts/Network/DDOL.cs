using UnityEngine;
using UnityEngine.SceneManagement;

public class DDOL : MonoBehaviour
{

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadMultiplayer()
    {
        SceneManager.LoadScene(1);
    }
}
