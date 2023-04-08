using System.Collections.Generic;
using UnityEngine;

public class TouchDrawingHandler : MonoBehaviour
{
    public GameObject pixelRed;
    public GameObject pixelGreen;
    private List<GameObject> pixelsRed = new List<GameObject>();
    private List<GameObject> pixelsGreen = new List<GameObject>();

    void FixedUpdate()
    {
        if (Input.touchCount == 0 || Input.touchCount > 2)
        {
            return;
        }

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
            position.x = positionRelativeX * transform.localScale.x * 10 - (10 * transform.localScale.x) / 2 + transform.position.x;
            position.z = positionRelativeY * transform.localScale.z * 10 - (10 * transform.localScale.z) / 2 + transform.position.z;
            position.y = transform.position.y + 0.1f;

            if (i == 0)
            {
                GameObject newPixel = Instantiate(pixelRed, position, Quaternion.identity);
                pixelsRed.Add(newPixel);
            }
            else if (i == 1)
            {
                GameObject newPixel = Instantiate(pixelGreen, position, Quaternion.identity);
                pixelsGreen.Add(newPixel);
            }
        }

        for (int i = pixelsRed.Count - 1; i >= 0; i--)
        {
            Color pixelColor = pixelsRed[i].GetComponent<Renderer>().material.color;
            pixelColor.a -= Time.deltaTime / 5f;
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
            pixelColor.a -= Time.deltaTime / 5f;
            pixelsGreen[i].GetComponent<Renderer>().material.color = pixelColor;

            if (pixelColor.a <= 0)
            {
                Destroy(pixelsGreen[i]);
                pixelsGreen.RemoveAt(i);
            }
        }
    }
}