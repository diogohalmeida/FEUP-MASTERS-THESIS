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
        Vector3 objectSize = Vector3.Scale(dockingPoint.GetComponent<BoxCollider>().size, dockingPoint.transform.localScale);

        //Log the variables below
        // Debug.Log("Position X: " + (dockingPoint.position.x - objectToDock.position.x));
        // Debug.Log("Size X: " + objectSize.x * 0.3f);
        // Debug.Log("Position Y: " + (dockingPoint.position.y - objectToDock.position.y));
        // Debug.Log("Size Y: " + objectSize.y * 0.3f);
        // Debug.Log("Position Z: " + (dockingPoint.position.z - objectToDock.position.z));
        // Debug.Log("Size Z: " + objectSize.z * 0.3f);

        //Check if objectToDock is within 1 unit of dockingPoint
        if (Math.Abs(dockingPoint.position.x - objectToDock.position.x) < objectSize.x * 0.3f &&
            Math.Abs(dockingPoint.position.y - objectToDock.position.y) < objectSize.y * 0.3f &&
            Math.Abs(dockingPoint.position.z - objectToDock.position.z) < objectSize.z * 0.3f)
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
