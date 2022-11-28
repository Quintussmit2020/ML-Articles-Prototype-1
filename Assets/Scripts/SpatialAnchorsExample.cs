using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.MagicLeap;

public class SpatialAnchorsExample : MonoBehaviour
{
    [System.Serializable]
    public class SavedAnchor
    {
        public string SpaceID;
        public string ID;
        public int type; // cube = 0 sphere =1
    }

    private MagicLeapInputs _magicLeapInputs;
    private MagicLeapInputs.ControllerActions _controllerActions;

    private List<SavedAnchor> _savedAnchors = new List<SavedAnchor>();

    private MLAnchors.Request _spatialAnchorRequest; 

    // Start is called before the first frame update
    void Start()
    {
        _magicLeapInputs = new MagicLeapInputs();
        _magicLeapInputs.Enable();

        _controllerActions = new MagicLeapInputs.ControllerActions(_magicLeapInputs);
        _controllerActions.Bumper.started += BumperStarted;
        _controllerActions.Trigger.started += TriggerStarted;

        var result = MLPermissions.CheckPermission(MLPermission.SpatialAnchors);
        if (result.IsOk)
        {
            MLResult mlResult = MLAnchors.GetLocalizationInfo(out MLAnchors.LocalizationInfo info);
#if !UNITY_EDITOR
            if (info.LocalizationStatus == MLAnchors.LocalizationStatus.NotLocalized)
            {
                UnityEngine.XR.MagicLeap.SettingsIntentsLauncher.LaunchSystemARCloudSettings();
            }
#endif
        }

        _spatialAnchorRequest = new MLAnchors.Request();
    }

    void Update()
    {
      MLResult startStatus =  _spatialAnchorRequest.Start(new MLAnchors.Request.Params(Camera.main.transform.position, 100, 0, false));
      
      if (!startStatus.IsOk)
      {
            Debug.LogError("Could not start" + startStatus);
            return;
      }

      MLResult queryStatus = _spatialAnchorRequest.TryGetResult(out MLAnchors.Request.Result result);

      if (!queryStatus.IsOk)
      {
          Debug.LogError("Could not get result " + queryStatus);
      }

      for (int i = 0; i < result.anchors.Length; i++)
      {
          MLAnchors.Anchor anchor = result.anchors[i];
          string anchorID = anchor.Id;

          var savedAnchor = _savedAnchors.Find(x => x.ID == anchorID && x.SpaceID == anchor.SpaceId);
          if (savedAnchor !=null)
          {
              if (savedAnchor.type == 0)
              {
                  GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                  //Render the cube with the default URP shader
                  cube.AddComponent<Renderer>();
                  cube.GetComponent<Renderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                  //Scale the cube so it is a reasonable size
                  cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                  cube.transform.position = anchor.Pose.position;
                  cube.transform.rotation = anchor.Pose.rotation;
              }
              else if (savedAnchor.type == 1)
              {
                  //create sphere 
              }
          }
      }
    }

    private void TriggerStarted(InputAction.CallbackContext obj)
    {
        Pose controllerPose = new Pose(_controllerActions.Position.ReadValue<Vector3>(),
            _controllerActions.Rotation.ReadValue<Quaternion>());

        MLAnchors.Anchor.Create(controllerPose, 300, out MLAnchors.Anchor anchor);
        var result = anchor.Publish();
        if (result.IsOk)
        {
            SavedAnchor savedAnchor = new SavedAnchor();
            savedAnchor.ID = anchor.Id;
            savedAnchor.SpaceID = anchor.SpaceId;
            _savedAnchors.Add(savedAnchor);
        }
    }

    private void BumperStarted(InputAction.CallbackContext obj)
    {
      
    }

}
