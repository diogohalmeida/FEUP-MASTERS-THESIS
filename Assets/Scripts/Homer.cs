using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;

public class Homer : MonoBehaviour
{
    public Transform rightHand;
    private bool isGrabbing;
    private Vector3 previousHandPosition;
    private Quaternion previousHandRotation;
    private Quaternion previousObjectRotation;

    private float velocityModifierTranslations = 10f;

    private float scalingConstant = 1f;

    private TaskHandler taskHandler;
    // Start is called before the first frame update
    void Start()
    {
        isGrabbing = false;
        taskHandler = GetComponent<TaskHandler>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //If player is holding the HTC Vive controller trigger
        //Debug.Log(SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand));
        if (taskHandler.mode == 0)
        {
            return;
        }
        if (SteamVR_Actions._default.GrabPinch.GetState(SteamVR_Input_Sources.RightHand))
        {
            if (!isGrabbing)
            {
                isGrabbing = true;
                previousHandPosition = rightHand.position;
                previousHandRotation = rightHand.rotation;
                previousObjectRotation = taskHandler.objectToDock.transform.rotation;
            }
            else{
                 //Move the object to the position of the right hand
                Vector3 handDeltaTranslation = rightHand.position - previousHandPosition;
                Quaternion handDeltaRotation = Quaternion.Inverse(previousHandRotation) * rightHand.rotation;

                float velocity = handDeltaTranslation.magnitude / Time.deltaTime;

                Debug.Log(velocity);

                float scalingFactor = velocity / scalingConstant;

                Vector3 newPosition = handDeltaTranslation * (velocityModifierTranslations * Math.Min(scalingFactor, 1.2f)) + taskHandler.objectToDock.transform.position;
                if (newPosition.x > taskHandler.collisionXmin && newPosition.x < taskHandler.collisionXmax && 
                newPosition.y > taskHandler.collisionYmin && newPosition.y < taskHandler.collisionYmax && 
                newPosition.z > taskHandler.collisionZmin && newPosition.z < taskHandler.collisionZmax)
                {
                    taskHandler.objectToDock.transform.position = newPosition;
                }
                taskHandler.objectToDock.transform.rotation = previousObjectRotation * handDeltaRotation;
                
                previousHandPosition = rightHand.position;
                previousHandRotation = rightHand.rotation;
                previousObjectRotation = taskHandler.objectToDock.transform.rotation;
            }
            return;
           
        }
        else{    
            isGrabbing = false;
            return;
        }

    }
}           