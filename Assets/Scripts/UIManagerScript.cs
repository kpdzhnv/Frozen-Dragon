using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManagerScript : MonoBehaviour
{
    public Canvas inGameCanvas;
    public Canvas pauseCanvas;
    public Canvas dialogueCanvas;

    GameManagerScript gms;
    public bool isPaused;

    private void Awake()
    {
        gms = GetComponent<GameManagerScript>();
        OnPause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (isPaused)
                OnPlay();
            else
                OnPause();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            OnDialogue();
        }
    }

    public void OnPlay()
    {
        isPaused = false;
        gms.Play();

        inGameCanvas.enabled = true;
        pauseCanvas.enabled = false;
        dialogueCanvas.enabled = false;
    }

    public void OnPause()
    {
        isPaused = true;
        gms.Pause();

        inGameCanvas.enabled = false;
        pauseCanvas.enabled = true;
        dialogueCanvas.enabled = false;
    }
    public void OnDialogue()
    {
        gms.Pause();
        inGameCanvas.enabled = false;
        pauseCanvas.enabled = false;
        dialogueCanvas.enabled = true;

        // panel = dialogueCanvas.transform.GetChild(0)
        Transform leftDialogue = dialogueCanvas.transform.GetChild(0).GetChild(0);
        Transform rightDialogue = dialogueCanvas.transform.GetChild(0).GetChild(1);

        TextMeshProUGUI leftName = leftDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rightName = rightDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI leftText = rightDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rightText = rightDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();

        leftName.text = "NEW NAME";
        rightName.text = "NEW NAME";
        leftText.text = "NEW TEXT";
        rightText.text = "NEW TEXT";

    }

    public void OnQuit()
    {
        gms.Quit();
    }

}
