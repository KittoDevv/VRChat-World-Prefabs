using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class Notification : UdonSharpBehaviour
{

    [Space(-8)]
    [Header("Notification System | Prefab by Kitto Dev")]
    [Space(-8)]
    [Header("Last Updated: 10/25/2025 | Version 1.0")]

    [Header("Object References")]
    [Tooltip("The AudioSource which will play when the notification is triggered.")]
    public AudioSource notificationSound; // Notification sound
    [Tooltip("The GameObject that will be shown when the notification is triggered.")]
    public GameObject visibleObject; // Object to show
    [Tooltip("The Text Component that is updated to display the new notification. Currently only supports TextMeshPro.")]
    public TextMeshPro textMesh; // TextMeshPro component to display message
    [Header("Notification Settings")]
    [Tooltip("This will be displayed when the notification is triggered.")]
    public string notificationMessage = "Thanks for using my prefab!"; // Settable notification text
    [Tooltip("Time in seconds the object will be displayed before fading out.")]
    public float displayTime = 5f; // Time to display the object
    [Tooltip("Duration of the fade effect in seconds.")]
    public float fadeDuration = 0.5f; // Duration of the fade effect
    public bool disableNotificationSound = false; // Option to disable the notification sound
    [Tooltip("This will add a Notification prefix at the start of the message.")]
    public bool addPrefix = true;
    private string prefixText = "<color=#005DFF>Notification</color>: ";

    public bool sendToOthers = false; // Option to send notification to other players only
    public bool sendToLocal = false; // Option to send notification to local player only
    [Tooltip("Enable this to allow notifications to be triggered by Triggers. Requires a Collider set as Trigger on the same object as this script.")]
    public bool enableColliderTrigger = false; // Option to enable/disable the collider trigger functionality

    [Header("Cooldown Settings")]
    [Tooltip("Enable cooldown to limit how often notifications can be sent.")]
    public bool enableCooldown = false; // Toggle for enabling cooldown
    [Tooltip("Length of the cooldown in seconds.")]
    public float cooldownLength = 15f; // Length of the cooldown in seconds

    private bool isCooldownActive = false;
    private float cooldownEndTime = 0f;
    private float elapsedTime;

    void Start()
    {
        visibleObject.SetActive(false);
        SetTextAlpha(0);
        textMesh.text = GetFormattedMessage();
    }

    void Update()
    {
        if (isCooldownActive)
        {
            if (Time.realtimeSinceStartup >= cooldownEndTime)
            {
                isCooldownActive = false;
            }
        }
    }

    public void _sendNotification()
    {
        if (!enableCooldown || (enableCooldown && !isCooldownActive))
        {
            // Ensure local player owns the object before sending network events
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            SendNotification();
            if (enableCooldown)
            {
                isCooldownActive = true;
                cooldownEndTime = Time.realtimeSinceStartup + cooldownLength;
            }
        }
    }

    public void _testingignoreowner()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisplayNotification");
    }

    public override void Interact()
    {
        _sendNotification();
    }

    private void SendNotification()
    {
        if (sendToOthers && sendToLocal)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisplayNotification");
        }
        else if (sendToOthers)
        {
            Debug.Log("Send To Others Fired");
            if (Networking.LocalPlayer.displayName != Networking.GetOwner(gameObject).displayName)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisplayNotificationToOthers");
        }
        else if (sendToLocal)
        {
            DisplayNotification();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisplayNotification");
        }
    }

    private string GetFormattedMessage()
    {
        return addPrefix ? prefixText + notificationMessage : notificationMessage;
    }

    public void DisplayNotification()
    {
        textMesh.text = GetFormattedMessage();
        PlayNotificationSound();
        ShowObject();
        FadeInText();
    }

    public void DisplayNotificationToOthers()
    {
        if (Networking.LocalPlayer.displayName != Networking.GetOwner(gameObject).displayName)
        {
            textMesh.text = GetFormattedMessage();
            PlayNotificationSound();
            ShowObject();
            FadeInText();
        }
    }

    private void PlayNotificationSound()
    {
        if (!disableNotificationSound && notificationSound != null)
        {
            notificationSound.Play();
        }
    }

    private void ShowObject()
    {
        visibleObject.SetActive(true);
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

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!enableColliderTrigger) return;

        Networking.SetOwner(player, gameObject);

        if (player.isLocal)
        {
            if (sendToLocal || (!sendToLocal && !sendToOthers))
            {
                _sendNotification();
            }
        }
        else
        {
            if (sendToOthers)
            {
                _sendNotification();
            }
        }
    }
}
