using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TimeSinceDisplay : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Time Since Display | Script by Kitto Dev")]
    [Space(-8)]
    [Header("Updated 9/6/2025 | Version 1.71")]

    [Header("Target Date")]
    [SerializeField] private int targetYear = 2000;
    [SerializeField] private int targetMonth = 1;
    [SerializeField] private int targetDay = 1;
    [SerializeField] private int targetHour = 0;
    [SerializeField] private int targetMinute = 0;
    [SerializeField] private int targetSecond = 0;

    [Header("Update Settings")]
    [Tooltip("How often to update the display in seconds. Set to 0 for continuous updates at cost of performance.")]
    [SerializeField] private float updateInterval = 1f;

    [Header("Display Mode")]
    [Tooltip("If true, counts down to the target date. If false, counts up from it.")]
    [SerializeField] private bool countDown = false;

    [Header("Prefix Settings")]
    [Tooltip("If true, adds a prefix to the output string.")]
    [SerializeField] private bool usePrefix = true;
    [Tooltip("Prefix shown when counting up from a past date.")]
    [SerializeField] private string prefixCountUp = "Time Since: ";
    [Tooltip("Prefix shown when counting down to a future date.")]
    [SerializeField] private string prefixCountDown = "Time Remaining: ";

    [Header("Enable Parts")]
    [SerializeField] private bool showYears = true;
    [SerializeField] private bool showMonths = true;
    [SerializeField] private bool showDays = true;
    [SerializeField] private bool showHours = true;
    [SerializeField] private bool showMinutes = true;
    [SerializeField] private bool showSeconds = true;

    [Header("Text Target")]
    [Tooltip("Required. Assign a text component to update. (TextMeshProUGUI, TextMeshPro, or Text)")]
    [SerializeField] private MaskableGraphic display;

    private DateTime targetDate;
    private float nextUpdateTime = 0f;
    private string lastDisplayText = "";

    void Start()
    {
        targetDate = new DateTime(targetYear, targetMonth, targetDay, targetHour, targetMinute, targetSecond);
        UpdateDisplay(); // Initial update
    }

    void Update()
    {
        if (Time.realtimeSinceStartup >= nextUpdateTime)
        {
            nextUpdateTime = Time.realtimeSinceStartup + updateInterval;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (!display) return;

        DateTime now = DateTime.Now;
        TimeSpan span = countDown ? targetDate - now : now - targetDate;

        if (span.TotalSeconds < 0) span = TimeSpan.Zero;

        int totalMonths = Math.Abs((now.Year - targetDate.Year) * 12 + now.Month - targetDate.Month);
        int totalYears = Math.Abs(now.Year - targetDate.Year);

        string tempstring = "";

        if (showYears) tempstring += totalYears + "y ";
        if (showMonths) tempstring += (totalMonths % 12) + "mo ";
        if (showDays) tempstring += span.Days + "d ";
        if (showHours) tempstring += span.Hours + "h ";
        if (showMinutes) tempstring += span.Minutes + "m ";
        if (showSeconds) tempstring += span.Seconds + "s";

        tempstring = tempstring.Trim();

        if (usePrefix)
        {
            tempstring = (countDown ? prefixCountDown : prefixCountUp) + tempstring;
        }

        if (tempstring == lastDisplayText) return;

        Type type = display.GetType();

        if (type == typeof(Text))
        {
            ((Text)display).text = tempstring;
        }
        else if (type == typeof(TextMeshPro))
        {
            ((TextMeshPro)display).text = tempstring;
        }
        else if (type == typeof(TextMeshProUGUI))
        {
            ((TextMeshProUGUI)display).text = tempstring;
        }

        lastDisplayText = tempstring;
    }
}
