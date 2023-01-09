using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class Dialogue
{
    public string name;
    public Queue<string> sentences;
    public bool left;

    public string[] dialogues = new string[3]
    {@"n: Dragon
t: So cold...
t: What happened?
t: ...
t: I am hungry. I must find something to eat.",
    @"n: Dragon
t: Wow, a carrot!
t: I wabt to eat it right away...
t: But then I wont have any carrots left.
t: I should plant it to get more carrots!",
    @"n: Dragon
t: Wow, a carrot!
t: I wabt to eat it right away...
t: But then I wont have any carrots left.
t: I should plant it to get more carrots!"};

    public Dialogue(int i)
    {
        sentences = new Queue<string>();
        char[] delims = new[] { '\r', '\n' };
        List<string> fileLines = dialogues[i].Split(delims, StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (string line in fileLines)
        {
            if (line.StartsWith('n'))
            {
                name = line.Substring(3, line.Length - 3);
                if (name == "Dragon")
                    left = true;
                else
                    left = false;
            }
            else
            {
                string t = line.Substring(3, line.Length - 3);
                sentences.Enqueue(t);
            }
        }
    }

    public string NextSentence()
    {
        return sentences.Dequeue();
    }
    public int SentenceCount()
    {
        return sentences.Count(); ;
    }

}
