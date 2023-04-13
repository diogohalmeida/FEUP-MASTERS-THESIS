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

    // Update is called once per frame
    void FixedUpdate() 
    {
        Vector3 objectToDockCenter = taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = taskHandler.dockingPoint.GetComponent<MeshCollider>().bounds.center;
        
        //Get distance from camera to dockingPoint
        float distanceCameraToDP = Vector3.Distance(Camera.main.transform.position, dockingPointCenter);


        float distanceObjectToDP = Vector3.Distance(dockingPointCenter, objectToDockCenter);
        float angleX = Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward);
        float angleY = Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up);
        float angleZ = Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right);

        //Check if distance between dockingPoint and objectToDock is less or equal than 2% of distance from camera to dockingPoint and angle between dockingPoint and objectToDock is less or equal than 10 degrees    
        if (distanceObjectToDP <= distanceCameraToDP * distanceFactor && angleX <= maximumAngle && angleY <= maximumAngle && angleZ <= maximumAngle)
        {
            //Change color of objectToDock to green
            taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.green;
            completeCount++;
            if (completeCount == 150)
            {
                taskHandler.nextPair();
                completeCount = 0;
            }
            updateStatusUI(true, completeCount);
        }
        else
        {
            //Change color of objectToDock to red
            //objectToDock.GetComponent<Renderer>().material.color = Color.red;
            
            //Blinking effect
            taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * blinkingSpeed, 1));
            completeCount = 0;
            updateStatusUI(false, completeCount);
        }

        updateDistanceRotationUI(distanceCameraToDP, distanceObjectToDP, angleX, angleY, angleZ);

    }


    void updateDistanceRotationUI(float distanceCameraToDP, float distanceObjectToDP, float angleX, float angleY, float angleZ){
        //Update GUI
        Transform distanceText = GameObject.Find("DistanceText").transform;
        Transform rotationXText = GameObject.Find("RotationXText").transform;
        Transform rotationYText = GameObject.Find("RotationYText").transform;
        Transform rotationZText = GameObject.Find("RotationZText").transform;

        //Change text from distanceText to distance from objectToDock to dockingPoint
        distanceText.GetComponent<TMPro.TextMeshProUGUI>().text = "Target Distance (max " + (distanceCameraToDP * distanceFactor).ToString("F2") + "m): " + distanceObjectToDP.ToString("F2") + "m";

        //Change text from rotationXText to angle between dockingPoint and objectToDock
        rotationXText.GetComponent<TMPro.TextMeshProUGUI>().text = "Offset X (max " + maximumAngle + "°): " + angleX.ToString("F2") + "°";

        //Change text from rotationYText to angle between dockingPoint and objectToDock
        rotationYText.GetComponent<TMPro.TextMeshProUGUI>().text = "Offset Y (max " + maximumAngle + "°): " + angleY.ToString("F2") + "°";

        //Change text from rotationZText to angle between dockingPoint and objectToDock
        rotationZText.GetComponent<TMPro.TextMeshProUGUI>().text = "Offset Z (max " + maximumAngle + "°): " + angleZ.ToString("F2") + "°";
    }

    void updateStatusUI(bool status, int completeCount){
        Transform statusText = GameObject.Find("StatusText").transform;

        if (status)
        {
            statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "Docking Successful";
            statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.green;
        }
        else
        {
            statusText.GetComponent<TMPro.TextMeshProUGUI>().text = "Docking Incomplete";
            statusText.GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
        }
    }
}

