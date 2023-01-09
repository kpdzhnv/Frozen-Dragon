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
    Dialogue dialogue;

    public bool isPaused;
    public bool dialogueStarted = true;

    private void Awake()
    {
        gms = GetComponent<GameManagerScript>();

        gms.Pause();

        dialogue = new Dialogue(0);
        dialogueStarted = true;
        OnDialogue(dialogue);
        DisplayNextSentence(dialogue);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (isPaused && !dialogueStarted)
                OnPlay();
            else if (!dialogueStarted)
                OnPause();
        }
        if (dialogueStarted && (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)))
        {
            OnDialogue(dialogue);
            DisplayNextSentence(dialogue);
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
    public void OnDialogue(Dialogue dialogue)
    {
        isPaused = true;
        gms.Pause();

        inGameCanvas.enabled = false;
        pauseCanvas.enabled = false;
        dialogueCanvas.enabled = true;

    }

    public void DisplayNextSentence(Dialogue dialogue)
    {
        if (dialogue.SentenceCount() == 0)
        {
            EndDialogue();
            return;
        }

        Transform leftDialogue = dialogueCanvas.transform.GetChild(0).GetChild(0);
        Transform rightDialogue = dialogueCanvas.transform.GetChild(0).GetChild(1);
        TextMeshProUGUI name;
        TextMeshProUGUI text;

        if (dialogue.left)
        {
            rightDialogue.gameObject.SetActive(false);
            leftDialogue.gameObject.SetActive(true);

            name = leftDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();
            text = leftDialogue.GetChild(1).GetComponent<TextMeshProUGUI>();

            name.text = dialogue.name;
            text.text = dialogue.NextSentence();
        }
        else
        {
            rightDialogue.gameObject.SetActive(true);
            leftDialogue.gameObject.SetActive(false);

            name = rightDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();
            text = rightDialogue.GetChild(1).GetComponent<TextMeshProUGUI>();

            name.text = dialogue.name;
            text.text = dialogue.NextSentence();
        }
    }

    public void EndDialogue()
    {
        dialogueStarted = false;
        OnPlay();
    }

    public void OnQuit()
    {
        gms.Quit();
    }

}
