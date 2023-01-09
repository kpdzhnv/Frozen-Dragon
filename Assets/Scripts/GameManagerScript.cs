using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public bool isPaused;

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
    }
    public void Play()
    {
        isPaused = false;
        Time.timeScale = 1;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
