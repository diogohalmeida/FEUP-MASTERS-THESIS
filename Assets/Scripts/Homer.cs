using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;
using System.IO;

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

    //For logging
    private float timeSpentIdle = 0.0f;
    private float timeSpentTranslation = 0.0f;
    private float timeSpentRotationX = 0.0f;
    private float timeSpentRotationY = 0.0f;
    private float timeSpentRotationZ = 0.0f;
    private float totalTranslation = 0.0f;
    private float totalRotationX = 0.0f;
    private float totalRotationY = 0.0f;
    private float totalRotationZ = 0.0f;



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
                RotateAround(taskHandler.objectToDock.transform, pivotPoint, handDeltaRotation);

                if (taskHandler.phase == 1){
                    if (handDeltaTranslation.magnitude > 0.00035f)
                    {
                        timeSpentTranslation += Time.deltaTime;
                        totalTranslation += Vector3.Distance(newPosition, taskHandler.objectToDock.transform.position);
                    }

                    if (handDeltaRotation.eulerAngles.x > 0.5f)
                    {
                        timeSpentRotationX += Time.deltaTime;
                        totalRotationX += handDeltaRotation.eulerAngles.x;
                    }
                    if (handDeltaRotation.eulerAngles.y > 0.5f)
                    {
                        timeSpentRotationY += Time.deltaTime;
                        totalRotationY += handDeltaRotation.eulerAngles.y;
                    }
                    if (handDeltaRotation.eulerAngles.z > 0.5f)
                    {
                        timeSpentRotationZ += Time.deltaTime;
                        totalRotationZ += handDeltaRotation.eulerAngles.z;
                    }
                }
                
                previousHandPosition = rightHand.position;
                previousHandRotation = rightHand.rotation;
                previousObjectRotation = taskHandler.objectToDock.transform.rotation;
            }
            return;
           
        }
        else{    
            if (taskHandler.phase == 1){
                timeSpentIdle += Time.deltaTime;
            }
            isGrabbing = false;
            return;
        }

    }

    public static void RotateAround(Transform objectTransform, Vector3 pivotPoint, Quaternion rotation)
    {   
        rotation.ToAngleAxis(out var angle, out var axis);

        objectTransform.RotateAround(pivotPoint, axis, angle);
    }

    public void resetLog(){
        this.timeSpentIdle = 0;
        this.timeSpentTranslation = 0;
        this.timeSpentRotationX = 0;
        this.timeSpentRotationY = 0;
        this.timeSpentRotationZ = 0;
        this.totalTranslation = 0;
        this.totalRotationX = 0;
        this.totalRotationY = 0;
        this.totalRotationZ = 0;
    }


    public void logHomerData(bool completed, TimeSpan time, float distanceMismatch, float rotationMismatch){
        //open file on filePath
        using (StreamWriter sw = File.AppendText(taskHandler.filePathHOMER))
        {
            //write data
            //Task,Time,DistanceMismatch,RotationMismatch,TimeSpentIdle,TimeSpentTranslation,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslation,TotalRotationX,TotalRotationY,TotalRotationZ
            if (completed){
                sw.WriteLine((taskHandler.currentPairIndex+1)+  "," + time.TotalSeconds + "," + distanceMismatch + "," + rotationMismatch + "," + this.timeSpentIdle + "," + this.timeSpentTranslation + "," + this.timeSpentRotationX + "," + this.timeSpentRotationY + "," + this.timeSpentRotationZ + "," + this.totalTranslation + "," + this.totalRotationX + "," + this.totalRotationY + "," + this.totalRotationZ);
            }
            else{
                sw.WriteLine((taskHandler.currentPairIndex+1) + "," + "NA" + "," + distanceMismatch + "," + rotationMismatch + "," + this.timeSpentIdle + "," + this.timeSpentTranslation + "," + this.timeSpentRotationX + "," + this.timeSpentRotationY + "," + this.timeSpentRotationZ + "," + this.totalTranslation + "," + this.totalRotationX + "," + this.totalRotationY + "," + this.totalRotationZ);
            }
        }
    }
}           