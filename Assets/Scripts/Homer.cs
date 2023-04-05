using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;

public class Homer : MonoBehaviour
{
    public Transform rightHand;
    public Transform referenceFrame;
    private bool isGrabbing;
    private bool isRotating;
    private Vector3 previousHandPosition;
    private Vector3 previousHandRotation;

    private float velocityModifierTranslations = 10f;

    private float scalingConstant = 1f;
    // Start is called before the first frame update
    void Start()
    {
        isGrabbing = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //If player is holding the HTC Vive controller trigger
        //Debug.Log(SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand));
        if (SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand))
        {
            if (!isGrabbing)
            {
                isGrabbing = true;
                previousHandPosition = rightHand.position;
                previousHandRotation = rightHand.rotation.eulerAngles;
            }
            else{
                 //Move the object to the position of the right hand
                Vector3 handDelta = rightHand.position - previousHandPosition;
                Vector3 handDeltaRotation = rightHand.rotation.eulerAngles - previousHandRotation;

                float velocity = handDelta.magnitude / Time.deltaTime;

                Debug.Log(velocity);

                float scalingFactor = velocity / scalingConstant;

                transform.position = handDelta*velocityModifierTranslations*Math.Min(scalingFactor, 1.2f) + transform.position;
                transform.RotateAround(GetComponent<MeshCollider>().bounds.center, referenceFrame.up, handDeltaRotation.y);
                transform.RotateAround(GetComponent<MeshCollider>().bounds.center, referenceFrame.right, handDeltaRotation.x);
                transform.RotateAround(GetComponent<MeshCollider>().bounds.center, referenceFrame.forward, handDeltaRotation.z);
                
                previousHandPosition = rightHand.position;
                previousHandRotation = rightHand.rotation.eulerAngles;
            }
            return;
           
        }
        else{    
            isGrabbing = false;
            return;
        }

    }


    // void FixedUpdate()
    // {

    //     //If player is holding the HTC Vive controller trigger
    //     //Debug.Log(SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand));
    //     if (SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand))
    //     {
    //         if (!isGrabbing)
    //         {
    //             isGrabbing = true;
    //             previousHandPosition = rightHand.position;
    //         }
    //         else{
    //              //Move the object to the position of the right hand
    //             Vector3 handDelta = rightHand.position - previousHandPosition;

    //             transform.position = handDelta*velocityModifierTranslations*scalingConstant + transform.position;
                
    //             previousHandPosition = rightHand.position;
    //         }
    //         return;
           
    //     }
    //     else{    
    //         isGrabbing = false;
    //         return;
    //     }

    //      //If player is holding the HTC Vive controller button
    //     if (SteamVR_Actions._default.GrabGrip.GetState(SteamVR_Input_Sources.RightHand))
    //     {
    //         if (!isRotating)
    //         {
    //             isRotating = true;
    //             previousHandRotation = rightHand.rotation.eulerAngles;
    //         }
    //         else{
    //             Vector3 handDeltaRotation = rightHand.rotation.eulerAngles - previousHandRotation;

    //             transform.RotateAround(GetComponent<MeshCollider>().bounds.center, referenceFrame.up, handDeltaRotation.y);
    //             transform.RotateAround(GetComponent<MeshCollider>().bounds.center, referenceFrame.right, handDeltaRotation.x);
    //             transform.RotateAround(GetComponent<MeshCollider>().bounds.center, referenceFrame.forward, handDeltaRotation.z);

    //             previousHandRotation = rightHand.rotation.eulerAngles;
    //         }
    //         return;
    //     }
    //     else{    
    //         isRotating = false;
    //         return;
    //     }

    // }
  
}
           