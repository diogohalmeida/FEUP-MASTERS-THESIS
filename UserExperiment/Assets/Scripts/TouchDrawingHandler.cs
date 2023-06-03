using System.Collections.Generic;
using UnityEngine;

public class TouchDrawingHandler : MonoBehaviour
{
    public GameObject pixelRed;
    public GameObject pixelGreen;
    private List<GameObject> pixelsRed = new List<GameObject>();
    private List<GameObject> pixelsGreen = new List<GameObject>();
    private TouchMovement touchMovement;

    private TaskHandler taskHandler;

    private float sceneScale;

    void Start(){
        touchMovement = GameObject.Find("DockingObjects").GetComponent<TouchMovement>();
        sceneScale = GameObject.Find("Docking Task Scene").transform.localScale.x;
        taskHandler = GameObject.Find("DockingObjects").GetComponent<TaskHandler>();
    }

    void FixedUpdate()
    {
        if (taskHandler.mode == 1 && taskHandler.phase == 1){
            //destroy all pixels and return
            for (int i = pixelsRed.Count - 1; i >= 0; i--)
            {
                Destroy(pixelsRed[i]);
                pixelsRed.RemoveAt(i);
            }
            for (int i = pixelsGreen.Count - 1; i >= 0; i--)
            {
                Destroy(pixelsGreen[i]);
                pixelsGreen.RemoveAt(i);
            }
            return;
        }
        if (Input.touchCount > 0){
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.position.x > Screen.width || touch.position.y > Screen.height || touch.position.x < 0 || touch.position.y < 0)
                {
                    continue;
                }

                float positionRelativeX = touch.position.x / Screen.width;
                float positionRelativeY = touch.position.y / Screen.height;

                Vector3 position = new Vector3();
                position.x = positionRelativeX * transform.localScale.x*10*sceneScale - (10*sceneScale*transform.localScale.x) / 2 + transform.position.x;
                position.z = positionRelativeY * transform.localScale.z*10*sceneScale  - (10*sceneScale*transform.localScale.z) / 2 + transform.position.z;
                position.y = transform.position.y + 0.1f * sceneScale;



                if (touch.fingerId == touchMovement.touch1ID || touch.fingerId == touchMovement.touch2ID)
                {
                    GameObject newPixel = Instantiate(pixelGreen, position, Quaternion.identity);
                    newPixel.transform.SetParent(transform);
                    pixelsGreen.Add(newPixel);
                }
                else
                {
                    GameObject newPixel = Instantiate(pixelRed, position, Quaternion.identity);
                    newPixel.transform.SetParent(transform);
                    pixelsRed.Add(newPixel);
                }
            }
        }

        for (int i = pixelsRed.Count - 1; i >= 0; i--)
        {
            Color pixelColor = pixelsRed[i].GetComponent<Renderer>().material.color;
            pixelColor.a -= Time.deltaTime / 0.1f;
            pixelsRed[i].GetComponent<Renderer>().material.color = pixelColor;

            if (pixelColor.a <= 0)
            {
                Destroy(pixelsRed[i]);
                pixelsRed.RemoveAt(i);
            }
        }

        for (int i = pixelsGreen.Count - 1; i >= 0; i--)
        {
            Color pixelColor = pixelsGreen[i].GetComponent<Renderer>().material.color;
            pixelColor.a -= Time.deltaTime / 2f;
            pixelsGreen[i].GetComponent<Renderer>().material.color = pixelColor;

            if (pixelColor.a <= 0)
            {
                Destroy(pixelsGreen[i]);
                pixelsGreen.RemoveAt(i);
            }
        }
    }
}