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
    DimmingController dimmingController;

    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    public Transform mediaPlayerRoot;
    public MLMediaPlayerBehavior mlMediaPlayer;
    
    private bool isDimmed = false;
    public GameObject screenDimmer;


    private void Start()
    { 
        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
        controllerActions.Bumper.performed += Bumper_performed;
        controllerActions.Menu.performed += Menu_performed;
        controllerActions.TouchpadPosition.performed += TouchpadPositionOnperformed;

        dimmingController = new DimmingController();
        dimmingController.screenDimmer = screenDimmer;
    }

    private void Menu_performed(InputAction.CallbackContext obj)
    {
        isDimmed = !isDimmed;
        //ToggleGlobalDimming(isDimmed);
        dimmingController.ToggleGlobalDimming(isDimmed);
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
        dimmingController.SegmentedDimming(DimmingValue);
        

        //screenDimmer.GetComponent<MeshRenderer>().material.SetFloat("_DimmingValue", DimmingValue);
        //MLGlobalDimmer.SetValue(DimmingValue);
    }

    //private void ToggleGlobalDimming(bool isEnabled)
    //{
    //    MLGlobalDimmer.SetValue(isEnabled ? 1 : 0);
    //}

}
