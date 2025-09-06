// MIT License
// Modified from original DateTime_UI script by Jetdog8808 (2020)
// Original source: https://github.com/jetdog8808/Jetdogs-Prefabs-Udon
// Copyright (c) 2025 Kitto Dev
// See LICENSE file in the Github repository for full license text.

using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI; //needs to be added to interact with unity ui components.
using TMPro; //needed for text mesh pro component support.
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DateTimeDisplay : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Date/Time Display | Script by Kitto Dev, Jetdog")]
    [Space(-8)]
    [Header("Updated 9/5/2025 | Version 1.0")]
    [Space(-8)]
    [Header("Original Script by Jetdog8808 under MIT License")]

    [Header("Format Settings")]

    //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.datetimeformatinfo?view=net-5.0#remarks
    [Tooltip("For Date, add 'MM-dd-yyyy'. For time add 'hh:mm tt'. Full options can be found in the link in the code.")]
    public string format = "hh:mm:ss tt";

    [Tooltip("Some ID's in code comments. Leave blank for local timezone.")]
    public string timeZoneID = string.Empty;
    /*some common timezone ids you can use. 
     * 
     * Pacific Standard Time
     * Mountain Standard Time
     * Central Standard Time
     * Eastern Standard Time
     * GMT Standard Time
     * Central Europe Standard Time
     * Tokyo Standard Time
     * 
     */

    [Header("Display Settings")]
    [Tooltip("Requied. Assign a Text, TextMeshPro, or TextMeshProUGUI component to display the date/time.")]
    public MaskableGraphic display;
    [Tooltip("How often to update the display in seconds. Set to 0 for continuous updates at cost of performance.")] [Range(0f, 60f)]
    public float updateInterval = 60f; // update every minute by default

    private TimeZoneInfo timezone;
    private string lastDisplayText = string.Empty;
    private float nextUpdateTime = 0f;

    void Start()
    {
        if (timeZoneID == string.Empty)
        {
            timezone = TimeZoneInfo.Local;
        }
        else
        {
            timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneID);
        }

        UpdateDisplay(); // initial update
        nextUpdateTime = Time.time + updateInterval;
    }

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateDisplay();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateDisplay()
    {
        if (!display)
        {
            return;
        }

        Type type = display.GetType();
        string tempstring = TimeZoneInfo.ConvertTime(DateTime.Now, timezone).ToString(format);

        if (tempstring == lastDisplayText) return;

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