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
    [Header("Updated 9/5/2025 | Version 1.4")]

    [Header("Start Date")]
    [SerializeField] private int startYear = 2000;
    [SerializeField] private int startMonth = 1;
    [SerializeField] private int startDay = 1;
    [SerializeField] private int startHour = 0;
    [SerializeField] private int startMinute = 0;
    [SerializeField] private int startSecond = 0;

    [Header("Update Settings")]
    [Tooltip("How often to update the display in seconds. Set to 0 for continuous updates at cost of performance.")]
    [SerializeField] private float updateInterval = 1f;

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

    private DateTime startDate;
    private float nextUpdateTime = 0f;
    private string lastDisplayText = "";

    void Start()
    {
        startDate = new DateTime(startYear, startMonth, startDay, startHour, startMinute, startSecond);
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
        if (!display)
        {
            return;
        }

        TimeSpan span = DateTime.Now - startDate;
        DateTime now = DateTime.Now;

        int totalDays = (int)span.TotalDays;
        int totalMonths = (now.Year - startDate.Year) * 12 + now.Month - startDate.Month;
        int totalYears = now.Year - startDate.Year;

        string tempstring = "";

        if (showYears) tempstring += totalYears + "y ";
        if (showMonths) tempstring += (totalMonths % 12) + "mo ";
        if (showDays) tempstring += span.Days + "d ";
        if (showHours) tempstring += span.Hours + "h ";
        if (showMinutes) tempstring += span.Minutes + "m ";
        if (showSeconds) tempstring += span.Seconds + "s";

        tempstring = tempstring.Trim();

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