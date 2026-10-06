using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public float displayDuration = 3f;  // Duration for which each message is displayed
    public float fadeDuration = 1f;     // Duration of the fade-out effect
    public string[] messages;           // List of messages to display

    private TMP_Text textComponent;
    private int currentMessageIndex = 0;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (messages.Length > 0)
        {
            DisplayNextMessage();
        }
    }

    void DisplayNextMessage()
    {
        if (currentMessageIndex < messages.Length)
        {
            StartCoroutine(DisplayMessage(messages[currentMessageIndex]));
            currentMessageIndex++;
        }
    }

    IEnumerator DisplayMessage(string message)
    {
        textComponent.text = message;
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f); // fully opaque
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        float fadeRate = 1f / fadeDuration;
        while (textComponent.color.a > 0f)
        {
            float newAlpha = textComponent.color.a - fadeRate * Time.deltaTime;
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, newAlpha);
            yield return null;
        }

        // Display the next message or do something else when all messages are shown
        DisplayNextMessage();
    }
}
