using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DockingHandler : MonoBehaviour
{
    //Get child object called "DockingPoint" and "ObjectToDock"
    public Transform dockingPoint;
    public Transform objectToDock;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize objectToDock with red color
        objectToDock.GetComponent<Renderer>().material.color = Color.red;
        dockingPoint.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 1.0f, 0.1f);
    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        Vector3 objectToDockCenter = objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = dockingPoint.GetComponent<MeshCollider>().bounds.center;
        
        //Get distance from camera to dockingPoint
        float distance = Vector3.Distance(Camera.main.transform.position, dockingPointCenter);


        //Log the variables below
        // Debug.Log("Distance from camera: " + (distance * 0.01f));
        // Debug.Log("Object distance " + (Vector3.Distance(dockingPointCenter, objectToDockCenter)));

        //Check if distance between dockingPoint and objectToDock is less or equal than 2% of distance from camera to dockingPoint and angle between dockingPoint and objectToDock is less or equal than 12 degrees
        //Log angles
        // Debug.Log("Angle between dockingPoint and objectToDock fwd: " + Vector3.Angle(dockingPoint.forward, objectToDock.forward));
        // Debug.Log("Angle between dockingPoint and objectToDock up: " + Vector3.Angle(dockingPoint.up, objectToDock.up));
        // Debug.Log("Angle between dockingPoint and objectToDock right: " + Vector3.Angle(dockingPoint.right, objectToDock.right));

        
        if (Vector3.Distance(dockingPointCenter, objectToDockCenter) <= distance * 0.02f 
        && Vector3.Angle(dockingPoint.forward, objectToDock.forward) <= 12 && Vector3.Angle(dockingPoint.up, objectToDock.up) <= 12 && Vector3.Angle(dockingPoint.right, objectToDock.right) <= 12)
        {
            //Change color of objectToDock to green
            objectToDock.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            //Change color of objectToDock to red
            objectToDock.GetComponent<Renderer>().material.color = Color.red;
        }

    }
}
