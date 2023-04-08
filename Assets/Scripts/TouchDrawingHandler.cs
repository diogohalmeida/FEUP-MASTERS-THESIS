using System.Collections.Generic;
using UnityEngine;

public class TouchDrawingHandler : MonoBehaviour
{
    public GameObject pixel;

    void FixedUpdate()
    {
        if (Input.touchCount > 0)
        {

            Touch touch = Input.GetTouch(0);

            if (touch.position.x > Screen.width || touch.position.y > Screen.height || touch.position.x < 0 || touch.position.y < 0)
            {
                return;
            }
            float positionRelativeX = touch.position.x / Screen.width;
            float positionRelativeY = touch.position.y / Screen.height;

            Vector3 position = new Vector3();
            
            position.x = positionRelativeX * transform.localScale.x * 10 - (10*transform.localScale.x)/2 + transform.position.x;
            position.z = positionRelativeY * transform.localScale.z * 10 - (10*transform.localScale.z)/2 + transform.position.z;
            position.y = transform.position.y + 0.1f;

            Debug.Log(position);
            
            Instantiate(pixel, position, Quaternion.identity);
        }
    }
}