using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AnnouncementSystem : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("World Announcement System | Prefab by Kitto Dev")]
    [Space(-8)]
    [Header("Last Updated: 2/1/2026 | Version 1.10 Announcer Name Update")]
    
    [Header("Please run in VRChat to test this prefab.")]
    [Space(-8)]
    [Header("This uses networking features that won't work in ClientSim.")]

    [Header("Object References")]
    [Tooltip("Audio source to play notification sound. Leave empty for no sound.")]
    public AudioSource notificationSound;
    [Tooltip("The GameObject that contains or has the TextMeshPro component.")]
    public GameObject visibleObject;
    [Tooltip("Required. TextMeshPro (non-UI) component to display announcements. It's recommended to use the Overlay shader so Text doesn't clip into world geometry, and also disable Dynamic Occlusion on the Mesh Renderer, so it doesn't get culled by Occlusion Culling (if used by your scene).")]
    public TextMeshPro textMesh;
    [Tooltip("Required. Used to input announcement text. Currently only support TextMeshPro components.")]
    public TMP_InputField inputField;
    [Tooltip("Object that contains the GUI and destroy it, preventing access to the GUI while allowing Notifications to still work.")]
    public GameObject guiObject;

    [Header("Staff Settings")]
    [Tooltip("Text file containing admin usernames. Compatible with Reimajo's Admin Tool Panel.")]
    [SerializeField] private TextAsset adminFile;

    [Tooltip("Text file containing moderator usernames. Compatible with Reimajo's Admin Tool Panel.")]
    [SerializeField] private TextAsset moderatorFile;

    [Tooltip("Put people's display names that are allowed to use this system. Please note that if a user changes their display name, they will lose access unless you put their new updated name. This is case-sensitive. Either copy their name using VRCX, or from the VRChat Website, this is easier than trying to type it out manually, or if they use special characters.")]
    [SerializeField] private string[] localNames;

    [Tooltip("Use any form of raw text hosting like Pastebin to host allowed users to be allowed access to make announcements (one user per new line). When a user changes their name, they must have their name updated else they won't have access anymore.")]
    [SerializeField] private VRCUrl remoteUrlList;

    [Tooltip("Enable staff checks. Disable for private events or external security. Disabling this will allow anyone to make announcements however, and isn't recommended unless externally protected through other means.")]
    [SerializeField] private bool enableStaffCheck = true;

    [Tooltip("Destroy GUI at start if local player is not staff. This prevents non-staff from even seeing the GUI. If disabled, non-staff will see the GUI but will be denied access if they try to make an announcement. This is useful if you don't plan on parenting the GUI on a tool, like Reimajo's Admin Tool Panel, and don't want non-whitelisted users to see it.")]
    [SerializeField] private bool destroyIfNotStaff = false;

    private string[] staffList; // Combined list of admins, moderators, names in the URL list and local names are stored here
    private bool waitingForRemoteList = false; // used to track if we're waiting for the remote list to load

    [Header("Display Settings")]
    [Tooltip("How long the announcement will stay fully visible.")]
    public float displayTime = 10f;
    [Tooltip("Time taken to fade in and out.")]
    public float fadeDuration = 0.5f;
    [Tooltip("How often to sync the announcement text across the network while visible.")]
    public float syncInterval = 0.5f;
    [Tooltip("If true, a cooldown is applied after an announcement is made before another can be sent again. This prevents spamming the announcement system. This can prevent abuse.")]
    public bool useCooldown = false;

    private float elapsedTime;
    private bool isAnnouncementActive = false;
    private float nextSyncTime = 0f;

    [UdonSynced, FieldChangeCallback(nameof(NotificationMessage))]
    private string notificationMessage;

    // When true, we are waiting for the next PostSerialization result to confirm
    // that the latest notificationMessage was sent successfully. Only then will
    // we broadcast the DisplayAnnouncement network event to ensure clients read
    // the up-to-date synced string instead of an old/blank value.
    private bool awaitingPostSerialization = false;

    void Start()
    {
        LoadStaffList();

        if (remoteUrlList != null && !string.IsNullOrEmpty(remoteUrlList.Get()))
        {
            waitingForRemoteList = true;
            VRCStringDownloader.LoadUrl(remoteUrlList, (IUdonEventReceiver)this);
        }
        else
        {
            FinalizeStartup();
        }

        visibleObject.SetActive(false);
        SetTextAlpha(0);
    }

    private void FinalizeStartup()
    {
        if (enableStaffCheck && destroyIfNotStaff && !IsStaff(Networking.LocalPlayer.displayName))
        {
            if (guiObject == null) // Nothing to destroy, exit early
            {
                Debug.LogError("[AnnouncementSystem] GUI Object is not assigned, so there was nothing to destroy.");
                return;
            }
            else
            {
                Destroy(guiObject);
            }

        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string[] remoteStaff = result.Result.Split('\n');
        if (remoteStaff.Length > 0)
        {
            string[] combined = new string[staffList.Length + remoteStaff.Length];
            staffList.CopyTo(combined, 0);
            remoteStaff.CopyTo(combined, staffList.Length);
            staffList = combined;
        }

        waitingForRemoteList = false;
        FinalizeStartup();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogWarning("[AnnouncementSystem] Failed to load remote staff list: " + result.Error);
        waitingForRemoteList = false;
        FinalizeStartup();
    }

    void Update()
    {
        if (visibleObject.activeSelf && isAnnouncementActive && Time.realtimeSinceStartup >= nextSyncTime)
        {
            nextSyncTime = Time.realtimeSinceStartup + syncInterval;
            RequestSerialization();

            textMesh.text = notificationMessage;
        }
    }

    public void SendAnnouncement()
    {
        if (isAnnouncementActive || (useCooldown && visibleObject.activeSelf)) return;

        if (!enableStaffCheck || IsStaff(Networking.LocalPlayer.displayName))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            string senderName = Networking.LocalPlayer != null ? Networking.LocalPlayer.displayName : "Unknown";
            notificationMessage = CreateAnnouncementText(inputField.text, senderName);
            // Request serialization and wait for OnPostSerialization to confirm
            awaitingPostSerialization = true;
            RequestSerialization();
        }
        else
        {
            DisplayNotStaffMessage();
        }
    }

    public override void OnPostSerialization(SerializationResult result)
    {
        // Only send the DisplayAnnouncement event when we explicitly requested
        // serialization for a new announcement and it succeeded.
        if (awaitingPostSerialization)
        {
            awaitingPostSerialization = false;
            if (result.success)
            {
                // Broadcast to all clients to display the announcement now that
                // the synced string should be available to them.
                SendCustomNetworkEvent(NetworkEventTarget.All, "DisplayAnnouncement");
            }
            else
            {
                Debug.LogWarning("[AnnouncementSystem] Announcement serialization failed. Will not broadcast.");
                // Inform the staff member (owner) locally that serialization failed
                // so they know the announcement wasn't sent.
                if (Networking.IsOwner(gameObject))
                {
                    textMesh.text = "<color=red>Announcement failed to send. Please try again.</color>";
                    ShowObject();
                    FadeInText();
                    // Do not mark as active announcement for others
                    isAnnouncementActive = false;
                }
            }
        }
    }

    public void OnTextInputChanged()
    {
        if (!enableStaffCheck || IsStaff(Networking.LocalPlayer.displayName))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            string senderName = Networking.LocalPlayer != null ? Networking.LocalPlayer.displayName : "Unknown";
            notificationMessage = CreateAnnouncementText(inputField.text, senderName);
            RequestSerialization();
        }
        else
        {
            DisplayNotStaffMessage();
        }
    }

    private void LoadStaffList()
    {
        string[] admins = adminFile ? adminFile.text.Split('\n') : new string[0];
        string[] moderators = moderatorFile ? moderatorFile.text.Split('\n') : new string[0];
        string[] manual = localNames ?? new string[0];

        int total = admins.Length + moderators.Length + manual.Length;
        staffList = new string[total];

        admins.CopyTo(staffList, 0);
        moderators.CopyTo(staffList, admins.Length);
        manual.CopyTo(staffList, admins.Length + moderators.Length);
    }

    private bool IsStaff(string playerName)
    {
        if (staffList == null || playerName == null) return false;

        playerName = playerName.Trim();
        foreach (string staff in staffList)
        {
            if (playerName.Equals(staff.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private string CreateAnnouncementText(string message, string senderName)
    {
        if (string.IsNullOrEmpty(senderName)) senderName = "Unknown";
        if (message == null) message = string.Empty;
        return "<color=green>Announcement by " + senderName + "</color>: " + message;
    }

    public string NotificationMessage
    {
        set
        {
            notificationMessage = value;
            if (Networking.IsOwner(gameObject))
            {
                textMesh.text = notificationMessage;
                PlayNotificationSound();
                ShowObject();
                FadeInText();
                isAnnouncementActive = true;
            }
        }
    }

    public void DisplayAnnouncement()
    {
        textMesh.text = notificationMessage;
        PlayNotificationSound();
        ShowObject();
        FadeInText();
        isAnnouncementActive = true;
    }

    private void DisplayNotStaffMessage()
    {
        textMesh.text = "<color=red>Access Denied. You don't have permission to use this.</color>";
        Debug.LogError("[AnnouncementSystem] Access Denied. You don't have permission to use this.");
#if UNITY_EDITOR
        if (staffList == null || staffList.Length == 0 || string.IsNullOrWhiteSpace(staffList[0]))
        {
            Debug.LogWarning("[AnnouncementSystem] It appears the Staff list is empty. Leave Play Mode, then make sure you have setup names, including yourself correctly. If you don't have a Local Player Name set in ClientSim, add '[1] Local Player' to the name list when running inside Unity Editor.");
        }
#endif
        ShowObject();
        FadeInText();
    }

    private void PlayNotificationSound()
    {
        if (notificationSound != null)
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
        isAnnouncementActive = false;
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