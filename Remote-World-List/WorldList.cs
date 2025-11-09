using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class WorldList : UdonSharpBehaviour
{
    [Space(-8)]
    [Header("Remote World List | Prefab by Kitto Dev")]
    [Space(-8)]
    [Header("Updated 11/6/2025 | Version 1.0")]

    [Header("Remote Config")]
    [Tooltip("Raw Pastebin URL (e.g., https://pastebin.com/raw/abc123) to pull portal list from. Format each line as a single world ID (e.g., wrld_abc123)")]
    [SerializeField] private VRCUrl worldListUrl;

    [Header("Portals Per Page")]
    [Tooltip("Assign portals in page order")]
    [SerializeField] private VRC_PortalMarker[] portals;

    private string[] portalLines;
    private int currentPage = 0;
    private int portalsPerPage;

    void Start()
    {
        LoadList();
    }

    private void LoadList()
    {
        portalsPerPage = portals.Length;
        VRCStringDownloader.LoadUrl(worldListUrl, (IUdonEventReceiver)this);
    }

    void OnEnable()
    {
        LoadList();
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        string raw = result.Result;
        portalLines = raw.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        currentPage = 0;
        UpdatePage();
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogError("[WorldList] Error loading World ID list: " + result.Error);
    }

    public void NextPage()
    {
        int maxPages = Mathf.CeilToInt((float)portalLines.Length / portalsPerPage);
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
        int startIndex = currentPage * portalsPerPage;

        for (int i = 0; i < portalsPerPage; i++)
        {
            int index = startIndex + i;
            VRC_PortalMarker portal = portals[i];

            if (index < portalLines.Length)
            {
                string worldId = portalLines[index].Trim();
                portal.roomId = worldId;
                portal.gameObject.SetActive(true);
            }
            else
            {
                portal.gameObject.SetActive(false);
            }
        }
    }
}