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

    public int mode = 0; //0 is idle, 1 is touchMovement and 2 is HOMER

    private float sceneScale;

    // Start is called before the first frame update
    void Start()
    {
        sceneScale = GameObject.Find("Docking Task Scene").transform.localScale.x;
        //Get all children objects
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }     
        dockingPoint = children[currentPairIndex].transform.Find("DockingPoint");
        objectToDock = children[currentPairIndex].transform.Find("ObjectToDock");

        //Initialize collision list with office boundary
        Vector3 officeOffset = GameObject.Find("Office").transform.position;

        collisionXmin = -12.5f*sceneScale + officeOffset.x;
        collisionXmax = 12.5f*sceneScale + officeOffset.x;
        collisionYmin = 0*sceneScale + officeOffset.y;
        collisionYmax =  6.6f*sceneScale + officeOffset.y;
        collisionZmin = -10*sceneScale + officeOffset.z; 
        collisionZmax = 5*sceneScale + officeOffset.z;
    }

    public void nextPair()
    {
        children[currentPairIndex].gameObject.SetActive(false);
        currentPairIndex++;
        if (currentPairIndex < children.Count)
        {
            dockingPoint = children[currentPairIndex].transform.Find("DockingPoint");
            objectToDock = children[currentPairIndex].transform.Find("ObjectToDock");
            children[currentPairIndex].gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("All tasks completed");
        }

        if (currentPairIndex == 2){
            //find object called "Office" and "Glass"
            GameObject office = GameObject.Find("Office");
            GameObject glass = GameObject.Find("Glass");

            //disable glass
            glass.SetActive(false);

            //rotate office 180 degrees and set position to 0,50,-215 (relative to parent), original is 0,50,-205 with 180 degrees rotation
            office.transform.Rotate(0,180,0, Space.Self);
            office.transform.position = new Vector3(0,20,-86);

            //Get camera position
            Vector3 cameraPosition = Camera.main.transform.position;

            //Outer limits -350, 350
            collisionXmin = -350;
            collisionXmax = 350;
            collisionYmin = 0;
            collisionYmax = 100;
            collisionZmin = cameraPosition.z;
            collisionZmax = 350;
                
            
        }
    }
}
