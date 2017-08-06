using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    private AsyncOperation LoadAsync;
    [SerializeField] private Slider loadingBar;


	void Start ()
    {
        LoadAsync = SceneManager.LoadSceneAsync("MainMenu");
        LoadAsync.allowSceneActivation = false;
	}
	
	void Update ()
    {
        if (loadingBar.value > 0.9f)
            loadingBar.value += 0.01f;
        else if (LoadAsync.progress == 0.9f)
            loadingBar.value = 0.91f;
        else
            loadingBar.value = LoadAsync.progress;

       
        if (loadingBar.value > 0.98f)
            LoadAsync.allowSceneActivation = true;
	}
}
