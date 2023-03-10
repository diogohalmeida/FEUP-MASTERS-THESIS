using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movement : MonoBehaviour
{

    private Touch touch;
    private float speedModifier;
    private int currentState;
    private int threshold;
    
    private int initialCheckThreshold;
    private const int startingInitialCheckThreshold = 10; //Tolerance for initial check
    
    private const int thresholdInitial = 45;     //Tolerance for state change
    private const int thresholdDistance = 250;  //Tolerance for distance between touches to differentiate between YTranslation and rotations
    private const int thresholdMovement = 10;   //Tolerance for y movement in ZRotation and x movement in XRotation
    
    private Vector2 fingerVector;

    // Start is called before the first frame update
    void Start()
    {
        speedModifier = 0.01f;
        threshold = thresholdInitial;
        initialCheckThreshold = startingInitialCheckThreshold;
        currentState = 0; //0 = initial state, 1 = XZTranslation, 2 = YTranslation, 3 = ZRotation, 4 = XRotation, 5 = YRotation
    }

    

    void Update(){
    //     if (Input.touchCount > 2){
    //         return;
    //     }
    //     if (Input.touchCount == 2){
    //         Touch touch1 = Input.GetTouch(0);
    //         Touch touch2 = Input.GetTouch(1);

    //         //Calculate vector between touch1 and touch2 position
    //         Vector2 vector = touch2.position - touch1.position;

    //         //Check if fingerVector is equal to (0,0)
    //         if (fingerVector == Vector2.zero){
    //             fingerVector = vector;
    //         }
    //         else{
    //             if(initialCheckThreshold > 0){
    //                 initialCheckThreshold--;
    //                 return;
    //             }
    //             else{
    //                 //Debug vector and fingerVector
    //                 Debug.Log("V: " + vector);
    //                 Debug.Log("FV: " + fingerVector);

    //                 //Calculate angle between fingerVector and vector
    //                 float angle = Vector2.Angle(fingerVector, vector);
    //                 Debug.Log("Angle: " + angle);

    //                 //Update for next check 
    //                 fingerVector = vector;
    //                 initialCheckThreshold = startingInitialCheckThreshold;
    //             }
                
    //         }
    //     }
    //     else{
    //         fingerVector = Vector2.zero;
    //     }
        
    // }
    




        //log current state
        Debug.Log(currentState);

        if (Input.touchCount > 2){
            return;
        }

        if (currentState == 0){
            initialCheck();
        }
        else if(threshold > 0){
            switch (currentState)
            {
                case 1:
                    if (Input.touchCount == 1){
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Moved){
                            XZTranslation(touch);
                            threshold = thresholdInitial;
                        }
                        else{
                            threshold--;
                        }
                    }
                    else{
                        threshold--;
                    }
                    break;
                case 2:
                    if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        //Calculate distance between touch1 and touch2 position
                        float fingerDistance = Vector2.Distance(touch1.position, touch2.position);
                        //Debug.Log(fingerDistance);

                        if (fingerDistance >= thresholdDistance && ((touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary) || (touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Moved))){
                            YTranslation(touch1, touch2);
                            threshold = thresholdInitial;
                        }
                        else{
                            threshold--;
                        }
                    }
                    else{
                        threshold--;
                    }
                    break;
                case 3:
                    if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        //Calculate distance between touch1 and touch2 position
                        float fingerDistance = Vector2.Distance(touch1.position, touch2.position);
                        //Debug.Log(fingerDistance);

                        if (fingerDistance < thresholdDistance && (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)){
                            //log delta positions for both touches
                            Debug.Log(touch1.deltaPosition);
                            Debug.Log(touch2.deltaPosition);
                            if ((touch1.deltaPosition.x != 0 && touch2.deltaPosition.x != 0) && (Math.Abs(touch1.deltaPosition.y) < thresholdMovement && Math.Abs(touch2.deltaPosition.y) < thresholdMovement)){
                                ZRotation(touch1, touch2);
                                threshold = thresholdInitial;
                            }
                            else{
                                threshold--;
                            }
                        }
                        else{
                            threshold--;
                        }
                    }
                    else{
                        threshold--;
                    }
                    break;
                case 4:
                    if (Input.touchCount == 2){
                        Touch touch1 = Input.GetTouch(0);
                        Touch touch2 = Input.GetTouch(1);
                        //Calculate distance between touch1 and touch2 position
                        float fingerDistance = Vector2.Distance(touch1.position, touch2.position);
                        //Debug.Log(fingerDistance);

                        if (fingerDistance < thresholdDistance && (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)){
                            Debug.Log(touch1.deltaPosition);
                            Debug.Log(touch2.deltaPosition);

                            if ((touch1.deltaPosition.y != 0 && touch2.deltaPosition.y != 0) && (Math.Abs(touch1.deltaPosition.x) < thresholdMovement && Math.Abs(touch2.deltaPosition.x) < thresholdMovement)){
                                XRotation(touch1, touch2);
                                threshold = thresholdInitial;
                            }
                            else{
                                threshold--;
                            }
                        }
                        else{
                            threshold--;
                        }
                    }
                    else{
                        threshold--;
                    }
                    break;
            }
        }
        else{
            currentState = 0;
            threshold = thresholdInitial;
        }


    }
            


    void initialCheck(){
        if (Input.touchCount == 1){
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved){
                XZTranslation(touch);
                currentState = 1;
                return;
            }
        }
        else if (Input.touchCount == 2){
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            //Calculate distance between touch1 and touch2 position
            float fingerDistance = Vector2.Distance(touch1.position, touch2.position);
            //Debug.Log(fingerDistance);

            if (fingerDistance >= thresholdDistance && ((touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary) || (touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Moved))){
                YTranslation(touch1, touch2);
                currentState = 2;
                return;
            }
            else if (fingerDistance < thresholdDistance && (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)){
                if (touch1.deltaPosition.x != 0 && touch2.deltaPosition.x != 0 ){
                    ZRotation(touch1, touch2);
                    currentState = 3;
                    return;
                } 
                else if (touch1.deltaPosition.y != 0 && touch2.deltaPosition.y != 0 ){
                    XRotation(touch1, touch2);
                    currentState = 4;
                    return;
                }
                
            }
        }
    }

    void XZTranslation(Touch touch){
        transform.position = new Vector3(
            transform.position.x + touch.deltaPosition.x * speedModifier, 
            transform.position.y, 
            transform.position.z + touch.deltaPosition.y * speedModifier);
    }


    void YTranslation(Touch touch1, Touch touch2){
        if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary){
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y + touch1.deltaPosition.y * speedModifier, 
                transform.position.z);
        }
        else if (touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Moved){
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y + touch2.deltaPosition.y * speedModifier, 
                transform.position.z);
        }
    }


    void XRotation(Touch touch1, Touch touch2){
        if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary){
            transform.Rotate(new Vector3(touch1.deltaPosition.y * speedModifier*25, 0, 0), Space.World);
        } 
        else if (touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Moved){
            transform.Rotate(new Vector3(touch2.deltaPosition.y * speedModifier*25, 0, 0), Space.World);
        }
        else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved){
            transform.Rotate(new Vector3((touch1.deltaPosition.y + touch2.deltaPosition.y)/2 * speedModifier*25, 0, 0), Space.World);
        }
    }


    void ZRotation(Touch touch1, Touch touch2){
        if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary){
            transform.Rotate(new Vector3(0, 0, touch1.deltaPosition.x * speedModifier*25), Space.World);
        } 
        else if (touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Moved){
            transform.Rotate(new Vector3(0, 0, touch2.deltaPosition.x * speedModifier*25), Space.World);
        }
        else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved){
            transform.Rotate(new Vector3(0, 0, -(touch1.deltaPosition.x + touch2.deltaPosition.x)/2 * speedModifier*25), Space.World);
        }
    }














    //Old update function 
    // void Update()
    // {
    //     //X and Z Translation
    //     if (Input.touchCount == 1)
    //     {
    //         touch = Input.GetTouch(0);
    //         if (touch.phase == TouchPhase.Moved)
    //         {
    //             transform.position = new Vector3(
    //                 transform.position.x + touch.deltaPosition.x * speedModifier, 
    //                 transform.position.y, 
    //                 transform.position.z + touch.deltaPosition.y * speedModifier);
    //         }
            
    //     }
    //     else if (Input.touchCount == 2)
    //     {
    //         Touch touch1 = Input.GetTouch(0);
    //         Touch touch2 = Input.GetTouch(1);

    //         //Calculate distance between touch1 and touch2 position
    //         float fingerDistance = Vector2.Distance(touch1.position, touch2.position);
    //         //Debug.Log(fingerDistance);
            

    //         //Rotations
    //         if (fingerDistance <= 250 && touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved){
                
    //             //X (Pitch) Rotation
    //             if (touch1.deltaPosition.y != 0 && touch2.deltaPosition.y != 0 )
    //                 transform.Rotate(new Vector3((touch1.deltaPosition.y + touch2.deltaPosition.y)/2 * speedModifier*10, 0, 0), Space.World);
    //             //Z (Roll) Rotation
    //             else if (touch1.deltaPosition.x != 0 && touch2.deltaPosition.x != 0 )
    //                 transform.Rotate(new Vector3(0, 0, -(touch1.deltaPosition.x + touch2.deltaPosition.x)/2 * speedModifier*10), Space.World);
    //         }

            
    //         //Y Translation
    //         if (fingerDistance >= 250 && touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary){
    //             transform.position = new Vector3(
    //                 transform.position.x, 
    //                 transform.position.y + touch1.deltaPosition.y * speedModifier, 
    //                 transform.position.z);
    //         }
    //         else if (fingerDistance >= 250 && touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Moved){
    //             transform.position = new Vector3(
    //                 transform.position.x, 
    //                 transform.position.y + touch2.deltaPosition.y * speedModifier, 
    //                 transform.position.z);
    //         }
            
    //     }
    // }
}

    








