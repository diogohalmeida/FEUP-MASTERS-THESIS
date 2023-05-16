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

    private float timeSpentTranslating;
    private float timeSpentRotating;

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
                taskHandler.nextPair();
            }
            else if (Input.GetKeyDown(KeyCode.Return)){
                taskHandler.startTest();
            }
        }
        else{
            if (!taskHandler.moving){
                if (Input.GetKeyDown(KeyCode.Space)){
                    taskHandler.moving = true;
                    endTime = DateTime.Now.AddMinutes(maxTime);
                }
            }else{
                if (taskHandler.mode == 0){
                    this.timeSpentTranslating = touchMovement.timeSpentTranslating;
                    this.timeSpentRotating = touchMovement.timeSpentRotating;
                }
                else{
                    this.timeSpentTranslating = homer.timeSpentTranslating;
                    this.timeSpentRotating = homer.timeSpentRotating;
                }

                if (DateTime.Now >= endTime && !docking){
                    taskHandler.logData(false, TimeSpan.Zero, 0, 0, this.timeSpentTranslating, this.timeSpentRotating);
                    touchMovement.resetTimes();
                    homer.resetTimes();
                    nextTask();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Return)){
                    taskHandler.logData(false, TimeSpan.Zero, 0, 0, this.timeSpentTranslating, this.timeSpentRotating);
                    touchMovement.resetTimes();
                    homer.resetTimes();
                    nextTask();
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
            updateStatusUI(2);
            return;
        }
        Vector3 objectToDockCenter = taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = taskHandler.dockingPoint.GetComponent<MeshCollider>().bounds.center;
        
        //Get distance from camera to dockingPoint
        float distanceCameraToDP = Vector3.Distance(Camera.main.transform.position, dockingPointCenter);


        float distanceObjectToDP = Vector3.Distance(dockingPointCenter, objectToDockCenter);

        //Debug.Log("Distance to dp: " + distanceCameraToDP.ToString("F2") + "m");
        //Debug.Log("Distance to object: " + Vector3.Distance(Camera.main.transform.position, objectToDockCenter).ToString("F2") + "m");
        //Calculate angle between dockingPoint and objectToDock
        float angle = Quaternion.Angle(taskHandler.dockingPoint.transform.rotation, taskHandler.objectToDock.transform.rotation);

        float angleX = Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward);
        float angleY = Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up);
        float angleZ = Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right);

        float maximumDistance = distanceCameraToDP * distanceFactor;

        //Check if distance between dockingPoint and objectToDock is less or equal than 2% of distance from camera to dockingPoint and angle between dockingPoint and objectToDock is less or equal than 10 degrees    
        if (distanceObjectToDP <= maximumDistance && angleX <= maximumAngle && angleY <= maximumAngle && angleZ <= maximumAngle)
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

                    if (taskHandler.mode == 0){
                        this.timeSpentTranslating = touchMovement.timeSpentTranslating;
                        this.timeSpentRotating = touchMovement.timeSpentRotating;
                    }
                    else{
                        this.timeSpentTranslating = homer.timeSpentTranslating;
                        this.timeSpentRotating = homer.timeSpentRotating;
                    }
                    
                    taskHandler.logData(true, completionTime.Subtract(TimeSpan.FromSeconds(5)), distanceObjectToDP, angle, this.timeSpentTranslating, this.timeSpentRotating);
                    
                    touchMovement.resetTimes();
                    homer.resetTimes();
                    
                    nextTask();
                    //Debug.Log("Docking successful!");
                }
                else{
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

        updateDistanceRotationUI(maximumDistance, distanceObjectToDP, angle);
        updateTimerGUI();

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
            taskText.GetComponent<TMPro.TextMeshProUGUI>().text = "Training Mode - Press SPACE to move another object or ENTER to start the test";
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
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "PRESS SPACE TO START";
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

    void updateTimerGUI(){
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


    void nextTask(){
        updateDistanceRotationUI(0,0,0);
        updateStatusUI(2);
        if (taskHandler.mode == 0){
            taskHandler.mode = 1;
            taskHandler.objectToDock.transform.position = taskHandler.initialObjectToDockPosition;
            taskHandler.objectToDock.transform.rotation = taskHandler.initialObjectToDockRotation;
            taskHandler.moving = false;
            changeColor(Color.white);
        }
        else if (taskHandler.mode == 1){
            taskHandler.mode = 0;
            taskHandler.moving = false;
            taskHandler.nextPair();
            changeColor(Color.white);
        }
    }
}

