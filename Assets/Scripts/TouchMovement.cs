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
    
    private const int stateCheckIntervalInitial = 5; //Number of ticks to wait while checking for state change - initial value
    private int stateCheckInterval = stateCheckIntervalInitial; //Number of ticks to wait while checking for state change

    private Vector2 previousTouch1Position;
    private Vector2 previousTouch2Position;

    private const int thresholdMovement = 25; //Maximum movement to be considered an accidental movement
    private const int thresholdMovementTranslationY = 25; //Maximum movement to be considered an accidental movement (X Movement in YTranslation)
    private const int thresholdMovementStationary = 5; //Maximum movement to be considered an accidental movement while stationary
    private const int necessaryMovement = 20; //Minimum movement to be considered a movement
    private const int necessaryAngle = 15; //Minimum angle to be considered a rotation
    private const int necessaryMovementCircle = 25; //Minimum movement to be considered a rotation (Circular Movement)
    
    private const int thresholdErrorInitial = 5; //Tolerance for state change caused by mistakes - initial value
    private int thresholdError = thresholdErrorInitial; //Tolerance for state change caused by mistakes

    private float speedModifierTranslations = 0.02f;
    private float speedModifierRotations = 0.25f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //FixedUpdate is called 50 times per second (can change in Edit -> Project Settings -> Time)
    void FixedUpdate(){
        if (thresholdError == 0){
            currentState = State.Idle;
            thresholdError = thresholdErrorInitial;
        }
        currentState = stateCheck();

        Debug.Log(currentState);    
        //Debug.Log("Error Threshold: " + thresholdError);
        //Debug.Log("Check Interval: " + stateCheckInterval);
    }

    State stateCheck(){
        switch (currentState)
        {
            case State.Idle:
                if (Input.touchCount == 1){
                    Touch touch = Input.GetTouch(0);
                    initialTouch1Position = touch.position;
                    touchCount = 1;
                    return State.Checking;
                }
                else if (Input.touchCount == 2){
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);
                    initialTouch1Position = touch1.position;
                    initialTouch2Position = touch2.position;
                    touchCount = 2;
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
                        previousTouch1Position = Input.GetTouch(0).position;
                        return State.TranslationXZ;
                    }
                    else if (checkTranslationY(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = Input.GetTouch(0).position;
                        previousTouch2Position = Input.GetTouch(1).position;
                        return State.TranslationY;
                    }
                    else if (checkRotationY(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = Input.GetTouch(0).position;
                        previousTouch2Position = Input.GetTouch(1).position;
                        return State.RotationY;
                    }
                    else if (checkRotationX(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = Input.GetTouch(0).position;
                        previousTouch2Position = Input.GetTouch(1).position;
                        return State.RotationX;
                    }
                    else if (checkRotationZ(initialTouch1Position, initialTouch2Position)){
                        previousTouch1Position = Input.GetTouch(0).position;
                        previousTouch2Position = Input.GetTouch(1).position;
                        return State.RotationZ;
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
                    Touch touch = Input.GetTouch(0);

                    Vector2 touchDistance = touch.position - previousTouch1Position;
                    
                    XZTranslation(touchDistance);
                    previousTouch1Position = touch.position;
                    thresholdError = thresholdErrorInitial;
                    return State.TranslationXZ;
                }
                else{
                    thresholdError--;
                    return State.TranslationXZ;
                }
            case State.TranslationY:
                if (checkTranslationY(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);
                    
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
                    else if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else if (Input.touchCount == 1){
                        thresholdError--;
                    }
                    return State.TranslationY;
                }
            case State.RotationX:
                if (checkRotationX(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

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
                    else if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else if (Input.touchCount == 1){
                        thresholdError--;
                    }
                    return State.RotationX;
                }
            case State.RotationY:
                if (checkRotationY(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);
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
                    else if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else if (Input.touchCount == 1){
                        thresholdError--;
                    }
                    return State.RotationY;
                }
            case State.RotationZ:
                if (checkRotationZ(previousTouch1Position, previousTouch2Position)){
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

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
                    else if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        previousTouch1Position = touch1.position;
                        previousTouch2Position = touch2.position;
                    }
                    else if (Input.touchCount == 1){
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
            Touch touch = Input.GetTouch(0);
            float touchDistance = Vector2.Distance(touch1Position, touch.position);
            if (currentState == State.TranslationXZ){
                if (touchDistance  > 0){
                    return true;
                }
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
        if (Input.touchCount == 2){
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);
            float touch1Distance = Vector2.Distance(touch1Position, touch1.position);
            float touch2Distance = Vector2.Distance(touch2Position, touch2.position);
            
            if (currentState == State.TranslationY){
                if ((touch1Distance == 0 && touch2DistanceY >= 0 && touch2DistanceX <= thresholdMovementTranslationY) || (touch2Distance == 0 && touch1DistanceY >= 0 && touch1DistanceX <= thresholdMovementTranslationY)){
                    return true;
                }
            }
            else{
                if ((touch1Distance <= thresholdMovementStationary && touch2DistanceY >= necessaryMovement && touch2DistanceX <= thresholdMovementTranslationY)|| (touch2Distance<= thresholdMovementStationary && touch1DistanceY >= necessaryMovement && touch1DistanceX <= thresholdMovementTranslationY)){
                    return true;
                }
            }
        }
        return false;
    }

    bool checkRotationX(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount == 2){
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);

            if (currentState == State.RotationX){
                if (touch1DistanceX <= thresholdMovement && touch2DistanceX <= thresholdMovement && touch1DistanceY > 0 && touch2DistanceY > 0){
                    return true;
                }
            }
            else{
                if (touch1DistanceX <= thresholdMovement && touch2DistanceX <= thresholdMovement && touch1DistanceY >= necessaryMovement && touch2DistanceY >= necessaryMovement){
                    return true;
                }
            }
        }
        return false;
    }

    bool checkRotationY(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount == 2){
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            //Calculate vector between touch1Position and touch2Position
            Vector2 prevVector = touch2Position - touch1Position;  
            //Calculate vector between current touch1 and current touch2 position
            Vector2 vector = touch2.position - touch1.position;

            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);
            float angle = Vector2.Angle(prevVector, vector);

            //Debug.Log(angle);
            if (currentState == State.RotationY){
                if (Math.Abs(angle) >= 0){
                    return true;
                }
            }
            else{
                if (Math.Abs(angle) >= necessaryAngle && ((touch1DistanceX >= necessaryMovementCircle || touch2DistanceX >= necessaryMovementCircle) && (touch1DistanceY > necessaryMovement || touch2DistanceY > necessaryMovement))){
                    return true;
                }      
            }
            
        }
        return false;
    }

    bool checkRotationZ(Vector2 touch1Position, Vector2 touch2Position){
        if (Input.touchCount == 2){
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            float touch1DistanceX = Math.Abs(touch1Position.x - touch1.position.x);
            float touch2DistanceX = Math.Abs(touch2Position.x - touch2.position.x);
            float touch1DistanceY = Math.Abs(touch1Position.y - touch1.position.y);
            float touch2DistanceY = Math.Abs(touch2Position.y - touch2.position.y);
            
            if (currentState == State.RotationZ){
                if (touch1DistanceX > 0 && touch2DistanceX > 0 && touch1DistanceY <= thresholdMovement && touch2DistanceY <= thresholdMovement){
                    return true;
                }
            }
            else{
                if (touch1DistanceX >= necessaryMovement && touch2DistanceX >= necessaryMovement && touch1DistanceY <= thresholdMovement && touch2DistanceY <= thresholdMovement){
                    return true;
                }
            }
        }
        return false;
    }

    void XZTranslation(Vector2 touchDistance){
        // transform.position = new Vector3(
        //     transform.position.x + touchDistance.x * speedModifierTranslations, 
        //     transform.position.y, 
        //     transform.position.z + touchDistance.y * speedModifierTranslations);

        //Relative to frame
        transform.position += referenceFrame.right * touchDistance.x * speedModifierTranslations;
        transform.position += referenceFrame.forward * touchDistance.y * speedModifierTranslations;
    }


    void YTranslation(Vector2 touch1Distance, Vector2 touch2Distance){
        if (Math.Abs(touch1Distance.y) > Math.Abs(touch2Distance.y)){
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y + touch1Distance.y * speedModifierTranslations, 
                transform.position.z);
        }
        else{
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y + touch2Distance.y * speedModifierTranslations, 
                transform.position.z);
        }
        
    }


    void XRotation(Vector2 touch1Distance, Vector2 touch2Distance){
        //transform.Rotate(new Vector3((touch1Distance.y + touch2Distance.y)/2 * speedModifierRotations, 0, 0), Space.World);

        //Relative to frame
        transform.Rotate(referenceFrame.right, (touch1Distance.y + touch2Distance.y)/2 * speedModifierRotations, Space.World);    
    }

    void YRotation(float angle){
        transform.Rotate(new Vector3(0, angle, 0), Space.World);
    }


    void ZRotation(Vector2 touch1Distance, Vector2 touch2Distance){
        //transform.Rotate(new Vector3(0, 0, -(touch1Distance.x + touch2Distance.x)/2 * speedModifierRotations), Space.World);

        //Relative to frame
        transform.Rotate(referenceFrame.forward, -(touch1Distance.x + touch2Distance.x)/2 * speedModifierRotations, Space.World);
    }
}

