using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo_Slider : MonoBehaviour
{
    public float vSliderValue1 = 1.0f;
    public float vSliderValue2 = 0.0f;

    public Slider vSlider1;
    public Slider vSlider2;


    public float speedMultiplier = 2.0f;

    List<int> values = new List<int>();
    public int index = 0;
    // Start is called before the first frame update
    //void Start()
    //{
    //    vSliderValue1 = 1.0f;
    //    vSliderValue2 = 1.0f;

    //}

    // Update is called once per frame
    void Update()
    {
        //vSliderValue1 += Time.deltaTime * speedMultiplier;
        //vSliderValue1 = Mathf.Clamp(vSliderValue1, 0.0f, 10.0f);
        for (int i = 0; i < 100; i++)
        {
            int value = UnityEngine.Random.Range(1, 6);
            values.Add(value);
        }

        // Generate the next 100 values in the range 5-10
        for (int i = 0; i < 2000; i++)
        {
            int value = UnityEngine.Random.Range(8, 11);
            values.Add(value);
        }

        for (int i = 0; i < 1000; i++)
        {
            int value = UnityEngine.Random.Range(1, 6);
            values.Add(value);
        }
        for (int i = 0; i < 1000; i++)
        {
            int value = UnityEngine.Random.Range(8, 11);
            values.Add(value);
        }

        if (values[index++] > 6)
        {
            StartCoroutine(IncreaseSliderCoroutine());
        }
        else
        {
            StartCoroutine(DecreaseSliderCoroutine());

        }

    }

    void Start()
    {
        vSliderValue1 = 1.0f;
        vSliderValue2 = 1.0f;

    }

    void OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float sliderWidth = 100f;
        float sliderHeight = 70f;
        float padding = 10f;

        float xPos = screenWidth - sliderWidth - padding;
        float yPos = padding;

        GUI.backgroundColor = Color.green; // Set the background color for the first slider
        vSliderValue1 = GUI.VerticalSlider(new Rect(xPos, yPos, sliderWidth, sliderHeight), vSliderValue1, 10.0f, 1.0f);

        yPos += sliderHeight + padding;

        GUI.backgroundColor = Color.blue; // Set the background color for the second slider
        vSliderValue2 = GUI.VerticalSlider(new Rect(xPos, yPos, sliderWidth, sliderHeight), vSliderValue2, 10.0f, 1.0f);

        Debug.Log("Slider value is = " + vSliderValue1);
    }

    IEnumerator IncreaseSliderCoroutine()
    {
        Debug.Log("Inside the coroutine" + vSliderValue1);
        while (vSliderValue1 > 0 && vSliderValue1 < 6f)
        {
            Debug.Log("inside coroutine while loop" + vSliderValue1);
            vSliderValue1 += vSliderValue1 * Time.deltaTime * 0.0005f;
            yield return null;
        }
        Debug.Log("Outside the while loop");
    }

    IEnumerator DecreaseSliderCoroutine()
    {
        Debug.Log("Inside the coroutine" + vSliderValue1);
        while (vSliderValue1 > 0)
        {
            Debug.Log("inside coroutine while loop" + vSliderValue1);
            vSliderValue1 -= vSliderValue1 * Time.deltaTime * 0.0005f;
            yield return null;
        }
        Debug.Log("Outside the while loop");
    }


}
