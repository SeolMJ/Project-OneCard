using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public static DialogueManager instance;

    [Header("UI")]
    public TMP_Text dialogueText;

    public static Stack<SpeachBobble> bobbles;

    void Awake()
    {
        instance = this;
        bobbles = new Stack<SpeachBobble>(4);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /* Speach Bobble */

    public static void Say(Conversation converstaion)
    {
        // Get Speach Bobble
        SpeachBobble bobble;
        if (bobbles.Count == 0)
        {
            GameObject newBobble = Instantiate(GameManager.Resource.speachBobble);
            bobble = newBobble.GetComponent<SpeachBobble>();
        }
        else
        {
            bobble = bobbles.Pop();
            if (!bobble)
            {
                Say(converstaion);
                return;
            }
        }

        // Activate
        bobble.Init(converstaion);
    }

    public static void ReturnSpeach(SpeachBobble bobble)
    {
        bobble.gameObject.SetActive(false);
        bobbles.Push(bobble);
    }

    /* Dialogue */

    public static void Talk(Conversation converstaion)
    {

    }

}

[System.Serializable]
public struct Conversation
{

    [Header("Speaker")]
    public Transform speaker;
    public Vector2 offset;

    [Header("Conversation")]
    public string content;
    public Choice[] choices;

}

[System.Serializable]
public struct Choice
{

    public string content;
    public string command;

}