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
        float distance = Vector3.Distance(Camera.main.transform.position, dockingPointCenter);


        //Log the variables below
        // Debug.Log("Distance from camera: " + (distance * 0.01f));
        // Debug.Log("Object distance " + (Vector3.Distance(dockingPointCenter, objectToDockCenter)));

        //Check if distance between dockingPoint and objectToDock is less or equal than 2% of distance from camera to dockingPoint and angle between dockingPoint and objectToDock is less or equal than 12 degrees    
        if (Vector3.Distance(dockingPointCenter, objectToDockCenter) <= distance * 0.02f 
        && Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward) <= 12 && Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up) <= 12 && Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right) <= 12)
        {
            //Change color of objectToDock to green
            taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.green;
            completeCount++;
            if (completeCount == 150)
            {
                taskHandler.nextPair();
                completeCount = 0;
            }
        }
        else
        {
            //Change color of objectToDock to red
            //objectToDock.GetComponent<Renderer>().material.color = Color.red;
            
            //Blinking effect
            taskHandler.objectToDock.GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * blinkingSpeed, 1));
            completeCount = 0;
        }

    }
}
