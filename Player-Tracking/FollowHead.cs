using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class FollowHead : UdonSharpBehaviour
{
    private VRCPlayerApi localPlayer;

    [Space(-8)]
    [Header("Follow Head | Script by Kitto Dev")]
    [Space(-8)]
    [Header("Last Updated: 9/4/2025 | Version 1.1")]

    [Header("Settings")]
    [Tooltip("Assign the object you want to follow the player's head. Leave empty to use the script's own Gameobject.")]
    [SerializeField] private GameObject followingObject; // The object that will follow the player's head

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    void Update()
    {
        if (localPlayer == null) return;

        // Get the player's head position and rotation
        Vector3 headPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Quaternion headRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;

        if (followingObject == null)
        {
            // Set the object's position and rotation to match the player's head
            transform.position = headPosition;
            transform.rotation = headRotation;
        }
        else
        {
            followingObject.transform.position = headPosition;
            followingObject.transform.rotation = headRotation;
        }
    }
}
