using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class FPSDisplay : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("FPS Counter | Script by Kitto Dev")]
    [Space(-8)]
    [Header("Updated 9/5/2025 | Version 1.21")]

    [Header("Display Settings")]
    [Tooltip("Required. Assign a Text, TextMeshPro, or TextMeshProUGUI component to display the FPS.")]
    public MaskableGraphic fpsText;

    [Header("Update Settings")]
    [Tooltip("How often to update the FPS display in seconds. Set to 0 for continuous updates at cost of performance.")]
    public float updateInterval = 0.5f;

    private float deltaTime;
    private float timeSinceLastUpdate;
    private System.Type displayType;

    private void Start()
    {
        if (fpsText == null)
        {
            Debug.LogError("[FPSDisplay] Text Component is not assigned!");
            return;
        }

        displayType = fpsText.GetType();

        if (displayType != typeof(Text) &&
            displayType != typeof(TextMeshPro) &&
            displayType != typeof(TextMeshProUGUI))
        {
            Debug.LogError("[FPSDisplay] Unsupported text component type! Use Text, TextMeshPro, or TextMeshProUGUI.");
        }
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            float fps = 1.0f / deltaTime;
            string fpsString = string.Format("{0:0.} FPS", fps);

            if (fpsText == null || displayType == null) return;

            if (displayType == typeof(Text))
            {
                ((Text)fpsText).text = fpsString;
            }
            else if (displayType == typeof(TextMeshPro))
            {
                ((TextMeshPro)fpsText).text = fpsString;
            }
            else if (displayType == typeof(TextMeshProUGUI))
            {
                ((TextMeshProUGUI)fpsText).text = fpsString;
            }
        }
    }
}