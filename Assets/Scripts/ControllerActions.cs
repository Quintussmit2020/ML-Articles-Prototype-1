using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap;

public class ControllerActions : MonoBehaviour
{


    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    public Transform mediaPlayerRoot;
    public MLMediaPlayerBehavior mlMediaPlayer;
    

    private bool isDimmed = false;

    public GameObject screenDimmer;



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


        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
        controllerActions.Bumper.performed += Bumper_performed;
        controllerActions.Menu.performed += Menu_performed;
        controllerActions.TouchpadPosition.performed += TouchpadPositionOnperformed;

    }

    private void Menu_performed(InputAction.CallbackContext obj)
    {
        if (!isDimmed)
        {
            ToggleGlobalDimming(true);
            isDimmed = true;
        }
        else
        {
            ToggleGlobalDimming(false);
            isDimmed = false;
        }
    }

    private void Bumper_performed(InputAction.CallbackContext obj)
    {
        if (mediaPlayerRoot.gameObject.activeSelf)
        {
            if (mlMediaPlayer.IsPlaying)
            {
                mlMediaPlayer.Pause();
            }
            else
            {
                mlMediaPlayer.Play();
            }
        }
    }

    private void TouchpadPositionOnperformed(InputAction.CallbackContext obj)
    {
        var touchPosition = controllerActions.TouchpadPosition.ReadValue<Vector2>();
        var DimmingValue = Mathf.Clamp((touchPosition.y + 1) / (1.8f), 0, 1);
        screenDimmer.GetComponent<MeshRenderer>().material.SetFloat("_DimmingValue", DimmingValue);
        Debug.Log(DimmingValue);
        MLGlobalDimmer.SetValue(DimmingValue);
    }

    private void ToggleGlobalDimming(bool isEnabled)
    {
        MLGlobalDimmer.SetValue(isEnabled ? 1 : 0);
    }
    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Trigger pressed");


    }

    private void Update()
    {
        
    }



    private void OnPermissionGranted(string permission)
    {

    }

   





    private void OnPermissionDenied(string permission)
    {
        Debug.LogError($"Failed to create Planes Subsystem due to missing or denied {MLPermission.SpatialMapping} permission. Please add to manifest. Disabling script.");
        enabled = false;
    }

}
