using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDK3.StringLoading;
using TMPro;
using VRC.Udon.Common.Interfaces;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RemoteTextLoader : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Remote Text Loader | Script by Kitto Dev")]
    [Space(-8)]
    [Header("Updated 9/6/2025 | Version 1.3")]

    [Header("URL Settings")]
    [Tooltip("Raw Text URL to pull board text from.")]
    public VRCUrl pastebinUrl;

    [Header("Display Settings")]
    [Tooltip("Required. Assign a text component to display the board text. Supports TextMeshProUGUI, TextMeshPro, or Text.")]
    public MaskableGraphic boardDisplay;

    [Header("Message Settings")]
    [Tooltip("Text shown when unable to load data.")]
    public string errorText = "<color=red>Unable to retrieve data. Either the data no longer exists, a network error occurred, or the link isn't whitelisted by VRChat and requires Untrusted Links to be turned on.</color>";

    [Tooltip("Text shown while loading data.")]
    public string loadingText = "<color=yellow>Loading data...</color>";

    [Header("Refresh Settings")]
    [Tooltip("Retry delay in seconds for failed loads.")]
    public float failedRetryDelay = 60f;

    [Tooltip("How often to refresh the board in seconds. Minimum 60 seconds recommended.")]
    [Range(60f, 18000f)]
    public float boardRefreshTime = 900f;

    [Tooltip("If false, disables auto-refresh after first successful load.")]
    public bool autoRefreshEnabled = true;

    private bool hasLoadedSuccessfully = false;

    void Start()
    {
        LoadPastebinText();
    }

    public void LoadPastebinText()
    {
        SetText(loadingText);
        VRCStringDownloader.LoadUrl(pastebinUrl, (IUdonEventReceiver)this);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        SetText(result.Result);
        hasLoadedSuccessfully = true;

        if (autoRefreshEnabled)
        {
            SendCustomEventDelayedSeconds("LoadPastebinText", boardRefreshTime);
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        SetText(errorText);

        // Retry once if we haven't successfully loaded yet
        if (!hasLoadedSuccessfully)
        {
            SendCustomEventDelayedSeconds("LoadPastebinText", failedRetryDelay);
        }
    }

    private void SetText(string value)
    {
        if (boardDisplay == null) return;

        Type type = boardDisplay.GetType();

        if (type == typeof(Text))
        {
            ((Text)boardDisplay).text = value;
        }
        else if (type == typeof(TextMeshPro))
        {
            ((TextMeshPro)boardDisplay).text = value;
        }
        else if (type == typeof(TextMeshProUGUI))
        {
            ((TextMeshProUGUI)boardDisplay).text = value;
        }
        else
        {
            Debug.LogWarning("[RemoteTextLoader] Unsupported text component type: " + type.Name);
        }
    }
}
