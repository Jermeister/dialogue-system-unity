using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private SingleCameraControls controllableCamera;
    private CameraSettings currentSettings;

    private Dictionary<string, CameraSettings> cameraSettingsDict;

    [SerializeField]
    private List<CameraSettings> allSettings;

    public string defaultSettingsName = "Default";
    
    private void Awake()
    {
        BuildCameraSettingsDictionary();
    }

    private void Start()
    {
        controllableCamera.SetDefaultCameraSettings(cameraSettingsDict[defaultSettingsName]);
    }

    private void BuildCameraSettingsDictionary()
    {
        if (allSettings == null || allSettings.Count == 0)
            return;
        
        cameraSettingsDict = new Dictionary<string, CameraSettings>();
        
        foreach (var settings in allSettings)
        {
            cameraSettingsDict[settings.location] = settings;
        }
    }

    public void NewLocation(string cameraLocation)
    {
        if (currentSettings.location == cameraLocation)
            return;

        if (!cameraSettingsDict.ContainsKey(cameraLocation))
            return;

        currentSettings = cameraSettingsDict[cameraLocation];
        controllableCamera.SetNewCameraSettings(currentSettings);
    }
}

[System.Serializable]
public struct CameraSettings
{
    public string location;
    public Vector3 positionOffset;
    public Vector3 rotation;
    public float fieldOfView;
    [Range(0f,5f)] public float transitionTime;
}
