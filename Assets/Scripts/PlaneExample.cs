using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap;

public class PlaneExample : MonoBehaviour
{
    private ARPlaneManager planeManager;

    [SerializeField, Tooltip("Maximum number of planes to return each query")]
    private uint maxResults = 100;

    [SerializeField, Tooltip("Minimum hole length to treat as a hole in the plane")]
    private float minHoleLength = 0.5f;

    [SerializeField, Tooltip("Minimum plane area to treat as a valid plane")]
    private float minPlaneArea = 0.25f;

    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;


    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    public ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Pose mediaPlayerPose;
    private bool mediaPlayerPoseValid;// = false;
    public GameObject mediaPlayerIndicatopr;

    

    private void Awake()
    {
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
    }

    private void Start()
    {
        planeManager = FindObjectOfType<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("Failed to find ARPlaneManager in scene. Disabling Script");
            enabled = false;
        }
        else
        {
            // disable planeManager until we have successfully requested required permissions
            planeManager.enabled = false;
        }

        MLPermissions.RequestPermission(MLPermission.SpatialMapping, permissionCallbacks);

        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
        controllerActions.Bumper.performed += Trigger_performed;
    }

    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Trigger pressed");
       if(raycastManager.Raycast(new Ray(controllerActions.Position.ReadValue<Vector3>(), controllerActions.Rotation.ReadValue<Quaternion>() * Vector3.forward), 
            hits,TrackableType.PlaneWithinPolygon))
            {
            for (int i = 0; i < hits.Count; i++)
                
            {
                Debug.Log(hits[i].hitType);
            }

            
        }
    }

    private void Update()
    {
        if (planeManager.enabled)
        {
            PlanesSubsystem.Extensions.Query = new PlanesSubsystem.Extensions.PlanesQuery
            {
                Flags = planeManager.currentDetectionMode.ToMLQueryFlags() | PlanesSubsystem.Extensions.MLPlanesQueryFlags.Polygons | PlanesSubsystem.Extensions.MLPlanesQueryFlags.Semantic_Wall,
                BoundsCenter = Camera.main.transform.position,
                BoundsRotation = Camera.main.transform.rotation,
                BoundsExtents = Vector3.one * 20f,
                MaxResults = maxResults,
                //MinHoleLength = minHoleLength,
                MinPlaneArea = minPlaneArea
            };
        }


        GetPlacementIndicator();
        ShowPlacementIndicator();

    }

    private void ShowPlacementIndicator()
    {
        if (mediaPlayerPoseValid)
        {
            mediaPlayerIndicatopr.SetActive(true);
            Debug.Log("Player pose is valid");
            mediaPlayerIndicatopr.transform.SetPositionAndRotation(mediaPlayerPose.position,mediaPlayerPose.rotation);
        }
        else
        {
            mediaPlayerIndicatopr.SetActive(false);
        }
    }

    private void GetPlacementIndicator()
    {
       var hitsCheck = new List<ARRaycastHit>();
    raycastManager.Raycast(new Ray(controllerActions.Position.ReadValue<Vector3>(), controllerActions.Rotation.ReadValue<Quaternion>() * Vector3.forward),
            hits, TrackableType.PlaneWithinPolygon);
        mediaPlayerPoseValid = hitsCheck.Count > 0;
        if (mediaPlayerPoseValid)
        {
            Debug.Log("Pose is valid");
            mediaPlayerPose = hitsCheck[0].pose;
        }
    }

    private void OnPermissionGranted(string permission)
    {
        planeManager.enabled = true;
    }

    private void OnPermissionDenied(string permission)
    {
        Debug.LogError($"Failed to create Planes Subsystem due to missing or denied {MLPermission.SpatialMapping} permission. Please add to manifest. Disabling script.");
        enabled = false;
    }

}
