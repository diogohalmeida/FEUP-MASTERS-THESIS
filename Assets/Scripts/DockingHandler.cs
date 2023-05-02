using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class DockingHandler : MonoBehaviour
{
    //Get child object called "DockingPoint" and "ObjectToDock"
    private float blinkingSpeed = 1;
    private int completeCount = 0;

    private TaskHandler taskHandler;

    private float distanceFactor = 0.02f;

    private float maximumAngle = 10f;
    

    // Start is called before the first frame update
    void Start()
    {     
        taskHandler = GetComponent<TaskHandler>();
    }

    void Update(){
        if (taskHandler.mode == 0)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                taskHandler.mode = 1;
            }
            return;
        }
    }

    //FixedUpdate is called 50 times per second (can change in Edit -> Project Settings -> Time) 
    void FixedUpdate() 
    {
        if (taskHandler.mode == 0)
        {
            updateDistanceRotationUI(0,0,0);
            updateStatusUI(2, completeCount);
            return;
        }
        Vector3 objectToDockCenter = taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = taskHandler.dockingPoint.GetComponent<MeshCollider>().bounds.center;
        
        //Get distance from camera to dockingPoint
        float distanceCameraToDP = Vector3.Distance(Camera.main.transform.position, dockingPointCenter);


        float distanceObjectToDP = Vector3.Distance(dockingPointCenter, objectToDockCenter);
        //Calculate angle between dockingPoint and objectToDock
        float angle = Quaternion.Angle(taskHandler.dockingPoint.transform.rotation, taskHandler.objectToDock.transform.rotation);

        float angleX = Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward);
        float angleY = Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up);
        float angleZ = Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right);

        //Check if distance between dockingPoint and objectToDock is less or equal than 2% of distance from camera to dockingPoint and angle between dockingPoint and objectToDock is less or equal than 10 degrees    
        if (distanceObjectToDP <= distanceCameraToDP * distanceFactor && angleX <= maximumAngle && angleY <= maximumAngle && angleZ <= maximumAngle)
        {
            //Change color of objectToDock to green
            taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.green;
            completeCount--;
            if (completeCount == 0)
            {
                updateDistanceRotationUI(0,0,0);
                updateStatusUI(2, completeCount);
                taskHandler.nextPair();
                completeCount = 250;
                taskHandler.mode = 0;
            }
            updateStatusUI(1, completeCount);
        }
        else
        {
            //Blinking effect
            taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * blinkingSpeed, 1));
            completeCount = 250;
            updateStatusUI(0, completeCount);
        }

        updateDistanceRotationUI(distanceCameraToDP, distanceObjectToDP, angle);

    }


    void updateDistanceRotationUI(float distanceCameraToDP, float distanceObjectToDP, float angle){
        //Update GUI
        Transform taskText = GameObject.Find("TaskText").transform;
        Transform distanceText = GameObject.Find("DistanceText").transform;
        Transform rotationText = GameObject.Find("RotationText").transform;

        //Change text from taskText to current task
        taskText.GetComponent<TMPro.TextMeshProUGUI>().text = "Task " + (taskHandler.currentPairIndex + 1) + "/" + taskHandler.children.Count;

        //Change text from distanceText to distance from objectToDock to dockingPoint
        distanceText.GetComponent<TMPro.TextMeshProUGUI>().text = "Target Distance (max " + (distanceCameraToDP * distanceFactor).ToString("F2") + "m): " + distanceObjectToDP.ToString("F2") + "m";

        //Change text from rotationXText to angle between dockingPoint and objectToDock
        rotationText.GetComponent<TMPro.TextMeshProUGUI>().text = "Angle Mismatch (max " + maximumAngle + "°): " + angle.ToString("F2") + "°";
    }

    void updateStatusUI(int status, int completeCount){
        Transform statusText = GameObject.Find("StatusText").transform;

        switch(status){ //0 = docking incomplete, 1 = docking successful, 2 = idle
            case 0:
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "DOCKING INCOMPLETE";
                statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
                break;
            case 1:
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "DOCKING SUCCESSFUL: " + completeCount/50 + "s";
                statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.green;
                break;
            case 2:
                statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "PRESS SPACE TO START";
                statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.white;
                break;
        }
    }
}

