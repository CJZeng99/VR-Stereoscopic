using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Skybox")]
    public GameObject cube;
    public Material monoSkybox;
    public Material stereoSkybox;

    [Header("Field of View")]
    public GameObject leftFilter;
    public GameObject rightFilter;

    [Header("Tracking Lag")]
    public Text trackingLagDisplay;

    [Header("Rendering Lag")]
    public Text renderingLagDisplay;

    enum SkyboxMode {Cube, Stereo, Mono}
    enum TrackingMode {Both, Position, Orientation, None}

    private SkyboxMode skyboxMode;
    private P2Utils.RenderingMode renderingMode;
    private bool fovHalf;
    private TrackingMode trackingMode;
    private float iod = 0.065f;

    private bool leftIndexPressed = false;
    private bool rightIndexPressed = false;
    private bool leftHandPressed = false;
    private bool rightHandPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        skyboxMode = SkyboxMode.Cube;
        fovHalf = false;
        renderingMode = P2Utils.RenderingMode.Stereo;
        trackingMode = TrackingMode.Both;
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            fovHalf = !fovHalf;
            UpdateFOV();
        }

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            renderingMode++;
            if (renderingMode > P2Utils.RenderingMode.Inverted)
            {
                renderingMode = P2Utils.RenderingMode.Stereo;
            }
            UpdateRenderingMode(); 
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            trackingMode++;
            if (trackingMode > TrackingMode.None)
            {
                trackingMode = TrackingMode.Both;
            }
            UpdateTrackingMode();
        }

        Vector2 raw = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
        {
            iod = 0.065f;
            P2Utils.instance.setIODDistance(iod);
        }
        else if (raw.x != 0f)
        {
            float delta = raw.x * Time.deltaTime * 0.1f;
            iod += delta;
            if (iod > 0.3f)
                iod = 0.3f;
            else if (iod < -0.1f)
                iod = -0.1f;
            P2Utils.instance.setIODDistance(iod);
        }

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) >= 0.7f && !leftIndexPressed)
        {
            P2Utils.instance.targetTrackingLag--;
            if (P2Utils.instance.targetTrackingLag < 0)
                P2Utils.instance.targetTrackingLag = 0;
            trackingLagDisplay.text = string.Format("{0} frames", P2Utils.instance.targetTrackingLag);
            leftIndexPressed = true;
        }
        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) < 0.7f && leftIndexPressed)
        {
            leftIndexPressed = false;
        }

        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) >= 0.7f && !rightIndexPressed)
        {
            P2Utils.instance.targetTrackingLag = (P2Utils.instance.targetTrackingLag + 1) % 30;
            trackingLagDisplay.text = string.Format("{0} frames", P2Utils.instance.targetTrackingLag);
            rightIndexPressed = true;
        }
        else if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) < 0.7f && rightIndexPressed)
        {
            rightIndexPressed = false;
        }

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) >= 0.7f && !leftHandPressed)
        {
            P2Utils.instance.targetRenderingLag--;
            if (P2Utils.instance.targetRenderingLag < 0)
                P2Utils.instance.targetRenderingLag = 0;
            renderingLagDisplay.text = string.Format("{0} frames", P2Utils.instance.targetRenderingLag);
            leftHandPressed = true;
        }
        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) < 0.7f && leftHandPressed)
        {
            leftHandPressed = false;
        }

        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) >= 0.7f && !rightHandPressed)
        {
            P2Utils.instance.targetRenderingLag = (P2Utils.instance.targetRenderingLag + 1);
            renderingLagDisplay.text = string.Format("{0} frames", P2Utils.instance.targetRenderingLag);
            rightHandPressed = true;
        }
        else if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) < 0.7f && rightHandPressed)
        {
            rightHandPressed = false;
        }

    }

    void UpdateSkybox()
    {
        switch(skyboxMode)
        {
            default:
            case SkyboxMode.Cube:
                cube.SetActive(true);
                RenderSettings.skybox = stereoSkybox;
                break;
            case SkyboxMode.Stereo:
                cube.SetActive(false);
                RenderSettings.skybox = stereoSkybox;
                break;
            case SkyboxMode.Mono:
                cube.SetActive(false);
                RenderSettings.skybox = monoSkybox;
                break;
        }
    }

    void UpdateFOV()
    {
        leftFilter.SetActive(fovHalf);
        rightFilter.SetActive(fovHalf);
    }


    void UpdateRenderingMode()
    {
        P2Utils.instance.changeRenderingMode(renderingMode);
    }


    void UpdateTrackingMode()
    {
        switch (trackingMode)
        {
            default:
            case TrackingMode.Both:
                P2Utils.instance.positionTracking = true;
                P2Utils.instance.rotationTracking = true;
                break;
            case TrackingMode.Position:
                P2Utils.instance.positionTracking = true;
                P2Utils.instance.rotationTracking = false;
                break;
            case TrackingMode.Orientation:
                P2Utils.instance.positionTracking = false;
                P2Utils.instance.rotationTracking = true;
                break;
            case TrackingMode.None:
                P2Utils.instance.positionTracking = false;
                P2Utils.instance.rotationTracking = false;
                break;
        }
    }
}
