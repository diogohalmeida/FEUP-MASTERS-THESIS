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

    private float scalingConstant = 1f;

    private TaskHandler taskHandler;

    public float timeSpentRotating = 0.0f;
    public float timeSpentTranslating = 0.0f;
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
        if ((!taskHandler.moving || taskHandler.mode == 0) && taskHandler.phase == 1)
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
                Quaternion handDeltaRotation = rightHand.rotation * Quaternion.Inverse(previousHandRotation);
                if (handDeltaTranslation.magnitude > 0.00035f)
                {
                    timeSpentTranslating += Time.deltaTime;
                }

                if (handDeltaRotation != Quaternion.identity)
                {
                    timeSpentRotating += Time.deltaTime;
                }

                Vector3 pivotPoint = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center;

                float velocity = handDeltaTranslation.magnitude / Time.deltaTime;


                float scalingFactorVelocity = velocity / scalingConstant;
                float scalingFactorDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position)/Vector3.Distance(Camera.main.transform.position, rightHand.position);

                Vector3 newPosition = handDeltaTranslation * (Math.Min(scalingFactorVelocity, 1.2f)) * scalingFactorDistance + taskHandler.objectToDock.transform.position;
                if (newPosition.x > taskHandler.collisionXmin && newPosition.x < taskHandler.collisionXmax && 
                newPosition.y > taskHandler.collisionYmin && newPosition.y < taskHandler.collisionYmax && 
                newPosition.z > taskHandler.collisionZmin && newPosition.z < taskHandler.collisionZmax)
                {
                    taskHandler.objectToDock.transform.position = newPosition;
                }
                //taskHandler.objectToDock.transform.rotation = previousObjectRotation * handDeltaRotation;

                RotateAround(taskHandler.objectToDock.transform, pivotPoint, handDeltaRotation);
                
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

    public static void RotateAround(Transform objectTransform, Vector3 pivotPoint, Quaternion rotation)
    {   
        rotation.ToAngleAxis(out var angle, out var axis);

        objectTransform.RotateAround(pivotPoint, axis, angle);
    }

    public void resetTimes(){
        this.timeSpentTranslating = 0;
        this.timeSpentRotating = 0;
    }
}           