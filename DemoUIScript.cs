using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DemoUIScript : MonoBehaviour
{

    public float vSliderValue1 = 0.0f;
    public float vSliderValue2 = 0.0f;

    public Slider vSlider1;
    public Slider vSlider2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    vSliderValue1 = GUI.VerticalSlider(new Rect(xPos, yPos, sliderWidth, sliderHeight), vSliderValue1, 10.0f, 0.0f);

    yPos += sliderHeight + padding;

    GUI.backgroundColor = Color.blue; // Set the background color for the second slider
    vSliderValue2 = GUI.VerticalSlider(new Rect(xPos, yPos, sliderWidth, sliderHeight), vSliderValue2, 10.0f, 0.0f);

    // Reset the background color
    GUI.backgroundColor = Color.white;
}





}
