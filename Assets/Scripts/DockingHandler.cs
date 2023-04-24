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

        updateDistanceRotationUI(distanceCameraToDP, distanceObjectToDP, angle);

    }


    void updateDistanceRotationUI(float distanceCameraToDP, float distanceObjectToDP, float angle){
        //Update GUI
        Transform distanceText = GameObject.Find("DistanceText").transform;
        Transform rotationText = GameObject.Find("RotationText").transform;

        //Change text from distanceText to distance from objectToDock to dockingPoint
        distanceText.GetComponent<TMPro.TextMeshProUGUI>().text = "Target Distance (max " + (distanceCameraToDP * distanceFactor).ToString("F2") + "m): " + distanceObjectToDP.ToString("F2") + "m";

        //Change text from rotationXText to angle between dockingPoint and objectToDock
        rotationText.GetComponent<TMPro.TextMeshProUGUI>().text = "Angle Offset (max " + maximumAngle + "°): " + angle.ToString("F2") + "°";
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

