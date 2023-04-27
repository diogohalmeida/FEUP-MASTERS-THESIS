using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHandler : MonoBehaviour
{

    //List of children objects
    public List<GameObject> children = new List<GameObject>();

    //Current pair of objects being moved
    public int currentPairIndex = 0;

    //Docking point and object to dock
    public Transform dockingPoint;
    public Transform objectToDock;

    public float collisionXmin;
    public float collisionXmax;
    public float collisionYmin;
    public float collisionYmax;
    public float collisionZmin;
    public float collisionZmax;

    // Start is called before the first frame update
    void Start()
    {
        //Get all children objects
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }     
        dockingPoint = children[currentPairIndex].transform.Find("DockingPoint");
        objectToDock = children[currentPairIndex].transform.Find("ObjectToDock");

        //Initialize collision list with office boundary
        Vector3 officeOffset = GameObject.Find("Office").transform.position;

        // collisionXmin = -12.5f + officeOffset.x;
        // collisionXmax = 12.5f + officeOffset.x;
        // collisionYmin = 0 + officeOffset.y;
        // collisionYmax = 6.6f + officeOffset.y;
        // collisionZmin = -5 + officeOffset.z; 
        // collisionZmax = 10f + officeOffset.z;


        //Outer limits -600, 600
        collisionXmin = -600;
        collisionXmax = 600;
        collisionYmin = 0;
        collisionYmax = 500;
        collisionZmin = -600;
        collisionZmax = 600;
    }

    public void nextPair()
    {
        currentPairIndex++;

        if (currentPairIndex < children.Count)
        {
            dockingPoint = children[currentPairIndex].transform.Find("DockingPoint");
            objectToDock = children[currentPairIndex].transform.Find("ObjectToDock");

            //Change collision list values to world boundaries
        }
        else
        {
            Debug.Log("All tasks completed");
        }
    }
}
