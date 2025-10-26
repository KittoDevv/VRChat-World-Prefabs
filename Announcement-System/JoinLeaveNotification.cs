using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class JoinLeaveNotification : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Join/Leave Notification | Prefab by Kitto Dev")]
    [Space(-8)]
    [Header("Last Updated: 10/25/2025 | Version 1.0")]

    [Header("Object References")]
    [Tooltip("Sound that plays when a player joins.")]
    public AudioSource joinSound;

    [Tooltip("Sound that plays when a player leaves.")]
    public AudioSource leaveSound;    
    [Tooltip("The GameObject that will be shown when the notification is triggered.")]
    public GameObject visibleObject;
    [Tooltip("The Text Component that is updated to display the new notification. Currently only supports TextMeshPro.")]
    public TextMeshPro textMesh;

    [Header("Notification Settings")]
    [Tooltip("Time in seconds the object will be displayed before fading out.")]
    public float displayTime = 5f;
    [Tooltip("Duration of the fade effect in seconds.")]
    public float fadeDuration = 0.5f;
    public bool disableNotificationSound = false;

    private float elapsedTime;

    void Start()
    {
        visibleObject.SetActive(false);
        SetTextAlpha(0);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        {
            ShowNotification($"{player.displayName} <color=#15ff00>Joined</color>", joinSound);
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        ShowNotification($"{player.displayName} <color=#ff0000>Left</color>", leaveSound);
    }

    private void ShowNotification(string message, AudioSource sound)
    {
        textMesh.text = message;

        if (!disableNotificationSound && sound != null)
        {
            sound.Play();
        }

        visibleObject.SetActive(true);
        FadeInText();
        SendCustomEventDelayedSeconds("HideObjectAfterDelay", displayTime);
    }

    public void HideObjectAfterDelay()
    {
        FadeOutText();
    }

    public void HideObject()
    {
        visibleObject.SetActive(false);
    }

    private void FadeInText()
    {
        elapsedTime = 0;
        SendCustomEventDelayedSeconds("IncreaseTextAlpha", 0);
    }

    public void IncreaseTextAlpha()
    {
        if (elapsedTime < fadeDuration)
        {
            SetTextAlpha(Mathf.Lerp(0, 1, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            SendCustomEventDelayedSeconds("IncreaseTextAlpha", Time.deltaTime);
        }
        else
        {
            SetTextAlpha(1);
        }
    }

    public void FadeOutText()
    {
        elapsedTime = 0;
        SendCustomEventDelayedSeconds("DecreaseTextAlpha", 0);
    }

    public void DecreaseTextAlpha()
    {
        if (elapsedTime < fadeDuration)
        {
            SetTextAlpha(Mathf.Lerp(1, 0, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            SendCustomEventDelayedSeconds("DecreaseTextAlpha", Time.deltaTime);
        }
        else
        {
            SetTextAlpha(0);
            HideObject();
        }
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = textMesh.color;
        color.a = alpha;
        textMesh.color = color;
    }
}