using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SingleCameraControls : MonoBehaviour
{
    private Camera thisCamera;
    private GameObject player;
    
    [SerializeField] private CameraSettings currentSettings;
    [SerializeField] private CameraSettings previousSettings;

    private Coroutine applySettingsCoroutine;
    private Vector3 temporaryPositionOffset;

    private bool shouldUpdate = true;

    void Awake()
    {
        previousSettings = currentSettings;
        player = GameObject.FindWithTag("Player");
        thisCamera = GetComponent<Camera>();
    }
    
    void Update()
    {
        if (!shouldUpdate)
        {
            thisCamera.transform.position = new Vector3(player.transform.position.x + temporaryPositionOffset.x, 
                player.transform.position.y + temporaryPositionOffset.y, 
                player.transform.position.z + temporaryPositionOffset.z);
            return;
        }
            
        
        thisCamera.transform.position = new Vector3(player.transform.position.x + currentSettings.positionOffset.x, 
            player.transform.position.y + currentSettings.positionOffset.y, 
            player.transform.position.z + currentSettings.positionOffset.z);
    }

    public void SetNewCameraSettings(CameraSettings newSettings)
    {
        previousSettings = currentSettings;
        currentSettings = newSettings;

        if (applySettingsCoroutine != null)
            StopCoroutine(applySettingsCoroutine);
        
        applySettingsCoroutine = StartCoroutine(ApplyNewSettings());
    }

    public void SetDefaultCameraSettings(CameraSettings defaultSettings)
    {
        currentSettings = defaultSettings;
        previousSettings = defaultSettings;
        
        ApplySettingsNoDelay();
    }

    private void ApplySettingsNoDelay()
    {
        transform.position = new Vector3(player.transform.position.x + currentSettings.positionOffset.x, 
            player.transform.position.y + currentSettings.positionOffset.y, 
            player.transform.position.z + currentSettings.positionOffset.z);

        transform.eulerAngles = currentSettings.rotation;
        thisCamera.fieldOfView = currentSettings.fieldOfView;
        
    }

    IEnumerator ApplyNewSettings()
    {
        shouldUpdate = false;
        var transitionTime = currentSettings.transitionTime;


        for (float time = 0; time < transitionTime; time += Time.deltaTime)
        {
            var part = Mathf.Clamp01(time / transitionTime);
            temporaryPositionOffset = Vector3.Lerp(previousSettings.positionOffset, currentSettings.positionOffset, part);
            thisCamera.transform.eulerAngles = Vector3.Lerp(previousSettings.rotation, currentSettings.rotation, part);
            thisCamera.fieldOfView = previousSettings.fieldOfView + (currentSettings.fieldOfView - previousSettings.fieldOfView) * part;
            yield return new WaitForEndOfFrame();
        }

        shouldUpdate = true;
        yield return null;
    }
}
