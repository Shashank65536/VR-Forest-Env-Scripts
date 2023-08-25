using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
public class ServerController : MonoBehaviour
{
    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private byte[] receiveBuffer = new byte[4096];

    // Define the port number for the server
    public int port = 25001;
    int count = 0;
    bool startFlag = true;
    private int messageCount = 0;  // To keep track of the messages
    private DateTime prevTime = new DateTime();  // To keep track of the last minute
    double sum = 0;
    double receivedDataFloat = 0;
    double result = 0;
    private List<double> receivedValues = new List<double>();
    double avgValue = 0;

    public float fogDensity = 0f;
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
    Coroutine sliderDecCoroutine;
    Coroutine defaultSliderCoroutine;
    Coroutine normalRangeSliderCoroutine;

    Coroutine incNormalRangeSliderCoroutine;
    List<int> values = new List<int>();
    List<double> sinWavevalues = new List<double>();
    int randomValue = 0;

    public Slider vSlider1;
    public Slider vSlider2;
    public float vSliderValue1 = 1.0f;
    public float vSliderValue2 = 1.0f;
    public Slider mySlider;
    Canvas canvas;
    private float timer;
    private Image background;
    private Image backgroundAreaImage;
    private Image fill;
    private void Start()
    {
        // Start the server on the specified port
        prevTime = DateTime.Now;
        StartServer();

        canvas = GetComponentInChildren<Canvas>();
        mySlider = canvas.GetComponentInChildren<Slider>();
        //Debug.Log(mySlider == null);
        mySlider.enabled = true;
        mySlider.maxValue = 10f;
        mySlider.minValue = 1f;

        mySlider.value = vSliderValue1;
        vSliderValue2 = 1.0f;
        //DisplayTimerProperties ();
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.0f;


        defaultSliderCoroutine = StartCoroutine(IncreaseDefaultSliderCoroutine());

        //sliderC.interactable = false;


    }

