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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void nextPair()
    {
        currentPairIndex++;
        dockingPoint = children[currentPairIndex].transform.Find("DockingPoint");
        objectToDock = children[currentPairIndex].transform.Find("ObjectToDock");
    }
}
