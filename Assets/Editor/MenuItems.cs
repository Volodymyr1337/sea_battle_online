using UnityEngine;
using UnityEditor;

public class MenuItems : MonoBehaviour
{
    [MenuItem("[ Configure ]/Clear PlayerPrefs")]
    static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Player prefs removed.");
    }
}

