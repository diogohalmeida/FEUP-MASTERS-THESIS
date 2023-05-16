using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class TaskHandler : MonoBehaviour
{
    //Current pair of objects being moved
    public int currentPairIndex = 0;

    public List<List<GameObject>> pairs = new List<List<GameObject>>();

    //Docking point and object to dock
    public Transform dockingPoint;
    public Transform objectToDock;

    public Vector3 initialObjectToDockPosition;
    public Quaternion initialObjectToDockRotation;

    public float collisionXmin;
    public float collisionXmax;
    public float collisionYmin;
    public float collisionYmax;
    public float collisionZmin;
    public float collisionZmax;

    public bool moving = false; //false is idle, true is moving
    public int mode = 0; //0 is touch, 1 is HOMER
    public int phase = 0; //0 is training, 1 is test
    public bool finished = false;

    private float sceneScale;

    private string filePath;


    // Start is called before the first frame update
    void Start()
    {
        //Setup training phase and all pairs
        phase = 0;
        finished = false;
        initializeLogFile();
        sceneScale = GameObject.Find("Docking Task Scene").transform.localScale.x;
        
        List<GameObject> trainingPairs = new List<GameObject>();
        List<GameObject> testPairs = new List<GameObject>();
        
        //get all children objects from "Training" and "Test" gameobjects
        foreach (Transform child in GameObject.Find("Training").transform)
        {
            trainingPairs.Add(child.gameObject);
        }
        foreach (Transform child in GameObject.Find("Test").transform)
        {
            testPairs.Add(child.gameObject);
        }

        pairs.Add(trainingPairs);
        pairs.Add(testPairs);

        dockingPoint = pairs[phase][currentPairIndex].transform.Find("DockingPoint");
        objectToDock = pairs[phase][currentPairIndex].transform.Find("ObjectToDock");

        initialObjectToDockPosition = objectToDock.position;
        initialObjectToDockRotation = objectToDock.rotation;

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
        //Reset to initial position and rotation before moving to next pair
        objectToDock.position = initialObjectToDockPosition;
        objectToDock.rotation = initialObjectToDockRotation;
        pairs[phase][currentPairIndex].gameObject.SetActive(false);
        
        currentPairIndex++;
        
        if (currentPairIndex < pairs[phase].Count)
        {
            pairs[phase][currentPairIndex].gameObject.SetActive(true);
            
            dockingPoint = pairs[phase][currentPairIndex].transform.Find("DockingPoint");
            objectToDock = pairs[phase][currentPairIndex].transform.Find("ObjectToDock");
            
            initialObjectToDockPosition = objectToDock.position;
            initialObjectToDockRotation = objectToDock.rotation;
            
        }
        else
        {
            if (phase == 0){
                currentPairIndex = 0;
                pairs[phase][currentPairIndex].gameObject.SetActive(true);
                
                dockingPoint = pairs[phase][currentPairIndex].transform.Find("DockingPoint");
                objectToDock = pairs[phase][currentPairIndex].transform.Find("ObjectToDock");   
                
                initialObjectToDockPosition = objectToDock.position;
                initialObjectToDockRotation = objectToDock.rotation;
            }
            else{
                finished = true;
            }
        }

        if (currentPairIndex == 4){
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


    public void startTest(){
        pairs[phase][currentPairIndex].gameObject.SetActive(false);
        
        phase = 1;
        currentPairIndex = 0;
        pairs[phase][currentPairIndex].gameObject.SetActive(true);
        dockingPoint = pairs[phase][currentPairIndex].transform.Find("DockingPoint");
        objectToDock = pairs[phase][currentPairIndex].transform.Find("ObjectToDock");
        
        initialObjectToDockPosition = objectToDock.position;
        initialObjectToDockRotation = objectToDock.rotation;
    }

    public void initializeLogFile(){
        // Set the directory path
        string directoryPath = Application.dataPath + "/UserTestData/";

        // Create the directory if it doesn't exist
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Set the file path
        filePath = directoryPath + System.DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".csv";

        //log filePath
        Debug.Log(filePath);

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            // Create a new file and write the column headers
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine("Task,Technique,Time,DistanceMismatch,RotationMismatch,TimeSpentTranslating,TimeSpentRotating");
            }
        }
    }

    public void logData(bool completed, TimeSpan time, float distanceMismatch, float rotationMismatch, float timeSpentTranslating, float timeSpentRotating){
        //open file on filePath
        using (StreamWriter sw = File.AppendText(filePath))
        {
            //write data
            if (completed){
                sw.WriteLine((currentPairIndex+1) + "," + mode + "," + time.TotalSeconds + "," + distanceMismatch + "," + rotationMismatch + "," + timeSpentTranslating + "," + timeSpentRotating);
            }
            else{
                sw.WriteLine((currentPairIndex+1) + "," + mode + "," + "NA" + "," + distanceMismatch + "," + rotationMismatch + "," + timeSpentTranslating + "," + timeSpentRotating);
            }
        }
    }
}
