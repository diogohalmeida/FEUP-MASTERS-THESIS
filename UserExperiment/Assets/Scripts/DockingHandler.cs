using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class DockingHandler : MonoBehaviour
{
    //Get child object called "DockingPoint" and "ObjectToDock"
    private float blinkingSpeed = 1;

    private TaskHandler taskHandler;

    private float distanceFactor = 0.02f;

    private float maximumAngle = 10f;

    private DateTime endTime;
    private int maxTime = 2;

    private DateTime endDockingTime;
    private bool docking = false;

    private TouchMovement touchMovement;
    private Homer homer;

    private float distanceMismatch;

    private float rotationMismatchX;
    private float rotationMismatchY;
    private float rotationMismatchZ;


    // Start is called before the first frame update
    void Start()
    {     
        taskHandler = GetComponent<TaskHandler>();
        touchMovement = GetComponent<TouchMovement>();
        homer = GetComponent<Homer>();
    }

    void Update(){
        if (taskHandler.finished){
            return;
        }
        if (taskHandler.phase == 0){
            if (Input.GetKeyDown(KeyCode.Space)){
                changeColor(Color.white);
                taskHandler.nextPair();
            }
            else if (taskHandler.iterations == 0){
                if (Input.GetKeyDown(KeyCode.Alpha1)){
                    taskHandler.mode = 0;
                    taskHandler.startTest();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2)){
                    taskHandler.mode = 1;
                    taskHandler.startTest();
                }
            }else if (taskHandler.iterations == 1){
                if (Input.GetKeyDown(KeyCode.Return)){
                    if (taskHandler.mode == 0){
                        taskHandler.mode = 1;
                    }
                    else{
                        taskHandler.mode = 0;
                    }
                    taskHandler.startTest();
                }
            }
        }
        else{
            if (!taskHandler.moving){
                if (Input.GetKeyDown(KeyCode.Space)){
                    taskHandler.moving = true;
                    endTime = DateTime.Now.AddMinutes(maxTime);
                }
            }else{
                if (DateTime.Now >= endTime && !docking){
                    logAndReset(false, TimeSpan.Zero);
                    changeColor(Color.white);
                    taskHandler.nextPair();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Return)){
                    logAndReset(false, TimeSpan.Zero);
                    changeColor(Color.white);
                    taskHandler.nextPair();
                    return;
                }
            }
        }
        
    }

    //FixedUpdate is called 50 times per second (can change in Edit -> Project Settings -> Time) 
    void FixedUpdate() 
    {

        if (taskHandler.finished){
            updateFinishedUI();
            return;
        }
        
        if (!taskHandler.moving && taskHandler.phase == 1)
        {
            updateDistanceRotationUI(0,0,0);
            updateTimerUI();
            updateStatusUI(2);
            return;
        }
        Vector3 objectToDockCenter = taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = taskHandler.dockingPoint.GetComponent<MeshCollider>().bounds.center;
        
        //Get distance from camera to dockingPoint
        float distanceCameraToDP = Vector3.Distance(Camera.main.transform.position, dockingPointCenter);


        distanceMismatch = Vector3.Distance(dockingPointCenter, objectToDockCenter);


        //Log mesh size
        //Debug.Log("Mesh size: " + taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.size.ToString("F2") + "m");
        //Debug.Log("Distance to dp: " + distanceCameraToDP.ToString("F2") + "m");
        //Debug.Log("Distance to object: " + Vector3.Distance(Camera.main.transform.position, objectToDockCenter).ToString("F2") + "m");
        Debug.Log("Distance between dp and object: " + distanceMismatch.ToString("F2") + "m");
        Debug.Log("Rotation mismatch: " + rotationMismatchX.ToString("F2") + "°, " + rotationMismatchY.ToString("F2") + "°, " + rotationMismatchZ.ToString("F2") + "°");
        //Calculate angle between dockingPoint and objectToDock
        float angle = Quaternion.Angle(taskHandler.dockingPoint.transform.rotation, taskHandler.objectToDock.transform.rotation);

        rotationMismatchX = Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right);
        rotationMismatchY = Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up);
        rotationMismatchZ = Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward);

        float maximumDistance = distanceCameraToDP * distanceFactor;

        //Check if distance between dockingPoint and objectToDock is less or equal than 2% of distance from camera to dockingPoint and angle between dockingPoint and objectToDock is less or equal than 10 degrees    
        if (distanceMismatch <= maximumDistance && rotationMismatchX <= maximumAngle && rotationMismatchY <= maximumAngle && rotationMismatchZ <= maximumAngle)
        {
            if (!docking)
            {
                endDockingTime = DateTime.Now.AddSeconds(5);
                docking = true;
            }

            //Change color of objectToDock to green
            changeColor(Color.green);
            
            if (DateTime.Now >= endDockingTime) 
            {
                if (taskHandler.phase == 1){
                    TimeSpan timeLeft = endTime.Subtract(DateTime.Now);
                    TimeSpan completionTime = TimeSpan.FromMinutes(maxTime).Subtract(timeLeft);

                    logAndReset(true, completionTime);
                    changeColor(Color.white);
                    taskHandler.nextPair();
                    //Debug.Log("Docking successful!");
                }
                else{
                    changeColor(Color.white);
                    taskHandler.nextPair();
                }
                docking = false;
                
            }
            updateStatusUI(1);
        }
        else
        {
            //Blinking effect
            blinkColor(Color.red);
            updateStatusUI(0);
            docking = false;
        }

        updateDistanceRotationUI(maximumDistance, distanceMismatch, angle);
        updateTimerUI();

    }

    void updateFinishedUI(){
        Transform taskText = GameObject.Find("TaskText").transform;
        Transform distanceText = GameObject.Find("DistanceText").transform;
        Transform rotationText = GameObject.Find("RotationText").transform;
        Transform statusText = GameObject.Find("StatusText").transform;
        taskText.GetComponent<TMPro.TextMeshProUGUI>().text = "Tasks Finished";
        distanceText.GetComponent<TMPro.TextMeshProUGUI>().text = "";
        rotationText.GetComponent<TMPro.TextMeshProUGUI>().text = "";
        statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "THANK YOU FOR PARTICIPATING!";
        statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.green;
    }


    void updateDistanceRotationUI(float maximumDistance, float distanceObjectToDP, float angle){
        //Update GUI
        Transform taskText = GameObject.Find("TaskText").transform;
        Transform distanceText = GameObject.Find("DistanceText").transform;
        Transform rotationText = GameObject.Find("RotationText").transform;

        //Change text from taskText to current task
        if (taskHandler.phase == 0){
            taskText.GetComponent<TMPro.TextMeshProUGUI>().text = "Training Mode";
        }
        else{
            if (taskHandler.mode == 0){
                taskText.GetComponent<TMPro.TextMeshProUGUI>().text = "Task " + (taskHandler.currentPairIndex + 1) + "/" + taskHandler.pairs[taskHandler.phase].Count + " - Touch";
            }
            else if (taskHandler.mode == 1){
                taskText.GetComponent<TMPro.TextMeshProUGUI>().text = "Task " + (taskHandler.currentPairIndex + 1) + "/" + taskHandler.pairs[taskHandler.phase].Count + " - HOMER";

            }
        }
        

        //Change text from distanceText to distance from objectToDock to dockingPoint
        distanceText.GetComponent<TMPro.TextMeshProUGUI>().text = "Target Distance (max " + maximumDistance.ToString("F2") + "m): " + distanceObjectToDP.ToString("F2") + "m";
        //Change text from rotationXText to angle between dockingPoint and objectToDock
        rotationText.GetComponent<TMPro.TextMeshProUGUI>().text = "Angle Mismatch (max " + maximumAngle + "°): " + angle.ToString("F2") + "°";
    }

    void updateStatusUI(int status){
        Transform statusText = GameObject.Find("StatusText").transform;

        switch(status){ //0 = docking incomplete, 1 = docking successful, 2 = idle
            case 0:
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "DOCKING INCOMPLETE";
                statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
                break;
            case 1:
                float secondsLeft = endDockingTime.Subtract(DateTime.Now).Seconds;
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "DOCKING SUCCESSFUL: " + secondsLeft + "s";
                statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.green;
                break;
            case 2:
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "WAITING FOR NEXT TASK";
                statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.white;
                break;
        }
    }


    void changeColor(Color color){
        taskHandler.objectToDock.GetComponent<Renderer>().material.color = color;
        if (taskHandler.objectToDock.transform.childCount > 0){
            foreach (Transform child in taskHandler.objectToDock.transform){
                child.GetComponent<Renderer>().material.color = color;
            }
        }
    }

    void blinkColor(Color color){
        taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * blinkingSpeed, 1));
        if (taskHandler.objectToDock.transform.childCount > 0){
            foreach (Transform child in taskHandler.objectToDock.transform){
                child.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * blinkingSpeed, 1));
            }
        }
    }

    void updateTimerUI(){
        Transform timerText = GameObject.Find("TimerText").transform;
        if (taskHandler.moving){
            //Countdown from endTime         
            float minutesLeft = endTime.Subtract(DateTime.Now).Minutes;
            float secondsLeft = endTime.Subtract(DateTime.Now).Seconds;

            if (secondsLeft > 0){
                timerText.GetComponent<TMPro.TextMeshProUGUI>().text = minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00");
            }
            else{
                timerText.GetComponent<TMPro.TextMeshProUGUI>().text = "00:00";
            }
            
        }
        else{
            timerText.GetComponent<TMPro.TextMeshProUGUI>().text = "00:00";
        }
    }

    private void logAndReset(bool completed, TimeSpan time){
        if (taskHandler.mode == 0){
            touchMovement.logTouchData(completed, time, distanceMismatch, rotationMismatchX, rotationMismatchY, rotationMismatchZ);
            touchMovement.resetLog();
        }
        else{
            homer.logHomerData(completed, time, distanceMismatch, rotationMismatchX, rotationMismatchY, rotationMismatchZ);
            homer.resetLog();
        }
    }
}

