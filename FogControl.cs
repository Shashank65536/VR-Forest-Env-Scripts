using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class FogControl : MonoBehaviour
{
    //// Start is called before the first frame update
    //public float startDensity = 0.01f;
    //public float targetDensity = 0.002f;
    //public float changeSpeed = 0.0001f;
    public float rotationSpeed = 10f; // The speed at which to rotate the light

    //private bool increasing = true;
    //public float twoSecondsTimer = 6.0f;
    //private float timer = 0.0f;

    public float fogDensity = 0.5f;
    public float maxFogDensity = 0.05f;
    public float fogStartTime = 0.0f;
    public float fogDuration = 20.0f;
    public float maxTime = 20.0f;
    private float elapsedTime = 0.0f;
    public int i = 0;
    public float fogDensityThreshold = 0.1f; // threshold value for the fog density
    public float fogSpeed = 0.01f; // speed at which the fog density changes
    public float minFogDensity = 0.1f;

    public float fogDensityStep = 0.1f;
    private bool isDecreasing = false;
    private bool isIncreasing = false;
    public float fogDecreaseTime = 1.0f;
    public int incCount = 0;

    Coroutine fogDensityIncCoroutine;
    Coroutine fogDensityDecCoroutine;
    Coroutine sliderIncCoroutine;
    List<int> values = new List<int>();
    List<double> sinWavevalues = new List<double>();
    int randomValue = 0;
    private float timer;
    Canvas canvas;  
    //canvasSlider = canvas.GetComponentInChildren<Slider>();
    public Slider mySlider;
    private float fillTime = 10f;
    TcpListener serverSocket = null;
    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity =fogDensity;
        canvas = GetComponentInChildren<Canvas>();
        mySlider = canvas.GetComponentInChildren<Slider>();
        mySlider.enabled = true;
        mySlider.maxValue = 10f;
        mySlider.minValue = 1f;
        if (mySlider == null) { 
            Debug.Log("Slider not found");
        }

 
        //for (int i = 0; i < 10; i++)
        //{
        //    int value = UnityEngine.Random.Range(1, 6);
        //    values.Add(value);
        //}

        //// Generate the next 100 values in the range 5-10
        //for (int i = 0; i < 1000; i++)
        //{
        //    int value = UnityEngine.Random.Range(8, 11);
        //    values.Add(value);
        //}

        //for (int i = 0; i < 1000; i++)
        //{
        //    int value = UnityEngine.Random.Range(1, 6);
        //    values.Add(value);
        //}
        //for (int i = 0; i < 1000; i++)
        //{
        //    int value = UnityEngine.Random.Range(8, 11);
        //    values.Add(value);
        //}
        //double minValue = 1;
        //double maxValue = 20;
        //int numValues = 100; // Number of values to generate

        //double amplitude = (maxValue - minValue) / 2;
        //double offset = minValue + amplitude;

        //for (int i = 0; i < numValues; i++)
        //{
        //    double t = i * (Math.PI / (numValues - 1)); // Scale the index to the range [0, π]
        //    double value = offset + amplitude * Math.Sin(t); // Calculate the value using the sine function
        //    sinWavevalues.Add(value); 
        //}

    }

    void Update() 
    {
        string serverAddress = "127.0.0.1"; // Replace with the server IP if needed
        int port = 12345; // Replace with the port number you are using

        try
        {
            // Create a TcpClient.
            
            using (TcpClient client = new TcpClient(serverAddress, port))
            {
                //Console.WriteLine("Connected to server.");
                Debug.Log("Connected");
                // Get the stream used to read data from the server.
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = new byte[1024];
                    int bytesRead;

                    // Read data from the server.
                    while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                    {
                        string responseData = Encoding.ASCII.GetString(data, 0, bytesRead);
                        Console.WriteLine($"Received data: {responseData}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        //for (int i = 0; i < 1000; i++)
        //{
        //    int value = UnityEngine.Random.Range(1, 6);
        //    values.Add(value);
        //}

        //// Generate the next 100 values in the range 5-10
        //for (int i = 0; i < 1000; i++)
        //{
        //    int value = UnityEngine.Random.Range(8, 11);
        //    values.Add(value);
        //}
        //if (i < values.Count)
        //{
        //    randomValue = values[i++];
        //    Debug.Log(randomValue);

        //}

        //if (randomValue > 7)
        //{
        //    if (!isDecreasing)
        //    { 
        //        //StopCoroutine(fogDensityDecCoroutine);
        //        IncreaseFogDensity();
        //    }
        //    else
        //    {
        //        StopCoroutine(fogDensityDecCoroutine);
        //        isDecreasing = false;
        //        IncreaseFogDensity();
        //    }
        //}
        //else if (randomValue < 7)
        //{
        //    if (!isIncreasing)
        //    {
        //        //StopCoroutine(fogDensityIncCoroutine);
        //        DecreaseFogDensity();
        //    }
        //    else
        //    {
        //        StopCoroutine(fogDensityIncCoroutine);
        //        isIncreasing = false;
        //        DecreaseFogDensity();
        //    }
        //}

        //if (Input.GetKey(KeyCode.A))
        //{
        //    StopCoroutine(fogDensityIncCoroutine);
        //    isIncreasing = false;
        //    StopCoroutine(fogDensityDecCoroutine);
        //    isDecreasing = false;
        //}
    }


    void IncreaseFogDensity()
    {
        //elapsedTime = Time.time - fogStartTime;
        //float fogFactor = elapsedTime / fogDuration;
        //fogDensity = Mathf.Lerp(0.0f, maxFogDensity, fogFactor);
        //RenderSettings.fogDensity = fogDensity;
        if (!isIncreasing)
        {
            fogDensityIncCoroutine = StartCoroutine(IncreaseFogDensityCoroutine());
            sliderIncCoroutine = StartCoroutine(IncreaseSliderValueCoroutine());


        }
        else {
            Debug.Log("Running acc to logic");
        }
    }

    void DecreaseFogDensity()
    {
        //RenderSettings.fogDensity -= fogDensityStep;
        //if (RenderSettings.fogDensity < 0f)
        //{
        //    RenderSettings.fogDensity = 0f;
        //}

        if (!isDecreasing)
        {
            fogDensityDecCoroutine = StartCoroutine(DecreaseFogDensityCoroutine());
        }
    }

    IEnumerator DecreaseFogDensityCoroutine()
    {
        isDecreasing = true;

        float startTime = Time.time;
        float startDensity = RenderSettings.fogDensity;

        while (RenderSettings.fogDensity > 0f)
        {
            Debug.Log("Decreasing fog");
            float elapsedTime = Time.time - startTime;
            float t = elapsedTime / fogDecreaseTime * 0.1f;
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, 0f, t);
            yield return null;
        }

        isDecreasing = false;

    }

    IEnumerator IncreaseFogDensityCoroutine()
    {
        isIncreasing = true;
        if (incCount > 0)
        {
            fogStartTime = Time.time;  //resetting the fog to 0
        }


        float startDensity = RenderSettings.fogDensity;

        while (RenderSettings.fogDensity < 0.03f)
        {
            //Debug.Log("Increasing fog = " + RenderSettings.fogDensity);
            elapsedTime = Time.time - fogStartTime;
            float fogFactor = elapsedTime / fogDuration;
            fogDensity = Mathf.Lerp(0f, maxFogDensity, fogFactor);
            RenderSettings.fogDensity = fogDensity;
            yield return null;
        }
        Debug.Log("In increasing coroutine");
        incCount++;
        isIncreasing = false;

    }

    IEnumerator IncreaseSliderValueCoroutine() {
        timer = 0f;
        
        while (mySlider.value < 7.0f) {
            //timer += Time.deltaTime;

            mySlider.value += mySlider.value * Time.deltaTime * 0.3f;
            Debug.Log("INSIDE SLIDER COROUTINE " + mySlider.value);
            yield return null;  
        }

    }

    //public void setupSocket() {
    //    string host = "127.0.0.1";
    //    int port = 12345;

    //    serverSocket = new TcpListener(IPAddress.Parse(host), port);
    //    serverSocket.Start();

    //    Console.WriteLine($"Server is listening on {host}:{port}");


    //}


}



//private void Update()
//{
//    while (true)
//    {
//        // Add fog for the specified duration
//        RenderSettings.fogDensity = fogDensity;;
//        elapsedTime += Time.deltaTime;
//        if (elapsedTime >= fogDuration)
//        {
//            elapsedTime = 0.0f;
//            break;
//        }

//        // Clear the fog and wait for the specified duration before adding fog again
//        RenderSettings.fogDensity = fogClearDensity;
//        elapsedTime += Time.deltaTime;
//        if (elapsedTime >= fogDuration)
//        {
//            elapsedTime = 0.0f;
//            break;
//        }
//    }
//}