using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
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
