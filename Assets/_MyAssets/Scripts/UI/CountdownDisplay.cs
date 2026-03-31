using UnityEngine;
using TMPro;

public class CountdownDisplay : MonoBehaviour
{
    public AIController aiController;
    public GameObject timerUI;
    public TextMeshProUGUI countdownTextMeshPro;
    public AudioSource audioSource;
    public AudioClip audioClip;
    public bool alertOn;

    float lastDisplayedValue;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        alertOn = false;
    }

    void Update()
    {
        if (aiController == null)
        {
            timerUI.SetActive(false);
            return;
        }

        float countdownValue = aiController.alarmTimer;

        if (countdownValue > 1)
        {
            timerUI.SetActive(true);

            // Only update text when the displayed value actually changes (avoids string alloc per frame)
            float truncated = Mathf.Floor(countdownValue * 100f) * 0.01f;
            if (truncated != lastDisplayedValue)
            {
                lastDisplayedValue = truncated;
                countdownTextMeshPro.text = countdownValue.ToString("F2");
            }

            if (!alertOn)
            {
                alertOn = true;
                audioSource.PlayOneShot(audioClip);
            }
        }
        else
        {
            timerUI.SetActive(false);
            alertOn = false;
            lastDisplayedValue = 0;
        }
    }

    public void SetAIController(AIController newAIController)
    {
        aiController = newAIController;
    }
}
