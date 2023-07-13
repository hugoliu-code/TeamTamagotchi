using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NextScene(string s)
    {
        SceneManager.LoadScene(s);
    }

    public void Quit()
    {
        Debug.Log("Pressed Quit");
        Application.Quit();
    }

}
