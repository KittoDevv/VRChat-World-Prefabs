using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class AvatarShelf : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Remote Avatar List | Prefab by Kitto Dev")]
    [Space(-8)]
    [Header("Updated 9/5/2025 | Version 1.01")]

    [Header("Remote Config")]
    [Tooltip("Raw Pastebin URL (e.g., https://pastebin.com/raw/abc123) to pull avatar list from. Format each line as: 'AvatarID, AvatarName, CreatorName'")]
    [SerializeField] private VRCUrl avatarListUrl;

    [Header("Avatar Pedestals Per Page")]
    [Tooltip("Assign pedestals in page order")]
    [SerializeField] private VRC_AvatarPedestal[] pedestals;

    [Header("TextMeshPro Labels")]
    [Tooltip("Assign TextMeshPro (non-UI) components to each pedestal. Ensure to assign them in the exact same order in the Pedestal array in order to properly label each pedestal.")]
    [SerializeField] private TextMeshPro[] pedestalLabels;

    [Header("Special Formatting")]
    [Tooltip("This formats a creator's name in a special color, useful for highlighting your own creations.")]
    [SerializeField] private string creatorName = "Kitto Dev";

    [Tooltip("Hex color code for creator name label (e.g. #C40DF8)")]
    [SerializeField] private string creatorHexColor = "#C40DF8";

    [Tooltip("If true, all labels will be hidden regardless of creator")]
    [SerializeField] private bool hideLabels = false;

    private string[] avatarLines;
    private int currentPage = 0;
    private int avatarsPerPage;

    void Start()
    {
        LoadList();
    }

    private void LoadList()
    {
        avatarsPerPage = pedestals.Length;
        VRCStringDownloader.LoadUrl(avatarListUrl, (IUdonEventReceiver)this);
    }

    void OnEnable()
    {
        currentPage = 0; // Reset to first page when enabled
        UpdatePage();
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string raw = result.Result;
        avatarLines = raw.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        UpdatePage();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError("[AvatarShelf] Error loading avatar list: " + result.Error);
    }

    public void NextPage()
    {
        int maxPages = Mathf.CeilToInt((float)avatarLines.Length / avatarsPerPage);
        if (currentPage < maxPages - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        int startIndex = currentPage * avatarsPerPage;

        for (int i = 0; i < avatarsPerPage; i++)
        {
            int index = startIndex + i;
            VRC_AvatarPedestal pedestal = pedestals[i];
            TextMeshPro label = (pedestalLabels != null && i < pedestalLabels.Length) ? pedestalLabels[i] : null;

            if (index < avatarLines.Length)
            {
                string[] parts = avatarLines[index].Split(',');
                if (parts.Length >= 3)
                {
                    string id = parts[0].Trim();
                    string name = parts[1].Trim();
                    string creator = parts[2].Trim();

                    pedestal.blueprintId = id;
                    pedestal.gameObject.SetActive(true);

                    if (label != null)
                    {
                        if (hideLabels)
                        {
                            label.text = "";
                        }
                        else if (creator == creatorName)
                        {
                            label.text = $"<color={creatorHexColor}>{name}</color>";
                        }
                        else
                        {
                            label.text = $"{name} by {creator}";
                        }
                    }
                }
                else
                {
                    pedestal.gameObject.SetActive(false);
                    if (label != null) label.text = "";
                }
            }
            else
            {
                pedestal.gameObject.SetActive(false);
                if (label != null) label.text = "";
            }
        }
    }
}