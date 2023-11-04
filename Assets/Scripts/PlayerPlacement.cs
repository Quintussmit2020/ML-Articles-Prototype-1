using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MagicLeap.Core;

public class PlayerPlacement : MonoBehaviour
{
    private ARPlaneManager planeManager;

    [SerializeField, Tooltip("Maximum number of planes to return each query")]
    private uint maxResults = 100;

    

    [SerializeField, Tooltip("Minimum plane area to treat as a valid plane")]
    private float minPlaneArea = 0.25f;

    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    public Transform mediaPlayerRoot;
    public MLMediaPlayerBehavior mlMediaPlayer;
    public GameObject mediaPlayerIndicator;

    public bool isPlacing = true;



    // Start is called before the first frame update
    private void Awake()
    {
        permissionCallbacks.OnPermissionGranted += PermissionCallbacks_OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += PermissionCallbacks_OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += PermissionCallbacks_OnPermissionDenied;

    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= PermissionCallbacks_OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= PermissionCallbacks_OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= PermissionCallbacks_OnPermissionDenied;
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

        mediaPlayerIndicator.SetActive(false);
        mediaPlayerRoot.gameObject.SetActive(false);

        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
        controllerActions.Trigger.performed += Trigger_performed;

    }

    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Trigger pressed");

        if (mediaPlayerIndicator.activeSelf)
        {
            isPlacing = false;
            mediaPlayerRoot.gameObject.SetActive(true);
            mediaPlayerIndicator.SetActive(false);
            mediaPlayerRoot.transform.position = mediaPlayerIndicator.transform.position;
            mediaPlayerRoot.transform.rotation = mediaPlayerIndicator.transform.rotation;
            mlMediaPlayer.Play();

        }

    }


    public void ExitMediaPlayer()
    {
        Debug.Log("Closing player");
        mlMediaPlayer.Pause();
        mediaPlayerRoot.gameObject.SetActive(false);
        mediaPlayerIndicator.SetActive(true);
        isPlacing = true;
    }

    private void Update()
    {
        if (planeManager.enabled)
        {
            
            PlanesSubsystem.Extensions.Query = new PlanesSubsystem.Extensions.PlanesQuery
            {
                Flags = planeManager.requestedDetectionMode.ToMLQueryFlags() | PlanesSubsystem.Extensions.MLPlanesQueryFlags.Polygons | PlanesSubsystem.Extensions.MLPlanesQueryFlags.Semantic_Wall,
                BoundsCenter = Camera.main.transform.position,
                BoundsRotation = Camera.main.transform.rotation,
                BoundsExtents = Vector3.one * 20f,
                MaxResults = maxResults,
                //MinHoleLength = minHoleLength,
                MinPlaneArea = minPlaneArea
            };
        }

        Ray raycastRay = new Ray(controllerActions.Position.ReadValue<Vector3>(), controllerActions.Rotation.ReadValue<Quaternion>() * Vector3.forward);
        if (isPlacing & Physics.Raycast(raycastRay, out RaycastHit hitInfo, 100, LayerMask.GetMask("Planes")))
        {
            //Debug.Log(hitInfo.transform);
            mediaPlayerIndicator.transform.position = hitInfo.point;
            mediaPlayerIndicator.transform.rotation = Quaternion.LookRotation(-hitInfo.normal);
            mediaPlayerIndicator.gameObject.SetActive(true);

        }
    }

    private void PermissionCallbacks_OnPermissionDenied(string permission)
    {
        Debug.LogError($"Failed to create Planes Subsystem due to missing or denied {MLPermission.SpatialMapping} permission. Please add to manifest. Disabling script.");
        planeManager.enabled = false;
    }

    private void PermissionCallbacks_OnPermissionGranted(string permission)
    {
        if(permission == MLPermission.SpatialMapping)
        {
            planeManager.enabled = true;
            Debug.Log("Plane manager is active");
        }
    }

    void DestroyObject()
    {
        Debug.Log("Close button detected");
        Destroy(gameObject); // Destroy the game object this script is attached to
    }
}
