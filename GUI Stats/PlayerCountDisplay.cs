using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerCountDisplay : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Player Count Display | Script by Kitto Dev")]
    [Space(-8)]
    [Header("Updated 11/7/2025 | Version 1.0")]

    [Header("Object References")]
    [Tooltip("Assign any MaskableGraphic text component (Text, TextMeshPro, TextMeshProUGUI).")]
    public MaskableGraphic displayTarget;

    private System.Type displayType;

    void Start()
    {
        if (displayTarget != null)
        {
            displayType = displayTarget.GetType();
            UpdatePlayerCount();
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        if (displayTarget == null || displayType == null) return;

        string displayText = $"{VRCPlayerApi.GetPlayerCount()}";

        if (displayType == typeof(Text))
        {
            ((Text)displayTarget).text = displayText;
        }
        else if (displayType == typeof(TextMeshPro))
        {
            ((TextMeshPro)displayTarget).text = displayText;
        }
        else if (displayType == typeof(TextMeshProUGUI))
        {
            ((TextMeshProUGUI)displayTarget).text = displayText;
        }
        else
        {
            Debug.LogWarning($"[PlayerCountDisplay] Unsupported type: {displayType.Name}. Please use Text or TMP components.");
        }
    }
}