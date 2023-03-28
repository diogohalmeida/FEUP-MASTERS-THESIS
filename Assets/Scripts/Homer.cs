using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

public class Homer : MonoBehaviour
{
    [SerializeField] GameObject hand;
    Hand handScript;
    bool grabbed = false;
    // Start is called before the first frame update
    void Start()
    {
        handScript = hand.GetComponent<Hand>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(GetComponent<Renderer>().material.color == Color.red){
            // change the hand's world position to the cube's world position
            //handScript.objectAttachmentPoint = transform;
            handScript.AttachObject(gameObject, GrabTypes.Grip, Hand.AttachmentFlags.ParentToHand |
                                                              Hand.AttachmentFlags.DetachOthers |
                                                              Hand.AttachmentFlags.DetachFromOtherHand |
                                                              Hand.AttachmentFlags.TurnOnKinematic, transform);
            grabbed = true;
        }
        else if(GetComponent<Renderer>().material.color != Color.green && grabbed){
            // ungrab the cube
            grabbed = false;
            handScript.DetachObject(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider collision)
    {        
        if(collision.gameObject.transform.GetComponent<Renderer>().material.color == Color.green){
            GetComponent<Renderer>().material.color = Color.red;
        }
        else{
            GetComponent<Renderer>().material.color = Color.green;
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if(collision.gameObject.transform.GetComponent<Renderer>().material.color == Color.green){
            GetComponent<Renderer>().material.color = Color.red;
        }

        else if(collision.gameObject.transform.GetComponent<Renderer>().material.color != Color.green){
            GetComponent<Renderer>().material.color = Color.green;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        // change the cube's color to white
        GetComponent<Renderer>().material.color = Color.white;
    }

}