    void Update()
    {
        //keep checking the server for messages, if a message is received from server, 
        //it gets logged in the Debug console (see function below)		


        if (avgValue > 0)
        {
            Debug.Log(avgValue);
            if (avgValue > 0.0f)
            {
                if (avgValue >= 5.5 && avgValue <= 6.5)
                {
                    Debug.Log("exe reaching here");

                    if (mySlider.value > 5)
                    {
                        // if (normalRangeSliderCoroutine != null)
                        // {
                        //     Debug.Log("Stopping the coroutine");
                        //     StopCoroutine(normalRangeSliderCoroutine);
                        // }
                        normalRangeSliderCoroutine = StartCoroutine(NormalRangeSliderCoroutine());
                    }
                    else
                    {
                        if (incNormalRangeSliderCoroutine != null)
                        {
                            Debug.Log("Stopping the coroutine");
                            StopCoroutine(incNormalRangeSliderCoroutine);
                        }
                        incNormalRangeSliderCoroutine = StartCoroutine(IncNormalRangeSliderCoroutine());
                    }


                }
                else if (avgValue > 6.5)
                {
                    // Debug.Log("res is  = " + avgValue);
                    if (!isDecreasing)
                    {
                        //StopCoroutine(fogDensityDecCoroutine);
                        IncreaseFogDensity();
                        sliderIncCoroutine = StartCoroutine(IncreaseSliderCoroutine());
                    }
                    else
                    {
                        StopCoroutine(fogDensityDecCoroutine);
                        isDecreasing = false;
                        StartCoroutine(IncreaseThenDecreaseFogDensity());
                    }
                }
                else if (avgValue < 5.5)
                {
                    // Debug.Log("res is  = " + avgValue);
                    if (!isIncreasing)
                    {
                        //StopCoroutine(fogDensityIncCoroutine);
                        DecreaseFogDensity();
                        sliderDecCoroutine = StartCoroutine(DecreaseSliderCoroutine());
                    }
                    else
                    {
                        StopCoroutine(fogDensityIncCoroutine);
                        isIncreasing = false;
                        StartCoroutine(IncreaseThenDecreaseFogDensity());
                    }
                }
            }

        }

    }
    public void StartServer()
    {
        try
        {
            // Create a new TCP listener on the specified port
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Debug.Log("Server started. Waiting for connections...");

            // Begin accepting client connections asynchronously
            server.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting server: " + e.Message);
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        try
        {
            // Accept the client connection and get the client TcpClient object
            client = server.EndAcceptTcpClient(ar);
            Debug.Log("Client connected.");

            // Get the network stream from the client to send and receive data
            stream = client.GetStream();

            // Start listening for data from the client asynchronously
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error accepting client connection: " + e.Message);
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        if (startFlag)
        {
            prevTime = DateTime.Now;
            Debug.Log(DateTime.Now);
            startFlag = false;
        }

        try
        {
            // Get the number of bytes received
            int bytesRead = stream.EndRead(ar);

            if (bytesRead <= 0)
            {
                // The client disconnected
                Debug.Log("Client disconnected.");
                return;
            }
            // Convert the received bytes to a string
            string receivedData = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
            //Debug.Log("Received data: " + receivedData);
            messageCount++;


            // Use regular expression to extract the number
            string pattern = @"-?\d+\.\d+";
            Match match = Regex.Match(receivedData, pattern);
            if (match.Success)
            {
                string extractedValue = match.Value;
                if (double.TryParse(extractedValue, out result))
                {
                    //Debug.Log(result);
                }
                else
                {
                    Debug.Log("Faild to parse");
                    result = 0;
                }
            }
            else
            {
                Debug.Log("No match");
            }
            //convert to float
            //try
            //{
            //    receivedDataFloat = double.Parse(receivedData);
            //}
            //catch (Exception e)
            //{
            //    Debug.Log(e);
            //    receivedDataFloat = 0f;
            //}
            sum = sum + result;
            DateTime now = DateTime.Now;
            receivedValues.Add(result);
            if ((DateTime.Now - prevTime).TotalMinutes >= 0.25)
            {
                // Output the message count per minute
                // double avgValue = sum/ (double)messageCount;
                calcAvg();
                // Debug.Log("avg value is :" + avgValue);
                // Debug.Log($"Messages per minute: {messageCount}");
                // Debug.Log(DateTime.Now);
                // this.OnDestroy();
            }
            // Continue listening for data from the client
            stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, OnDataReceived, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }
    }

    private void calcAvg()
    {
        if (receivedValues.Count > 0)
        {
            avgValue = receivedValues.Average();
            Debug.Log($"avg value = {avgValue}");
        }
        else
        {
            Debug.Log("No values");
        }
        prevTime = DateTime.Now;
        receivedValues.Clear();
    }

    private void OnDestroy()
    {
        // Clean up resources when the server GameObject is destroyed
        stream?.Close();
        client?.Close();
        server?.Stop();
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

            // sliderIncCoroutine = StartCoroutine(IncreaseSliderCoroutine());

        }
        else
        {
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
            // sliderDecCoroutine = StartCoroutine(DecreaseSliderCoroutine());
        }
    }

    IEnumerator DecreaseFogDensityCoroutine()
    {
        isDecreasing = true;

        float startTime = Time.time;
        float startDensity = RenderSettings.fogDensity;

        while (RenderSettings.fogDensity > 0f)
        {
            // Debug.Log("Decreasing fog");
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
        //if (incCount > 0)
        //{
        //    fogStartTime = Time.time;  //resetting the fog to 0
        //}

        fogStartTime = Time.time;
        float startDensity = RenderSettings.fogDensity;

        while (RenderSettings.fogDensity < 0.05f)
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

    IEnumerator IncreaseThenDecreaseFogDensity()
    {
        yield return fogDensityIncCoroutine;

        DecreaseFogDensity();
    }

    IEnumerator IncreaseSliderCoroutine()
    {
        //Debug.Log("Inside the coroutine" + vSliderValue1);
        while (mySlider.value > 0 && mySlider.value < 9f)
        {
            // Debug.Log("inside inc coroutine while loop" + vSliderValue1);
            //elapsedTime = Time.time - sliderStartTime;
            mySlider.value += mySlider.value * Time.deltaTime * 0.5f;
            // mySlider.value = vSliderValue1;

            //float increaseFactor = elapsedTime / fogDuration;
            //vSliderValue1 = Mathf.Lerp(0f,6f,increaseFactor);
            yield return null;
        }
        timer = 0f;

        //while (mySlider.value < 7.0f)
        //{
        //    //timer += Time.deltaTime;

        //    mySlider.value += mySlider.value * Time.deltaTime * 0.3f;
        //    Debug.Log("INSIDE SLIDER COROUTINE " + mySlider.value);
        //    yield return null;
        //}
        //Debug.Log("Outside the while loop");
    }

    IEnumerator NormalRangeSliderCoroutine()
    {
        Debug.Log("Inside the normal range coroutine" + vSliderValue1);
        if (mySlider.value > 5.2)
        {

            isIncreasing = false;
            StartCoroutine(IncreaseThenDecreaseFogDensity());
            while (mySlider.value > 5.0f)
            {
                // Debug.Log("normal range slider decrease" + vSliderValue1);
                //elapsedTime = Time.time - sliderStartTime;
                mySlider.value -= mySlider.value * Time.deltaTime;
                mySlider.value = Mathf.Clamp(mySlider.value, 5.0f, 7f);
                yield return null;
            }
            // }else{
            //     while (mySlider.value > 0 && mySlider.value < 5)
            //     {
            //         Debug.Log("normal range slider increase" + mySlider.value);          
            //         mySlider.value += mySlider.value * Time.deltaTime * 0.5f;
            //         yield return null;
            //     }
            // }
        }
    }

    IEnumerator IncNormalRangeSliderCoroutine()
    {
        Debug.Log("Inside IncNormalRangeSliderCoroutine" + vSliderValue1);

        while (mySlider.value > 0 && mySlider.value < 5)
        {
            // Debug.Log("normal range slider increase" + mySlider.value);
            mySlider.value += mySlider.value * Time.deltaTime * 0.5f;
            yield return null;
        }

    }

    IEnumerator DecreaseSliderCoroutine()
    {
        float sliderStartTime = Time.time;
        //Debug.Log("Inside the coroutine" + vSliderValue1);
        while (mySlider.value > 1 && mySlider.value != 1.0f)
        {
            // Debug.Log("inside decrease coroutine while loop" + vSliderValue1);
            mySlider.value -= mySlider.value * Time.deltaTime * 0.09f;
            // mySlider.value = vSliderValue1;
            //elapsedTime = Time.time - sliderStartTime;
            //vSliderValue1 += vSliderValue1 * Time.deltaTime * 0.0005f;
            //float increaseFactor = elapsedTime / fogDuration;
            //vSliderValue1 = Mathf.Lerp(vSliderValue1, 0f, increaseFactor);
            yield return null;
        }
        Debug.Log("Outside the while loop");
    }


    IEnumerator IncreaseDefaultSliderCoroutine()
    {
        //Debug.Log("Inside the coroutine" + vSliderValue1);
        while (mySlider.value > 0 && mySlider.value < 5f)
        {
            //Debug.Log("inside inc coroutine while loop" + vSliderValue2);
            //elapsedTime = Time.time - sliderStartTime;
            mySlider.value += mySlider.value * Time.deltaTime;

            //float increaseFactor = elapsedTime / fogDuration;
            //vSliderValue1 = Mathf.Lerp(0f,6f,increaseFactor);
            yield return null;
        }
        //Debug.Log("Outside the while loop");
    }
}