using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Utils : MonoBehaviour
{
    public enum RenderingMode { Stereo, Mono, LeftOnly, RightOnly, Inverted };
    public Camera leftEye;
    public Camera rightEye;
    public GameObject leftParent;
    public GameObject rightParent;
    public GameObject handParent;
    public GameObject leftAnchor;
    public GameObject rightAnchor;
    public GameObject handAnchor;
    public static P2Utils instance;

    RenderingMode renderingMode;
    Vector3 leftPosStart;
    Vector3 rightPosStart;
    float iod;

    public bool positionTracking = true;
    public bool rotationTracking = true;

    private int renderingLag = 0;
    public int targetRenderingLag = 0;
    private int trackingLag = 0;
    public int targetTrackingLag = 0;
    private int readIndex = 0;
    private int writeIndex = 0;

    private Vector3[] leftPosBuffer  = new Vector3[30];
    private Vector3[] rightPosBuffer = new Vector3[30];
    private Vector3[] handPosBuffer = new Vector3[30];
    private Quaternion[] leftRotBuffer = new Quaternion[30];
    private Quaternion[] rightRotBuffer = new Quaternion[30];
    private Quaternion[] handRotBuffer = new Quaternion[30];

    private Vector3 leftPos;
    private Vector3 rightPos;
    private Vector3 handPos;
    private Quaternion leftRot;
    private Quaternion rightRot;
    private Quaternion handRot;

    // Start is called before the first frame update
    void Start()
    {
        renderingMode = RenderingMode.Stereo;
        leftPosStart = leftEye.transform.localPosition;
        rightPosStart = rightEye.transform.localPosition;
        iod = 0.065f;
        if (instance == null)
            instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //if (renderingMode == RenderingMode.Mono)
        //{
        //    var lv = leftEye.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
        //    var lp = leftEye.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        //    rightEye.SetStereoViewMatrix(Camera.StereoscopicEye.Right, lv);
        //    rightEye.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, lp);
        //}
        //Debug.Log(string.Format("leftParent: x: {0}, y: {1}, z: {2}", leftPosStart.x, leftPosStart.y, leftPosStart.z));
        //Debug.Log(string.Format("rightParent: x: {0}, y: {1}, z: {2}", rightPosStart.x, rightPosStart.y, rightPosStart.z));
        //Debug.Log(iod);
        //if (iod != 0.065f)
        //{
        //    leftParent.transform.localPosition = leftPosStart + direction * (iod - 0.065f) / 2;
        //    rightParent.transform.localPosition = rightPosStart - direction * (iod - 0.065f) / 2;
        //}

        Debug.Log("Tracking Lag: " + trackingLag);
        Debug.Log("Target Tracking Lag: " + targetTrackingLag);
        Debug.Log("Write Index: " + writeIndex);
        Debug.Log("Read Index: " + readIndex);


        recordTracking();
        if (trackingLag == targetTrackingLag)
        {
            updateTracking();
        }
        else if (trackingLag < targetTrackingLag)
            trackingLag++;
        else if (trackingLag > targetTrackingLag)
        {
            readIndex = (readIndex + 2) % 30;
            trackingLag--;
        }

        if (renderingLag >= targetRenderingLag)
        {
            updateRendering();
            renderingLag = 0;
        }
        else
        {
            renderingLag++;
        }

    }

    public void changeRenderingMode(P2Utils.RenderingMode mode)
    {
        renderingMode = mode;
        switch (renderingMode)
        {
        default:
        case RenderingMode.Stereo:
            Shader.SetGlobalInt("_RenderingMode", 0);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Left;
            rightEye.stereoTargetEye = StereoTargetEyeMask.Right;
            break;
        case RenderingMode.Mono:
            Shader.SetGlobalInt("_RenderingMode", 1);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Both;
            break;
        case RenderingMode.LeftOnly:
            Shader.SetGlobalInt("_RenderingMode", 2);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Left;
            break;
        case RenderingMode.RightOnly:
            Shader.SetGlobalInt("_RenderingMode", 3);
            break;
        case RenderingMode.Inverted:
            Shader.SetGlobalInt("_RenderingMode", 0);
            leftEye.stereoTargetEye = StereoTargetEyeMask.Right;
            rightEye.stereoTargetEye = StereoTargetEyeMask.Left;
            break;
        }
    }

    public void disableTracking(bool enabled)
    {
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(leftEye, enabled);
        UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(rightEye, enabled);
    }

    public void setIODDistance(float distance)
    {
        iod = distance;
        if (iod == 0.065f)
            resetEyeParents();
        else
        {
            leftEye.transform.localPosition = new Vector3(-iod/2f,0f,0f);
            rightEye.transform.localPosition = new Vector3(iod/2f,0f,0f);
        }
    }

    void resetEyeParents()
    {
        leftEye.transform.localPosition = leftPosStart;
        rightEye.transform.localPosition = rightPosStart;
    }

    void recordTracking()
    {
        leftPosBuffer[writeIndex] = leftAnchor.transform.position;
        rightPosBuffer[writeIndex] = rightAnchor.transform.position;
        handPosBuffer[writeIndex] = handAnchor.transform.position;
        leftRotBuffer[writeIndex] = leftAnchor.transform.rotation;
        rightRotBuffer[writeIndex] = rightAnchor.transform.rotation;
        handRotBuffer[writeIndex] = handAnchor.transform.rotation;

        writeIndex = (writeIndex + 1) % 30;
    }

    void updateTracking()
    {
        if (positionTracking)
        {
            leftPos = leftPosBuffer[readIndex];
            rightPos = rightPosBuffer[readIndex];
        }
        handPos = handPosBuffer[readIndex];

        if (rotationTracking)
        {
            leftRot = leftRotBuffer[readIndex];
            rightRot = rightRotBuffer[readIndex];
        }
        handRot = handRotBuffer[readIndex];

        readIndex = (readIndex + 1) % 30;
    }

    void updateRendering()
    {
        leftParent.transform.position = leftPos;  
        rightParent.transform.position = rightPos;
        handParent.transform.position = handPos;  

        leftParent.transform.rotation = leftRot;  
        rightParent.transform.rotation = rightRot;
        handParent.transform.rotation = handRot;  
    }
}
