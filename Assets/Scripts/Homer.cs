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
                        totalRotationX += Math.Abs(taskHandler.convertEuler(handDeltaRotation.eulerAngles.x));
                    }
                    if (handDeltaRotation.eulerAngles.y > 0.5f)
                    {
                        timeSpentRotationY += Time.deltaTime;
                        totalRotationY += Math.Abs(taskHandler.convertEuler(handDeltaRotation.eulerAngles.y));
                    }
                    if (handDeltaRotation.eulerAngles.z > 0.5f)
                    {
                        timeSpentRotationZ += Time.deltaTime;
                        totalRotationZ += Math.Abs(taskHandler.convertEuler(handDeltaRotation.eulerAngles.z));
                    }
                }
                
                previousHandPosition = rightHand.position;
                previousHandRotation = rightHand.rotation;
                previousObjectRotation = taskHandler.objectToDock.transform.rotation;
            }
        }
        else{    
            if (taskHandler.phase == 1){
                timeSpentIdle += Time.deltaTime;
            }
            isGrabbing = false;
        }

        if (taskHandler.phase == 1){
            logHomerDataFrame();
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


    public void logHomerData(bool completed, TimeSpan time, float distanceMismatch, float rotationMismatchX, float rotationMismatchY, float rotationMismatchZ){
        //open file on filePath
        using (StreamWriter sw = File.AppendText(taskHandler.filePathHOMER))
        {
            //write data
            //Task,Time,DistanceMismatch,RotationMismatchX,RotationMismatchY,RotationMismatchZ,TimeSpentIdle,TimeSpentTranslation,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslation,TotalRotationX,TotalRotationY,TotalRotationZ
            string completionTime;
            if (completed){
                completionTime = time.TotalSeconds.ToString("F2");
            }
            else{
                completionTime = "NA";
            }
            sw.WriteLine((taskHandler.currentPairIndex+1)+  "," + completionTime + "," + distanceMismatch.ToString("F2") + "," + 
                rotationMismatchX.ToString("F2") + "," + rotationMismatchY.ToString("F2") + "," + rotationMismatchZ.ToString("F2") + "," + 
                this.timeSpentIdle.ToString("F2") + "," + this.timeSpentTranslation.ToString("F2") + "," + 
                this.timeSpentRotationX.ToString("F2") + "," + this.timeSpentRotationY.ToString("F2") + "," + this.timeSpentRotationZ.ToString("F2") + "," + 
                this.totalTranslation.ToString("F2") + "," + this.totalRotationX.ToString("F2") + "," + this.totalRotationY.ToString("F2") + "," + this.totalRotationZ.ToString("F2"));
        }
    }

    public void logHomerDataFrame(){
        float angleX = Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right);
        float angleY = Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up);
        float angleZ = Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward);

        Vector3 objectToDockCenter = taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = taskHandler.dockingPoint.GetComponent<MeshCollider>().bounds.center;

        float distanceObjectToDPX = Math.Abs(objectToDockCenter.x - dockingPointCenter.x);
        float distanceObjectToDPY = Math.Abs(objectToDockCenter.y - dockingPointCenter.y);
        float distanceObjectToDPZ = Math.Abs(objectToDockCenter.z - dockingPointCenter.z);

        //Task,Timestamp,isGrabbing,ControllerPosition,ControllerRotation,ObjectPosition,ObjectRotation,DistanceMismatchX,DistanceMismatchY,DistanceMismatchZ,RotationMismatchX,RotationMismatchY,RotationMismatchZ
        using (StreamWriter sw = File.AppendText(taskHandler.filePathHOMERFrames))
        {
            sw.WriteLine((taskHandler.currentPairIndex+1) + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "," + isGrabbing + ",(" +
            rightHand.position.x.ToString("F2") + ";" + rightHand.position.y.ToString("F2") + ";" + rightHand.position.z.ToString("F2") + "),(" +
            taskHandler.convertEuler(rightHand.rotation.eulerAngles.x).ToString("F2") + ";" + taskHandler.convertEuler(rightHand.rotation.eulerAngles.y).ToString("F2") + ";" + taskHandler.convertEuler(rightHand.rotation.eulerAngles.z).ToString("F2") + "),(" + 
            taskHandler.objectToDock.transform.position.x.ToString("F2") + ";" + taskHandler.objectToDock.transform.position.y.ToString("F2") + ";" + taskHandler.objectToDock.transform.position.z.ToString("F2") + "),(" +
            taskHandler.convertEuler(taskHandler.objectToDock.transform.rotation.eulerAngles.x).ToString("F2") + ";" + taskHandler.convertEuler(taskHandler.objectToDock.transform.rotation.eulerAngles.y).ToString("F2") + ";" + taskHandler.convertEuler(taskHandler.objectToDock.transform.rotation.eulerAngles.z).ToString("F2") + ")," +
            distanceObjectToDPX.ToString("F2") + "," + distanceObjectToDPY.ToString("F2") + "," + distanceObjectToDPZ.ToString("F2") + "," + 
            angleX.ToString("F2") + "," + angleY.ToString("F2") + "," + angleZ.ToString("F2"));
        }
    }
}           