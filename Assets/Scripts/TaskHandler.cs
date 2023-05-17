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

    //For logging
    public string filePathTouch;
    public string filePathHOMER;
    public string filePathTouchFrames;
    public string filePathHOMERFrames;


    // Start is called before the first frame update
    void Start()
    {
        //Setup training phase and all pairs
        phase = 0;
        finished = false;
        initializeLogFiles();
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

    public void initializeLogFiles(){
        // Set the directory path
        string directoryPath = Application.dataPath + "/UserTestData/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + "/";

        // Create the directory if it doesn't exist
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Set the file path
        filePathTouch = directoryPath + "touch.csv";
        filePathHOMER = directoryPath + "homer.csv";
        filePathTouchFrames = directoryPath + "touch_frames.csv";
        filePathHOMERFrames = directoryPath + "homer_frames.csv";

        // Check if the touch file exists
        if (!File.Exists(filePathTouch))
        {
            // Create a new file and write the column headers
            using (StreamWriter sw = File.CreateText(filePathTouch))
            {
                sw.WriteLine("Task,Time,DistanceMismatch,RotationMismatchX,RotationMismatchY,RotationMismatchZ,TimeSpentIdle,TimeSpentTranslationXZ,TimeSpentTranslationY,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslationXZ,TotalTranslationY,TotalRotationX,TotalRotationY,TotalRotationZ");
            }
        }

        // Check if the homer file exists
        if (!File.Exists(filePathHOMER))
        {
            // Create a new file and write the column headers
            using (StreamWriter sw = File.CreateText(filePathHOMER))
            {
                sw.WriteLine("Task,Time,DistanceMismatch,RotationMismatchX,RotationMismatchY,RotationMismatchZ,TimeSpentIdle,TimeSpentTranslation,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslation,TotalRotationX,TotalRotationY,TotalRotationZ");
            }
        }

        // Check if the touch frames file exists
        if (!File.Exists(filePathTouchFrames))
        {
            // Create a new file and write the column headers
            using (StreamWriter sw = File.CreateText(filePathTouchFrames))
            {
                sw.WriteLine("Task,Timestamp,State,TouchCount,TouchPosition1,TouchPosition2,ObjectPosition,ObjectRotation,DistanceMismatchX,DistanceMismatchY,DistanceMismatchZ,RotationMismatchX,RotationMismatchY,RotationMismatchZ");
            }
        }

        // Check if the homer frames file exists
        if (!File.Exists(filePathHOMERFrames))
        {
            // Create a new file and write the column headers
            using (StreamWriter sw = File.CreateText(filePathHOMERFrames))
            {
                sw.WriteLine("Task,Timestamp,isGrabbing,ControllerPosition,ControllerRotation,ObjectPosition,ObjectRotation,DistanceMismatchX,DistanceMismatchY,DistanceMismatchZ,RotationMismatchX,RotationMismatchY,RotationMismatchZ");
            }
        }
    }
}
