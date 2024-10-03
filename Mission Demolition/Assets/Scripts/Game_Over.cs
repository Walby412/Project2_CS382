using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Over : MonoBehaviour
{
    public void OnResetButton(){
        SceneManager.LoadScene(0);
    }

    public void OnQuitButton(){
        Application.Quit();
    }
}
