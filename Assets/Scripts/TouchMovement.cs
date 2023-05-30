using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class TouchMovement : MonoBehaviour
{
    public Transform referenceFrame;

    private enum State{TranslationXZ, TranslationY, RotationX, RotationY, RotationZ, Idle, Checking};
    //private enum Input{Touch1, Touch2X, Touch2Y, Touch2Circular, }
    private State currentState = State.Idle;

    private int touchCount = 0;
    private Vector2 initialTouch1Position;
    private Vector2 initialTouch2Position;

    public int touch1ID;
    public int touch2ID;
    
    private const int stateCheckIntervalInitial = 3; //Number of ticks to wait while checking for state change - initial value
    private int stateCheckInterval = stateCheckIntervalInitial; //Number of ticks to wait while checking for state change

    private Vector2 previousTouch1Position;
    private Vector2 previousTouch2Position;

    private const int thresholdMovementX = 50; //Maximum movement to be considered an accidental movement in the X axis 
    private const int thresholdMovementY = 50; //Maximum movement to be considered an accidental movement in the Y axis
    private const int thresholdMovementXTranslationY = 25; //Maximum movement to be considered an accidental movement in the X axis
    private const int thresholdMovementStationary = 1; //Maximum movement to be considered an accidental movement while stationary
    private const int necessaryMovement = 15; //Minimum movement to be considered a movement
    private const int necessaryMovementCircleX = 8; //Minimum movement to be considered a movement circle X
    private const int necessaryMovementCircleY = 5; //Minimum movement to be considered a movement circle Y
    private const int necessaryAngle = 8; //Minimum angle to be considered a rotation
    
    private const int thresholdErrorInitial = 5; //Tolerance for state change caused by mistakes - initial value
    private int thresholdError = thresholdErrorInitial; //Tolerance for state change caused by mistakes

    //newPosition = oldPosition + (touchDistance * velocityModifierTranslation * Math.Min(scalingFactorVelocity, 1.2f) * scalingFactorDistance);
    private float velocityModifierTranslations = 0.001f; //Fixed value to decrease translation velocity
    private float velocityModifierTranslationY = 0.001f; //Fixed value to decrease translationY velocity
    private float velocityModifierRotations = 0.25f; //Fixed value to decrease rotation velocity

    private float scalingConstant = 3000.0f;    //Used to calculate scaling factor (maximum velocity before scaling > 1)

    private TaskHandler taskHandler;

    //For indicators
    public GameObject rotationArrowX1Prefab;
    public GameObject rotationArrowX2Prefab;
    public GameObject rotationArrowY1Prefab;
    public GameObject rotationArrowY2Prefab;
    public GameObject rotationArrowZ1Prefab;
    public GameObject rotationArrowZ2Prefab;

    public GameObject translationArrowXPrefab;
    public GameObject translationArrowYPrefab;
    public GameObject translationArrowZPrefab;

    private GameObject rotationArrow1 = null;
    private GameObject rotationArrow2 = null;

    private GameObject translationArrowX1 = null;
    private GameObject translationArrowX2 = null;
    private GameObject translationArrowY1 = null;
    private GameObject translationArrowY2 = null;
    private GameObject translationArrowZ1 = null;
    private GameObject translationArrowZ2 = null;

    private int rotationArrowScale = 5;
    private bool rotationClockwise;

    private float translationArrowScale = 0.3f;


    //For logging
    private float timeSpentIdle = 0.0f;
    private float timeSpentChecking = 0.0f;
    private float timeSpentRotationX = 0.0f;
    private float timeSpentRotationY = 0.0f;
    private float timeSpentRotationZ = 0.0f;
    private float timeSpentTranslationXZ = 0.0f;
    private float timeSpentTranslationY = 0.0f;
    private float totalTranslationXZ = 0.0f;
    private float totalTranslationY = 0.0f;
    private float totalRotationX = 0.0f;
    private float totalRotationY = 0.0f;
    private float totalRotationZ = 0.0f;



    

    // Start is called before the first frame update
    void Start()
    {
        taskHandler = GetComponent<TaskHandler>();
    }

    //FixedUpdate is called 50 times per second (can change in Edit -> Project Settings -> Time)
    void FixedUpdate(){
        if ((!taskHandler.moving || taskHandler.mode == 1) && taskHandler.phase == 1)
        {
            return;
        }
        if (thresholdError == 0){
            handleIndicators();
            currentState = State.Idle;
            thresholdError = thresholdErrorInitial;
        }
        currentState = stateCheck();
        
        if (taskHandler.phase == 1){
            logTouchDataFrame();
        }
        
        //Debug.Log(currentState);    
        //Debug.Log("Error Threshold: " + thresholdError);
        //Debug.Log("Check Interval: " + stateCheckInterval);
    }

    State stateCheck(){
        switch (currentState)
        {
            case State.Idle:
                if (taskHandler.phase == 1){
                    timeSpentIdle += Time.deltaTime;
                }

                if (Input.touchCount == 1){
                    Touch touch = Input.GetTouch(0);
                    touch1ID = touch.fingerId;
                    initialTouch1Position = touch.position;
                    touchCount = Input.touchCount;
                    return State.Checking;
                }
                else if (Input.touchCount >= 2){
                    Touch touch1 = Input.GetTouch(0);
                    touch1ID = touch1.fingerId;
                    Touch touch2 = Input.GetTouch(1);
                    touch2ID = touch2.fingerId;
                    initialTouch1Position = touch1.position;
                    initialTouch2Position = touch2.position;
                    touchCount = Input.touchCount;
                    return State.Checking; 
                }
                else{
                    return State.Idle;
                }
            case State.Checking:
                if (taskHandler.phase == 1){
                    timeSpentChecking += Time.deltaTime;
                }
                if (Input.touchCount != touchCount){
                    thresholdError = thresholdErrorInitial;
                    stateCheckInterval = stateCheckIntervalInitial;
                    return State.Idle;
                }
                if (stateCheckInterval == 0){
                    stateCheckInterval = stateCheckIntervalInitial;
                    if (checkTranslationXZ(initialTouch1Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;

                        Vector3 center = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center;
                        
                        translationArrowX1 = Instantiate(translationArrowXPrefab, center, Quaternion.identity);
                        translationArrowX1.transform.Rotate(90, 0, 0);
                        translationArrowX1.transform.position -= taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents.x * referenceFrame.transform.right;
                        

                        translationArrowX2 = Instantiate(translationArrowXPrefab, center, Quaternion.identity);
                        translationArrowX2.transform.Rotate(90, 180, 0);
                        translationArrowX2.transform.position += taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents.x * referenceFrame.transform.right;
                        
                        translationArrowZ1 = Instantiate(translationArrowZPrefab, center, Quaternion.identity);
                        translationArrowZ1.transform.Rotate(0, -90, 0);
                        translationArrowZ1.transform.position -= taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents.z * referenceFrame.transform.forward;
                        
                        translationArrowZ2 = Instantiate(translationArrowZPrefab, center, Quaternion.identity);
                        translationArrowZ2.transform.Rotate(0, 90, 0);
                        translationArrowZ2.transform.position += taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents.z * referenceFrame.transform.forward;

                        XZTranslation(previousTouch1Position - initialTouch1Position);
                        return State.TranslationXZ;
                    }
                    else if (checkTranslationY(initialTouch1Position, initialTouch2Position)){
                        Vector3 center = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center;
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;

                        translationArrowY1 = Instantiate(translationArrowYPrefab, center, Quaternion.identity);
                        translationArrowY1.transform.Rotate(0, 90, 90);
                        translationArrowY1.transform.position -= taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents.y * referenceFrame.transform.up;
                        
                        translationArrowY2 = Instantiate(translationArrowYPrefab, center, Quaternion.identity);
                        translationArrowY2.transform.Rotate(0, 90, -90);
                        translationArrowY2.transform.position += taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents.y * referenceFrame.transform.up;

                        YTranslation(previousTouch1Position - initialTouch1Position, previousTouch2Position - initialTouch2Position);
                        return State.TranslationY;
                    }
                    else if (checkRotationY(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;
                        Vector2 vector = previousTouch2Position - previousTouch1Position;  
                        //Calculate vector between current touch1 and current touch2 position
                        Vector2 prevVector = initialTouch2Position - initialTouch1Position;

                        float angle = Vector2.Angle(prevVector, vector);

                        //Instantiate RotationArrowX prefab as child of objectToDock and rotate it around the objectToDock
                        Vector3 center1 = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center + new Vector3(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.size.x/2, 0, 0);
                        Vector3 center2 = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center + new Vector3(-taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.size.x/2, 0, 0);
                    
                        rotationArrow1 = Instantiate(rotationArrowY1Prefab, center1, Quaternion.identity);
                        rotationArrow2 = Instantiate(rotationArrowY2Prefab, center2, Quaternion.identity);

                        //Check if the motion is clockwise or counterclockwise (by default the angle is positive so it's clockwise)
                        float cross = (previousTouch1Position.x - previousTouch2Position.x) * (initialTouch1Position.y - initialTouch2Position.y) - (previousTouch1Position.y - previousTouch2Position.y) * (initialTouch1Position.x - initialTouch2Position.x);
                        if (cross < 0){
                            angle = -angle; //Counterclockwise
                            rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, -135);
                            rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, -135);
                            rotationClockwise = false;
                        }
                        else{
                            rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 0);
                            rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 0);
                            rotationClockwise = true;
                        }

                        //YRotation(angle);

                        return State.RotationY;
                    }
                    else if (checkRotationZ(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;
        
                        //Instantiate RotationArrowX prefab as child of objectToDock and rotate it around the objectToDock
                        Vector3 center1 = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center + new Vector3(0, taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.size.y/2, 0);
                        Vector3 center2 = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center + new Vector3(0, -taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.size.y/2, 0);

                        rotationArrow1 = Instantiate(rotationArrowZ1Prefab, center1, Quaternion.identity);
                        rotationArrow2 = Instantiate(rotationArrowZ2Prefab, center2, Quaternion.identity);
                        
                        //Rotate the rotationArrow 90 degrees to make it point in the correct direction
                        rotationArrow1.transform.Rotate(90, 0, 0, Space.World);
                        rotationArrow2.transform.Rotate(90, 0, 0, Space.World);


                        Vector3 touch1Distance = previousTouch1Position - initialTouch1Position;
                        Vector3 touch2Distance = previousTouch2Position - initialTouch2Position;

                       
                        if (touch1Distance.x + touch2Distance.x > 0){
                            rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, -45);
                            rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, -45);
                            rotationClockwise = true;
                        }
                        else{ 
                            rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 45);
                            rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 45);
                            rotationClockwise = false;
                        }

                        //ZRotation(touch1Distance, touch2Distance);

                        return State.RotationZ;
                    }
                    else if (checkRotationX(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;
                        
                        
                        //Instantiate RotationArrowX prefab as child of objectToDock and rotate it around the objectToDock
                        Vector3 center1 = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center + new Vector3(0, 0, taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.size.z/2);
                        Vector3 center2 = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center + new Vector3(0, 0, -taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.size.z/2);

                        rotationArrow1 = Instantiate(rotationArrowX1Prefab, center1, Quaternion.identity);
                        rotationArrow2 = Instantiate(rotationArrowX2Prefab, center2, Quaternion.identity);
                        
                        //Rotate the rotationArrow 90 degrees to make it point in the correct direction
                        rotationArrow1.transform.Rotate(0, 0, 90, Space.World);
                        rotationArrow2.transform.Rotate(0, 0, 90, Space.World);


                        Vector3 touch1Distance = previousTouch1Position - initialTouch1Position;
                        Vector3 touch2Distance = previousTouch2Position - initialTouch2Position;

                        if (touch1Distance.y + touch2Distance.y > 0){
                            rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 90);
                            rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 90);
                            rotationClockwise = true;
                        }
                        else{ 
                            rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 45);
                            rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 45);
                            rotationClockwise = false;
                        }
                        
                        //XRotation(previousTouch1Position - initialTouch1Position, previousTouch2Position - initialTouch2Position);
                        
                        return State.RotationX;
                    }
                    
                    else{
                        return State.Idle;
                    }
                }
                else{
                    stateCheckInterval--;
                    return State.Checking;
                }
            case State.TranslationXZ:
                if (taskHandler.phase == 1){
                    timeSpentTranslationXZ += Time.deltaTime;
                }

                if (checkTranslationXZ(previousTouch1Position)){
                    Touch touch = getTouchByID(touch1ID, 1);

                    Vector2 touchDistance = touch.position - previousTouch1Position;
                    
                    XZTranslation(touchDistance);
                    previousTouch1Position = touch.position;
                    thresholdError = thresholdErrorInitial;
                    return State.TranslationXZ;
                }
                else{
                    if (Input.touchCount == 0){
                        thresholdError = 0;
                    }
                    else{
                        thresholdError--;
                    }
                    return State.TranslationXZ;
                }
            case State.TranslationY:
                if (taskHandler.phase == 1){
                    timeSpentTranslationY += Time.deltaTime;
                }

                if (checkTranslationY(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = getTouchByID(touch1ID, 1);
                    Touch touch2 = getTouchByID(touch2ID, 2);
                    
                    Vector2 touch1Distance = touch1.position - previousTouch1Position;
                    Vector2 touch2Distance = touch2.position - previousTouch2Position;

                    YTranslation(touch1Distance, touch2Distance);
                    previousTouch1Position = touch1.position;
                    previousTouch2Position = touch2.position;
                    thresholdError = thresholdErrorInitial;
                    return State.TranslationY;
                }
                else{
                    //thresholdError--;
                    if (Input.touchCount == 0){
                        thresholdError = 0;
                    }
                    else if (Input.touchCount >= 2){
                        Touch touch1 = getTouchByID(touch1ID, 1);
                        Touch touch2 = getTouchByID(touch2ID, 2);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else{
                        thresholdError--;
                    }
                    return State.TranslationY;
                }
            case State.RotationX:
                if (taskHandler.phase == 1){
                    timeSpentRotationX += Time.deltaTime;
                }

                if (checkRotationX(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = getTouchByID(touch1ID, 1);
                    Touch touch2 = getTouchByID(touch2ID, 2);

                    Vector2 touch1Distance = touch1.position - previousTouch1Position;
                    Vector2 touch2Distance = touch2.position - previousTouch2Position;

                    XRotation(touch1Distance, touch2Distance);
                    previousTouch1Position = touch1.position;
                    previousTouch2Position = touch2.position;
                    thresholdError = thresholdErrorInitial;
                    return State.RotationX;
                }
                else{
                    //thresholdError--;
                    if (Input.touchCount == 0){
                        thresholdError = 0;
                    }
                    else if (Input.touchCount >= 2){
                        Touch touch1 = getTouchByID(touch1ID, 1);
                        Touch touch2 = getTouchByID(touch2ID, 2);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else{
                        thresholdError--;
                    }
                    return State.RotationX;
                }
            case State.RotationY:
                if (taskHandler.phase == 1){
                    timeSpentRotationY += Time.deltaTime;
                }

                if (checkRotationY(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = getTouchByID(touch1ID, 1);
                    Touch touch2 = getTouchByID(touch2ID, 2);
                    //Calculate vector between touch1Position and touch2Position
                    Vector2 prevVector = previousTouch2Position - previousTouch1Position;  
                    //Calculate vector between current touch1 and current touch2 position
                    Vector2 vector = touch2.position - touch1.position;

                    float angle = Vector2.Angle(prevVector, vector);

                    //Check if the motion is clockwise or counterclockwise (by default the angle is positive so it's clockwise)
                    float cross = (touch1.position.x - touch2.position.x) * (previousTouch1Position.y - previousTouch2Position.y) - (touch1.position.y - touch2.position.y) * (previousTouch1Position.x - previousTouch2Position.x);
                    if (cross < 0){
                        angle = -angle; //Counterclockwise
                    }
                    YRotation(angle);
                    previousTouch1Position = touch1.position;
                    previousTouch2Position = touch2.position;
                    thresholdError = thresholdErrorInitial;
                    return State.RotationY;
                }
                else{
                    //thresholdError--;
                    if (Input.touchCount == 0){
                        thresholdError = 0;
                    }
                    else if (Input.touchCount >= 2){
                        Touch touch1 = getTouchByID(touch1ID, 1);
                        Touch touch2 = getTouchByID(touch2ID, 2);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else{
                        thresholdError--;
                    }
                    return State.RotationY;
                }
            case State.RotationZ:
                if (taskHandler.phase == 1){
                    timeSpentRotationZ += Time.deltaTime;
                }

                if (checkRotationZ(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = getTouchByID(touch1ID, 1);
                    Touch touch2 = getTouchByID(touch2ID, 2);

                    Vector2 touch1Distance = touch1.position - previousTouch1Position;
                    Vector2 touch2Distance = touch2.position - previousTouch2Position;

                    ZRotation(touch1Distance, touch2Distance);
                    previousTouch1Position = touch1.position;
                    previousTouch2Position = touch2.position;
                    thresholdError = thresholdErrorInitial;
                    return State.RotationZ;
                }
                else{
                    //thresholdError--;
                    if (Input.touchCount == 0){
                        thresholdError = 0;
                    }
                    else if (Input.touchCount >= 2){
                        Touch touch1 = getTouchByID(touch1ID, 1);
                        Touch touch2 = getTouchByID(touch2ID, 2);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else{
                        thresholdError--;
                    }
                    return State.RotationZ;
                }
            default:
                return currentState;
        }
    }





    bool checkTranslationXZ(Vector2 touch1Position){
        if (Input.touchCount == 1){
            Touch touch = getTouchByID(touch1ID, 1);
            float touchDistance = Vector2.Distance(touch1Position, touch.position);
            if (currentState == State.TranslationXZ){
                return true;
            }
            else{
                if (touchDistance >= necessaryMovement){
                    return true;
                }
            }
        }
        return false;
    }

    bool checkTranslationY(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount >= 2){
            Touch touch1 = getTouchByID(touch1ID, 1);
            Touch touch2 = getTouchByID(touch2ID, 2);
            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);
            float touch1Distance = Vector2.Distance(touch1Position, touch1.position);
            float touch2Distance = Vector2.Distance(touch2Position, touch2.position);
            
            if (currentState == State.TranslationY){
                if ((touch1Distance == 0 && touch2DistanceY >= 0 && touch2DistanceX <= thresholdMovementXTranslationY) || (touch2Distance == 0 && touch1DistanceY >= 0 && touch1DistanceX <= thresholdMovementXTranslationY)){
                    return true;
                }
            }
            else{
                if ((touch1Distance <= thresholdMovementStationary && touch2DistanceY >= necessaryMovement && touch2DistanceX <= thresholdMovementXTranslationY)|| (touch2Distance<= thresholdMovementStationary && touch1DistanceY >= necessaryMovement && touch1DistanceX <= thresholdMovementXTranslationY)){
                    return true;
                }
            }
        }
        return false;
    }

    bool checkRotationX(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount >= 2){
            Touch touch1 = getTouchByID(touch1ID, 1);
            Touch touch2 = getTouchByID(touch2ID, 2);
            float touch1DistanceX = touch1Position.x - touch1.position.x;
            float touch2DistanceX = touch2Position.x - touch2.position.x;
            float touch1DistanceY = touch1Position.y - touch1.position.y;
            float touch2DistanceY = touch2Position.y - touch2.position.y;

            if ((touch1DistanceY > 0 && touch2DistanceY > 0) || (touch1DistanceY < 0 && touch2DistanceY < 0)){
                touch1DistanceX = Math.Abs(touch1DistanceX);
                touch2DistanceX = Math.Abs(touch2DistanceX);
                touch1DistanceY = Math.Abs(touch1DistanceY);
                touch2DistanceY = Math.Abs(touch2DistanceY);


                if (currentState == State.RotationX){
                    if (touch1DistanceX <= thresholdMovementX && touch2DistanceX <= thresholdMovementX && touch1DistanceY > 0 && touch2DistanceY > 0){
                        return true;
                    }
                }
                else{
                    if (touch1DistanceX <= thresholdMovementX && touch2DistanceX <= thresholdMovementX && touch1DistanceY >= necessaryMovement && touch2DistanceY >= necessaryMovement){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool checkRotationY(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount >= 2){
            Touch touch1 = getTouchByID(touch1ID, 1);
            Touch touch2 = getTouchByID(touch2ID, 2);
            //Calculate vector between touch1Position and touch2Position
            Vector2 prevVector = touch2Position - touch1Position;  
            //Calculate vector between current touch1 and current touch2 position
            Vector2 vector = touch2.position - touch1.position;

            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);
            float touch1Distance = Vector2.Distance(touch1Position, touch1.position);
            float touch2Distance = Vector2.Distance(touch2Position, touch2.position);
            float angle = Vector2.Angle(prevVector, vector);

            //Debug.Log(angle);
            if (currentState == State.RotationY){
                if (Math.Abs(angle) >= 0){
                    return true;
                }
            }
            else{
                if (Math.Abs(angle) >= necessaryAngle && ((touch1DistanceX >= necessaryMovementCircleX || touch2DistanceX >= necessaryMovementCircleX) && (touch1DistanceY >= necessaryMovementCircleY || touch2DistanceY >= necessaryMovementCircleY))){
                    return true;
                }      
            }
            
        }
        return false;
    }

    bool checkRotationZ(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount >= 2){
            Touch touch1 = getTouchByID(touch1ID, 1);
            Touch touch2 = getTouchByID(touch2ID, 2);
            float touch1DistanceX = touch1Position.x - touch1.position.x;
            float touch2DistanceX = touch2Position.x - touch2.position.x;
            float touch1DistanceY = touch1Position.y - touch1.position.y;
            float touch2DistanceY = touch2Position.y - touch2.position.y;

            if ((touch1DistanceX > 0 && touch2DistanceX > 0) || (touch1DistanceX < 0 && touch2DistanceX < 0)){
                touch1DistanceX = Math.Abs(touch1DistanceX);
                touch2DistanceX = Math.Abs(touch2DistanceX);
                touch1DistanceY = Math.Abs(touch1DistanceY);
                touch2DistanceY = Math.Abs(touch2DistanceY);
        
                if (currentState == State.RotationZ){
                    if (touch1DistanceX > 0 && touch2DistanceX > 0 && touch1DistanceY <= thresholdMovementY && touch2DistanceY <= thresholdMovementY){
                        return true;
                    }
                }
                else{
                    if (touch1DistanceX >= necessaryMovement && touch2DistanceX >= necessaryMovement && touch1DistanceY <= thresholdMovementY && touch2DistanceY <= thresholdMovementY){
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void XZTranslation(Vector2 touchDistance){
        float velocity = touchDistance.magnitude / Time.deltaTime;

        //Prevent teleports from lifting the finger and moving it back
        if (velocity > 10000){
            return;
        }

        float scalingFactorDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position);

        float scalingFactorVelocity = velocity / scalingConstant;

        Vector3 newPositionX = taskHandler.objectToDock.transform.position + (referenceFrame.right * touchDistance.x * velocityModifierTranslations * Math.Min(scalingFactorVelocity, 1.2f) * scalingFactorDistance);
        Vector3 newPositionZ = taskHandler.objectToDock.transform.position + (referenceFrame.forward * touchDistance.y * velocityModifierTranslations *  Math.Min(scalingFactorVelocity, 1.2f)* scalingFactorDistance);
        Vector3 newPosition = newPositionX + (newPositionZ - taskHandler.objectToDock.transform.position);

        if (taskHandler.phase == 1){
            totalTranslationXZ += Vector3.Distance(taskHandler.objectToDock.transform.position, newPosition);
        }

        //Relative to frame
        if ((newPositionX.x > taskHandler.collisionXmin && newPositionX.x < taskHandler.collisionXmax)
            && (newPositionZ.z > taskHandler.collisionZmin && newPositionZ.z < taskHandler.collisionZmax)){
            taskHandler.objectToDock.transform.position = newPosition;
        }  
    }


    void YTranslation(Vector2 touch1Distance, Vector2 touch2Distance){
        Vector3 newPositionY;
        if (Math.Abs(touch1Distance.y) > Math.Abs(touch2Distance.y)){
            float velocity = Math.Abs(touch1Distance.y) / Time.deltaTime;
            float scalingFactorVelocity = velocity / scalingConstant;
            
            float scalingFactorDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position);
            
            newPositionY = taskHandler.objectToDock.transform.position + (referenceFrame.up * touch1Distance.y * velocityModifierTranslationY * Math.Min(scalingFactorVelocity, 1.2f)) * scalingFactorDistance;
        }
        else{
            float velocity = Math.Abs(touch2Distance.y) / Time.deltaTime;
            float scalingFactorVelocity = velocity / scalingConstant;
            
            float scalingFactorDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position);
     
            newPositionY = taskHandler.objectToDock.transform.position + (referenceFrame.up * touch2Distance.y * velocityModifierTranslationY * Math.Min(scalingFactorVelocity, 1.2f)) * scalingFactorDistance;
        }

        if (taskHandler.phase == 1){
            totalTranslationY += Vector3.Distance(taskHandler.objectToDock.transform.position, newPositionY);
        }
        
        if (newPositionY.y > taskHandler.collisionYmin && newPositionY.y < taskHandler.collisionYmax){
            taskHandler.objectToDock.transform.position = newPositionY;
        }
        
    }


    void XRotation(Vector2 touch1Distance, Vector2 touch2Distance){
        float arrowScalingDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position)*0.1f;
        float scalingFactor = Math.Max(rotationArrowScale, rotationArrowScale * arrowScalingDistance);
        
        if (touch1Distance.y + touch2Distance.y < 0){
            if (rotationClockwise){
                rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, -45);
                rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, -45);
                rotationClockwise = false;
            }
            //mirror rotationArrow
            rotationArrow1.transform.localScale = new Vector3(scalingFactor, scalingFactor, -scalingFactor);
            rotationArrow2.transform.localScale = new Vector3(scalingFactor, scalingFactor, -scalingFactor);
        }
        else if (touch1Distance.y + touch2Distance.y > 0){

            if (!rotationClockwise){
                rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 45);
                rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 45);
                rotationClockwise = true;
            }
            //mirror rotationArrow
            rotationArrow1.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
            rotationArrow2.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
        }

        float angle = (touch1Distance.y + touch2Distance.y)/2 * velocityModifierRotations;

        if (taskHandler.phase == 1){
            totalRotationX += Math.Abs(angle);
        }
        
        taskHandler.objectToDock.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.right, angle);  
        
        rotationArrow1.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.right, (touch1Distance.y + touch2Distance.y)/2 * velocityModifierRotations);
        rotationArrow2.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.right, (touch1Distance.y + touch2Distance.y)/2 * velocityModifierRotations);

    }

    void YRotation(float angle){
        float arrowScalingDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position)*0.1f;
        float scalingFactor = Math.Max(rotationArrowScale, rotationArrowScale * arrowScalingDistance);

        if (angle > 0){
            if (!rotationClockwise){
                rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 135);
                rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 135);
                rotationClockwise = true;
            }
            //mirror rotationArrow
            rotationArrow1.transform.localScale = new Vector3(-scalingFactor, scalingFactor, scalingFactor);
            rotationArrow2.transform.localScale = new Vector3(-scalingFactor, scalingFactor, scalingFactor);        
        }
        else if (angle < 0){
            if (rotationClockwise){
                rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, -135);
                rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, -135);
                rotationClockwise = false;
            }
            rotationArrow1.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
            rotationArrow2.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
        }

        if (taskHandler.phase == 1){
            totalRotationY += Math.Abs(angle);
        }

        taskHandler.objectToDock.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.up, angle);

        rotationArrow1.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.up, angle);
        rotationArrow2.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.up, angle);

    }


    void ZRotation(Vector2 touch1Distance, Vector2 touch2Distance){
        float arrowScalingDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position)*0.1f;
        float scalingFactor = Math.Max(rotationArrowScale, rotationArrowScale * arrowScalingDistance);
        
        if (touch1Distance.x + touch2Distance.x < 0){
            
            if (rotationClockwise){
                rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, 90);
                rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, 90);
                rotationClockwise = false;
            }
            //mirror rotationArrow
            rotationArrow1.transform.localScale = new Vector3(-scalingFactor, scalingFactor, scalingFactor);
            rotationArrow2.transform.localScale = new Vector3(-scalingFactor, scalingFactor,scalingFactor);
        }
        else if (touch1Distance.x + touch2Distance.x > 0){

            if (!rotationClockwise){
                rotationArrow1.transform.RotateAround(rotationArrow1.transform.position, rotationArrow1.transform.up, -90);
                rotationArrow2.transform.RotateAround(rotationArrow2.transform.position, rotationArrow2.transform.up, -90);
                rotationClockwise = true;
            }
            //mirror rotationArrow
            rotationArrow1.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
            rotationArrow2.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
        }

        float angle = -(touch1Distance.x + touch2Distance.x)/2 * velocityModifierRotations;

        if (taskHandler.phase == 1){
            totalRotationZ += Math.Abs(angle);
        }

        taskHandler.objectToDock.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.forward, angle);

        rotationArrow1.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.forward, -(touch1Distance.x + touch2Distance.x)/2 * velocityModifierRotations);
        rotationArrow2.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.forward, -(touch1Distance.x + touch2Distance.x)/2 * velocityModifierRotations);

    }


    Touch getTouchByID(int id, int touchNumber){
        for (int i = 0; i < Input.touchCount; i++){
            if (Input.GetTouch(i).fingerId == id){
                return Input.GetTouch(i);
            }
        }

        //if touch is not found, return the first touch 
        if (touchNumber == 1){
            touch1ID = Input.GetTouch(0).fingerId;
        }
        else{
            touch2ID = Input.GetTouch(0).fingerId;
        }
        
        return Input.GetTouch(0);
    }

    private void handleIndicators(){
        if (rotationArrow1 != null && rotationArrow2 != null){
                Destroy(rotationArrow1);
                Destroy(rotationArrow2);
        }
        if (translationArrowX1 != null && translationArrowX2 != null && translationArrowZ1 != null && translationArrowZ2 != null){
            Destroy(translationArrowX1);
            Destroy(translationArrowX2);
            Destroy(translationArrowZ1);
            Destroy(translationArrowZ2);
        }
        if (translationArrowY1 != null && translationArrowY2 != null){
            Destroy(translationArrowY1);
            Destroy(translationArrowY2);
        }
    }


    void LateUpdate(){

        //Translation arrow logic must be updated here since mesh collider center and bounds are only updated in LateUpdate
        if (translationArrowX1 != null && translationArrowX2 != null && translationArrowZ1 != null && translationArrowZ2 != null){
            Vector3 center = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center;
            Vector3 extents = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents;

            float arrowScalingDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position)*0.1f;
            float scalingFactorArrow = Math.Max(translationArrowScale, translationArrowScale * arrowScalingDistance);

            translationArrowX1.transform.position = center + (-extents.x-scalingFactorArrow*0.5f) * referenceFrame.transform.right;
            translationArrowX2.transform.position = center + (extents.x+scalingFactorArrow*0.5f) * referenceFrame.transform.right;
            translationArrowZ1.transform.position = center + (-extents.z-scalingFactorArrow*0.5f) * referenceFrame.transform.forward;
            translationArrowZ2.transform.position = center + (extents.z+scalingFactorArrow*0.5f) * referenceFrame.transform.forward;

            translationArrowX1.transform.localScale = new Vector3(scalingFactorArrow, scalingFactorArrow, scalingFactorArrow);
            translationArrowX2.transform.localScale = new Vector3(scalingFactorArrow, scalingFactorArrow, scalingFactorArrow);
            translationArrowZ1.transform.localScale = new Vector3(scalingFactorArrow, scalingFactorArrow, scalingFactorArrow);
            translationArrowZ2.transform.localScale = new Vector3(scalingFactorArrow, scalingFactorArrow, scalingFactorArrow);
        }

        if (translationArrowY1 != null && translationArrowY2 != null){
            Vector3 center = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center;
            Vector3 extents = taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.extents;

            float arrowScalingDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position)*0.1f;
            float scalingFactorArrow = Math.Max(translationArrowScale, translationArrowScale * arrowScalingDistance);

            translationArrowY1.transform.position = center + (-extents.y-scalingFactorArrow*0.5f) * referenceFrame.transform.up;
            translationArrowY2.transform.position = center + (extents.y+scalingFactorArrow*0.5f) * referenceFrame.transform.up;

            translationArrowY1.transform.localScale = new Vector3(scalingFactorArrow, scalingFactorArrow, scalingFactorArrow);
            translationArrowY2.transform.localScale = new Vector3(scalingFactorArrow, scalingFactorArrow, scalingFactorArrow);
        }
    }

    public void resetLog(){
        this.timeSpentIdle = 0;
        this.timeSpentChecking = 0;
        this.timeSpentTranslationXZ = 0;
        this.timeSpentTranslationY = 0;
        this.timeSpentRotationX = 0;
        this.timeSpentRotationY = 0;
        this.timeSpentRotationZ = 0;
        this.totalTranslationXZ = 0;
        this.totalTranslationY = 0;
        this.totalRotationX = 0;
        this.totalRotationY = 0;
        this.totalRotationZ = 0;
    }


    public void logTouchData(bool completed, TimeSpan time, float distanceMismatch, float rotationMismatchX, float rotationMismatchY, float rotationMismatchZ){
        //open file on filePath
        using (StreamWriter sw = File.AppendText(taskHandler.filePathTouch))
        {
            //write data
            //Task,Time,DistanceMismatch,RotationMismatchX,RotationMismatchY,RotationMismatchZ,TimeSpentIdle,TimeSpentTranslationXZ,TimeSpentTranslationY,TimeSpentRotationX,TimeSpentRotationY,TimeSpentRotationZ,TotalTranslationXZ,TotalTranslationY,TotalRotationX,TotalRotationY,TotalRotationZ
            string completionTime;
            if (completed){
                completionTime = time.TotalSeconds.ToString("F2");
            }
            else{
                completionTime = "NA";
            }

            sw.WriteLine((taskHandler.currentPairIndex+1) + "," + completionTime + "," + distanceMismatch.ToString("F2") + "," + 
                rotationMismatchX.ToString("F2") + "," + rotationMismatchY.ToString("F2") + "," + rotationMismatchZ.ToString("F2") + "," + 
                this.timeSpentIdle.ToString("F2") + "," + this.timeSpentChecking.ToString("F2") + "," + this.timeSpentTranslationXZ.ToString("F2") + "," + this.timeSpentTranslationY.ToString("F2") + "," + 
                this.timeSpentRotationX.ToString("F2") + "," + this.timeSpentRotationY.ToString("F2") + "," + this.timeSpentRotationZ.ToString("F2") + "," + 
                this.totalTranslationXZ.ToString("F2") + "," + this.totalTranslationY.ToString("F2") + "," + 
                this.totalRotationX.ToString("F2") + "," + this.totalRotationY.ToString("F2") + "," + this.totalRotationZ.ToString("F2"));
        }
    }

    public void logTouchDataFrame(){
        float angleX = Vector3.Angle(taskHandler.dockingPoint.right, taskHandler.objectToDock.right);
        float angleY = Vector3.Angle(taskHandler.dockingPoint.up, taskHandler.objectToDock.up);
        float angleZ = Vector3.Angle(taskHandler.dockingPoint.forward, taskHandler.objectToDock.forward);
        
        Vector3 objectToDockCenter = taskHandler.objectToDock.GetComponent<MeshCollider>().bounds.center;
        Vector3 dockingPointCenter = taskHandler.dockingPoint.GetComponent<MeshCollider>().bounds.center;

        float distanceObjectToDPX = Math.Abs(objectToDockCenter.x - dockingPointCenter.x);
        float distanceObjectToDPY = Math.Abs(objectToDockCenter.y - dockingPointCenter.y);
        float distanceObjectToDPZ = Math.Abs(objectToDockCenter.z - dockingPointCenter.z);


        //Task,Timestamp,State,TouchCount,TouchPosition1,TouchPosition2,ObjectPosition,ObjectRotation,DistanceMismatchX,DistanceMismatchY,DistanceMismatchZ,RotationMismatchX,RotationMismatchY,RotationMismatchZ
        using (StreamWriter sw = File.AppendText(taskHandler.filePathTouchFrames))
        {
            string touchPosition1;
            string touchPosition2;
            if (Input.touchCount == 0){
                touchPosition1 = "NA";
                touchPosition2 = "NA";
            }
            else if (Input.touchCount == 1){
                Touch touch1 = getTouchByID(touch1ID, 1);
                
                touchPosition1 = "(" + touch1.position.x.ToString("F2") + ";" + touch1.position.y.ToString("F2") + ")";
                touchPosition2 = "NA";

            }
            else{
                Touch touch1 = getTouchByID(touch1ID, 1);
                Touch touch2 = getTouchByID(touch2ID, 2);

                touchPosition1 = "(" + touch1.position.x.ToString("F2") + ";" + touch1.position.y.ToString("F2") + ")";
                touchPosition2 = "(" + touch2.position.x.ToString("F2") + ";" + touch2.position.y.ToString("F2") + ")";
            }

            sw.WriteLine((taskHandler.currentPairIndex+1) + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "," + this.currentState + "," + Input.touchCount + "," +
                touchPosition1 + "," + touchPosition2 + ",(" +
                taskHandler.objectToDock.transform.position.x.ToString("F2") + ";" + taskHandler.objectToDock.transform.position.y.ToString("F2") + ";" + taskHandler.objectToDock.transform.position.z.ToString("F2") + "),(" +
                taskHandler.convertEuler(taskHandler.objectToDock.transform.rotation.eulerAngles.x).ToString("F2") + ";" + taskHandler.convertEuler(taskHandler.objectToDock.transform.rotation.eulerAngles.y).ToString("F2") + ";" + taskHandler.convertEuler(taskHandler.objectToDock.transform.rotation.eulerAngles.z).ToString("F2") + ")," +
                distanceObjectToDPX.ToString("F2") + "," + distanceObjectToDPY.ToString("F2") + "," + distanceObjectToDPZ.ToString("F2") + "," +
                angleX.ToString("F2") + "," + angleY.ToString("F2") + "," + angleZ.ToString("F2")); 
        }
    }
    
}



