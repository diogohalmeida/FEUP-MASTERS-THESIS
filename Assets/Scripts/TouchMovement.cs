using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    private const int thresholdMovementX = 25; //Maximum movement to be considered an accidental movement in the X axis 
    private const int thresholdMovementY = 40; //Maximum movement to be considered an accidental movement in the Y axis
    private const int thresholdMovementStationary = 1; //Maximum movement to be considered an accidental movement while stationary
    private const int necessaryMovement = 10; //Minimum movement to be considered a movement
    private const int necessaryAngle = 10; //Minimum angle to be considered a rotation
    private const int necessaryMovementCircle = 15; //Minimum movement to be considered a rotation (Circular Movement)
    
    private const int thresholdErrorInitial = 5; //Tolerance for state change caused by mistakes - initial value
    private int thresholdError = thresholdErrorInitial; //Tolerance for state change caused by mistakes

    private float velocityModifierTranslations = 0.02f; //Fixed value to decrease translation velocity
    private float velocityModifierTranslationY = 0.01f; //Fixed value to decrease translationY velocity
    private float velocityModifierRotations = 0.25f; //Fixed value to decrease rotation velocity

    private float scalingConstant = 3000.0f;    //Used to calculate scaling factor (maximum velocity before scaling > 1)

    private TaskHandler taskHandler;

    public GameObject rotationArrowX;
    public GameObject rotationArrowY;
    public GameObject rotationArrowZ;

    public GameObject rotationArrow = null;
    private int rotationArrowScale;
    

    // Start is called before the first frame update
    void Start()
    {
        taskHandler = GetComponent<TaskHandler>();
    }

    //FixedUpdate is called 50 times per second (can change in Edit -> Project Settings -> Time)
    void FixedUpdate(){
        if (thresholdError == 0){
            if (rotationArrow != null){
                Destroy(rotationArrow);
            }
            currentState = State.Idle;
            thresholdError = thresholdErrorInitial;
        }
        currentState = stateCheck();

        //Debug.Log(currentState);    
        //Debug.Log("Error Threshold: " + thresholdError);
        //Debug.Log("Check Interval: " + stateCheckInterval);
    }

    State stateCheck(){
        switch (currentState)
        {
            case State.Idle:
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
                if (Input.touchCount != touchCount){
                    thresholdError = thresholdErrorInitial;
                    stateCheckInterval = stateCheckIntervalInitial;
                    return State.Idle;
                }
                if (stateCheckInterval == 0){
                    stateCheckInterval = stateCheckIntervalInitial;
                    if (checkTranslationXZ(initialTouch1Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        XZTranslation(previousTouch1Position - initialTouch1Position);
                        return State.TranslationXZ;
                    }
                    else if (checkTranslationY(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;
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

                        //Check if the motion is clockwise or counterclockwise (by default the angle is positive so it's clockwise)
                        float cross = (previousTouch1Position.x - previousTouch2Position.x) * (initialTouch1Position.y - initialTouch2Position.y) - (previousTouch1Position.y - previousTouch2Position.y) * (initialTouch1Position.x - initialTouch2Position.x);
                        if (cross < 0){
                            angle = -angle; //Counterclockwise
                        }


                        //Instantiate RotationArrowX prefab as child of objectToDock and rotate it around the objectToDock
                        rotationArrow = Instantiate(rotationArrowY, taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, Quaternion.identity);
                    
                        rotationArrow.transform.SetParent(taskHandler.objectToDock.transform);

                        YRotation(angle);

                        return State.RotationY;
                    }
                    else if (checkRotationZ(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;

                        //Instantiate RotationArrowX prefab as child of objectToDock and rotate it around the objectToDock
                        rotationArrow = Instantiate(rotationArrowZ, taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, Quaternion.identity);

                        //Rotate the rotationArrow 90 degrees to make it point in the correct direction
                        rotationArrow.transform.Rotate(90, 0, 0);

                        rotationArrow.transform.SetParent(taskHandler.objectToDock.transform);

                        ZRotation(previousTouch1Position - initialTouch1Position, previousTouch2Position - initialTouch2Position);

                        return State.RotationZ;
                    }
                    else if (checkRotationX(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = getTouchByID(touch1ID, 1).position;
                        previousTouch2Position = getTouchByID(touch2ID, 2).position;
                        
                        //Instantiate RotationArrowX prefab as child of objectToDock and rotate it around the objectToDock
                        rotationArrow = Instantiate(rotationArrowX, taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, Quaternion.identity);
                        
                        //Rotate the rotationArrow 90 degrees to make it point in the correct direction
                        rotationArrow.transform.Rotate(0, 0, 90);

                        rotationArrow.transform.SetParent(taskHandler.objectToDock.transform);
                        
                        XRotation(previousTouch1Position - initialTouch1Position, previousTouch2Position - initialTouch2Position);
                        
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
                if ((touch1Distance == 0 && touch2DistanceY >= 0 && touch2DistanceX <= thresholdMovementX) || (touch2Distance == 0 && touch1DistanceY >= 0 && touch1DistanceX <= thresholdMovementX)){
                    return true;
                }
            }
            else{
                if ((touch1Distance <= thresholdMovementStationary && touch2DistanceY >= necessaryMovement && touch2DistanceX <= thresholdMovementX)|| (touch2Distance<= thresholdMovementStationary && touch1DistanceY >= necessaryMovement && touch1DistanceX <= thresholdMovementX)){
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
            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);

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
                if (Math.Abs(angle) >= necessaryAngle && ((touch1DistanceX >= necessaryMovement || touch2DistanceX >= necessaryMovement) && (touch1DistanceY >= 0 || touch2DistanceY >= 0) && (touch1Distance >= necessaryMovementCircle || touch2Distance >= necessaryMovementCircle))){
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
            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);
            
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
        return false;
    }

    void XZTranslation(Vector2 touchDistance){
        // taskHandler.objectToDock.transform.position = new Vector3(
        //     taskHandler.objectToDock.transform.position.x + touchDistance.x * velocityModifierTranslations, 
        //     taskHandler.objectToDock.transform.position.y, 
        //     taskHandler.objectToDock.transform.position.z + touchDistance.y * velocityModifierTranslations);


        float velocity = touchDistance.magnitude / Time.deltaTime;

        //Prevent teleports from lifting the finger and moving it back
        if (velocity > 10000){
            return;
        }

        float scalingFactorDistance = Vector3.Distance(Camera.main.transform.position, taskHandler.objectToDock.transform.position) * 0.05f;

        float scalingFactorVelocity = velocity / scalingConstant;

        Vector3 newPositionX = taskHandler.objectToDock.transform.position + (referenceFrame.right * touchDistance.x * velocityModifierTranslations * Math.Min(scalingFactorVelocity, 1.2f) * scalingFactorDistance);
        Vector3 newPositionZ = taskHandler.objectToDock.transform.position + (referenceFrame.forward * touchDistance.y * velocityModifierTranslations *  Math.Min(scalingFactorVelocity, 1.2f)* scalingFactorDistance);

        //Relative to frame
        if ((newPositionX.x > taskHandler.collisionXmin && newPositionX.x < taskHandler.collisionXmax)
            && (newPositionZ.z > taskHandler.collisionZmin && newPositionZ.z < taskHandler.collisionZmax)){
            taskHandler.objectToDock.transform.position = newPositionX + (newPositionZ - taskHandler.objectToDock.transform.position);
        }
    }


    void YTranslation(Vector2 touch1Distance, Vector2 touch2Distance){
        if (Math.Abs(touch1Distance.y) > Math.Abs(touch2Distance.y)){
            float velocity = Math.Abs(touch1Distance.y) / Time.deltaTime;
            float scalingFactorVelocity = velocity / scalingConstant;

            Vector3 newPositionY = taskHandler.objectToDock.transform.position + (referenceFrame.up * touch1Distance.y * velocityModifierTranslationY * Math.Min(scalingFactorVelocity, 1.2f));
            if (newPositionY.y > taskHandler.collisionYmin && newPositionY.y < taskHandler.collisionYmax){
                taskHandler.objectToDock.transform.position = newPositionY;
            }
        }
        else{
            float velocity = Math.Abs(touch2Distance.y) / Time.deltaTime;
            float scalingFactorVelocity = velocity / scalingConstant;
            
            Vector3 newPositionY = taskHandler.objectToDock.transform.position + (referenceFrame.up * touch2Distance.y * velocityModifierTranslationY * Math.Min(scalingFactorVelocity, 1.2f));
            if (newPositionY.y > taskHandler.collisionYmin && newPositionY.y < taskHandler.collisionYmax){
                taskHandler.objectToDock.transform.position = newPositionY;
            }
        }
        
    }


    void XRotation(Vector2 touch1Distance, Vector2 touch2Distance){
        //taskHandler.objectToDock.transform.Rotate(new Vector3((touch1Distance.y + touch2Distance.y)/2 * velocityModifierRotations, 0, 0), Space.World);

        //Relative to frame
        //taskHandler.objectToDock.transform.Rotate(referenceFrame.right, (touch1Distance.y + touch2Distance.y)/2 * velocityModifierRotations, Space.World);  
        if (touch1Distance.y + touch2Distance.y < 0){
            //mirror rotationArrow
            rotationArrow.transform.localScale = new Vector3(rotationArrowX.transform.localScale.x, rotationArrowX.transform.localScale.y, -rotationArrowX.transform.localScale.z);
        }
        else{
            rotationArrow.transform.localScale = new Vector3(rotationArrowX.transform.localScale.x, rotationArrowX.transform.localScale.y, rotationArrowX.transform.localScale.z);
        }
        
        taskHandler.objectToDock.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.right, (touch1Distance.y + touch2Distance.y)/2 * velocityModifierRotations);  

    }

    void YRotation(float angle){
        //taskHandler.objectToDock.transform.Rotate(new Vector3(0, angle, 0), Space.World);
        if (angle > 0){
            //mirror rotationArrow
            rotationArrow.transform.localScale = new Vector3(-rotationArrowY.transform.localScale.x, rotationArrowY.transform.localScale.y, rotationArrowY.transform.localScale.z);
        }
        else if (angle < 0){
            rotationArrow.transform.localScale = new Vector3(rotationArrowY.transform.localScale.x, rotationArrowY.transform.localScale.y, rotationArrowY.transform.localScale.z);
        }


        taskHandler.objectToDock.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.up, angle);
    }


    void ZRotation(Vector2 touch1Distance, Vector2 touch2Distance){
        //taskHandler.objectToDock.transform.Rotate(new Vector3(0, 0, -(touch1Distance.x + touch2Distance.x)/2 * velocityModifierRotations), Space.World);

        //Relative to frame
        //taskHandler.objectToDock.transform.Rotate(referenceFrame.forward, -(touch1Distance.x + touch2Distance.x)/2 * velocityModifierRotations, Space.World);
        if (touch1Distance.x + touch2Distance.x < 0){
            //mirror rotationArrow
            rotationArrow.transform.localScale = new Vector3(-rotationArrowZ.transform.localScale.x, rotationArrowZ.transform.localScale.y, rotationArrowZ.transform.localScale.z);
        }
        else{
            rotationArrow.transform.localScale = new Vector3(rotationArrowZ.transform.localScale.x, rotationArrowZ.transform.localScale.y, rotationArrowZ.transform.localScale.z);
        }
        
        
        taskHandler.objectToDock.transform.RotateAround(taskHandler.objectToDock.transform.GetComponent<MeshCollider>().bounds.center, referenceFrame.forward, -(touch1Distance.x + touch2Distance.x)/2 * velocityModifierRotations);
    }


    Touch getTouchByID(int id, int touchNumber){
        for (int i = 0; i < Input.touchCount; i++){
            if (Input.GetTouch(i).fingerId == id){
                return Input.GetTouch(i);
            }
        }
        if (touchNumber == 1){
            touch1ID = Input.GetTouch(0).fingerId;
        }
        else{
            touch2ID = Input.GetTouch(0).fingerId;
        }
        
        return Input.GetTouch(0);
    }
}

