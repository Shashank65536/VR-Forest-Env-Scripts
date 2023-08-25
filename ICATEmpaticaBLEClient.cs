/* -- ICAT's Empatica Bluetooth Low Energy(BLE) Comm Client -- *
 * ----------------------------------------------------------- *
 * 0. Attach this to main camera or any empty game object
 * 1. On launch, it tries to connect to the localhost/port20 
 * 	  (You have to change it to your own ip/port combination).
 * 2. Enter the Device ID and connect to device.
 * 3. Select the data streams to log and hit "Log Data"
 * 4. Hit Ctrl+Shift+Z to disconnect at anytime.
 * 
 * Written By: Deba Saha (dpsaha@vt.edu)
 * Virginia Tech, USA.  */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class ICATEmpaticaBLEClient : MonoBehaviour
{
    //variables	
    private TCPConnection myTCP;
    private string streamSelected;
    public string msgToServer;
    public string connectToServer;

    private string savefilename = "name" + DateTime.UtcNow.ToString("dd_mm_yyyy_hh_mm_ss") + ".txt";

    //flag to indicate device conection status
    private bool deviceConnected = false;

    //flag to indicate if data to be logged to file
    private bool logToFile = false;

    private Image fill;
    //******************************************************************//
    //// Start is called before the first frame update
    //public float startDensity = 0.01f;
    //public float targetDensity = 0.002f;
    //public float changeSpeed = 0.0001f;
    public float rotationSpeed = 10f; // The speed at which to rotate the light

    //private bool increasing = true;
    //public float twoSecondsTimer = 6.0f;
    //private float timer = 0.0f;

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
    void Awake()
    {
        //add a copy of TCPConnection to this game object		
        myTCP = gameObject.AddComponent<TCPConnection>();
    }

    void Start()
    {

        canvas = GetComponentInChildren<Canvas>();
        mySlider = canvas.GetComponentInChildren<Slider>();
        //Debug.Log(mySlider == null);
        mySlider.enabled = true;
        mySlider.maxValue = 10f;
        mySlider.minValue = 1f;

        mySlider.value = vSliderValue1;


        // Get the background component of the slider
        background = canvas.GetComponentInChildren<Slider>().targetGraphic as Image;
        //backgroundAreaImage = canvas.GetComponentInChildren<Background>().targetGraphic as Image;

        Transform t = mySlider.transform.Find("Background");
        Image i = t.GetComponent<Image>();

        i.color = Color.red;

        //Get Fill Area 
        Transform fillAreaT = mySlider.transform.Find("Fill Area");
        Transform fillT = fillAreaT.transform.Find("Fill");
        Image fillImage = fillT.GetComponent<Image>();
        fillImage.color = Color.green;
        //Image fillImage = fill.GetComponent<Image>();


        background.color = Color.red;
        //backgroundAreaImage.color = Color.red;
        vSliderValue2 = 1.0f;
        //DisplayTimerProperties ();
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.0f;
        if (myTCP.socketReady == false)
        {
            Debug.Log("Attempting to connect..");
            //Establish TCP connection to server
            myTCP.setupSocket();
        }

        defaultSliderCoroutine = StartCoroutine(IncreaseDefaultSliderCoroutine());

        //sliderC.interactable = false;


    }

    void Update()
    {
        //keep checking the server for messages, if a message is received from server, 
        //it gets logged in the Debug console (see function below)		

        float responseValue = SocketResponse();
        
        if(responseValue > 0)
        {
            Debug.Log(responseValue);
            if (responseValue > 0.0f)
            {
                if (responseValue > 80)
                {
                    Debug.Log("res is  = " + responseValue);
                    if (!isDecreasing)
                    {
                        //StopCoroutine(fogDensityDecCoroutine);
                        IncreaseFogDensity();
                    }
                    else
                    {
                        StopCoroutine(fogDensityDecCoroutine);
                        isDecreasing = false;
                        StartCoroutine(IncreaseThenDecreaseFogDensity());
                    }
                }
                else if (responseValue < 60)
                {
                    Debug.Log("res is  = " + responseValue);
                    if (!isIncreasing)
                    {
                        //StopCoroutine(fogDensityIncCoroutine);
                        DecreaseFogDensity();
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

    void OnGUI()
    {
        //if connection has not been made, display button to connect		
        if (myTCP.socketReady == false)
        {
            if (GUILayout.Button("Connect"))
            {
                Debug.Log("Attempting to connect..");
                //Establish TCP connection to server
                myTCP.setupSocket();
            }
        }

        //once TCP connection has been made, connect to Empatica device
        if (myTCP.socketReady == true && deviceConnected == false)
        {
            if (GUILayout.Button("Device List"))
            {
                // ask to pupulate device list
                SendToServer("device_list");
            }
            connectToServer = GUILayout.TextField(connectToServer);
            if (GUILayout.Button("Connect to Device", GUILayout.Height(30)))
            {
                SendToServer("device_connect " + connectToServer);
                Debug.Log("Connected to Empatica. Press Ctrl+Shift+Z to disconnect Empatica at any time");
            }
        }

        //once device has been connected, choose which streams to select and start logging	
        if (myTCP.socketReady == true && deviceConnected == true && logToFile == false)
        {
            msgToServer = GUILayout.TextField(msgToServer);
            if (GUILayout.Button("Write to server", GUILayout.Height(30)))
            {
                SendToServer(msgToServer);
            }

            //Buttons for selecting data streams
            streamSelected = GUILayout.TextField(streamSelected);
            if (GUILayout.Button("Galvanic Skin Response"))
            {
                SendToServer("device_subscribe gsr ON");
                streamSelected += "GSR ";
            }
            if (GUILayout.Button("Accelerometer"))
            {
                SendToServer("device_subscribe acc ON");
                streamSelected += "ACC ";
            }
            if (GUILayout.Button("Blood Volume Pulse"))
            {
                SendToServer("device_subscribe bvp ON");
                streamSelected += "BVP ";
            }
            if (GUILayout.Button("Inter Beat Interval"))
            {
                SendToServer("device_subscribe ibi ON");
                streamSelected += "IBI ";
            }
            if (GUILayout.Button("Skin Temperature"))
            {
                SendToServer("device_subscribe tmp ON");
                streamSelected += "TMP ";
            }

            //Button for logging data to file
            if (GUILayout.Button("Log Data"))
            {
                logToFile = true;
                Debug.Log("Started Logging data. Press Ctrl+Shift+Z to disconnect Empatica at any time");
            }
        }

        //button combination for disconnecting
        Event e = Event.current;
        if (myTCP.socketReady == true && deviceConnected == true)
        {
            if (e.type == EventType.KeyDown && e.control && e.shift && e.keyCode == KeyCode.Z)
            {
                Debug.Log("Disconnecting Device and TCP connection...");
                //disconnect Empatica
                SendToServer("device_disconnect");
                //disconnect TCP
                myTCP.closeSocket();

                //reset all flags
                deviceConnected = false;
                logToFile = false;
                streamSelected = "";
            }
        }

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float sliderWidth = 100f;
        float sliderHeight = 100f;
        float textBoxFontSize = 30f; // Set the font size for the text box
        float padding = 10f;

        float xPos = screenWidth - sliderWidth - padding - 100f;
        float yPos = padding;

        GUI.backgroundColor = Color.green; // Set the background color for the first slider
        vSliderValue1 = GUI.VerticalSlider(new Rect(xPos, yPos, sliderWidth, sliderHeight), vSliderValue1, 10.0f, 1.0f);

        // Create a text box around the first slider
        float textBoxYPos = yPos + (sliderHeight - textBoxFontSize) / 2f;
        string textBoxText = "User's State";
        float textBoxWidth = GUI.skin.textField.CalcSize(new GUIContent(textBoxText)).x;
        float textBoxXPos = xPos - textBoxWidth - padding;
        GUI.TextField(new Rect(textBoxXPos, textBoxYPos, textBoxWidth, textBoxFontSize), textBoxText);

        // Update yPos and xPos for the second slider
        yPos = padding;
        xPos -= sliderWidth + padding;

        GUI.backgroundColor = Color.red; // Set the background color for the second slider
        vSliderValue2 = GUI.VerticalSlider(new Rect(xPos, yPos, sliderWidth, sliderHeight), vSliderValue2, 10.0f, 1.0f);

        // Create a text box around the second slider
        textBoxYPos = yPos + (sliderHeight - textBoxFontSize) / 2f;
        textBoxText = "Ideal state";
        textBoxWidth = GUI.skin.textField.CalcSize(new GUIContent(textBoxText)).x;
        textBoxXPos = xPos - textBoxWidth - padding;
        GUI.TextField(new Rect(textBoxXPos, textBoxYPos, textBoxWidth, textBoxFontSize), textBoxText);



    }


    //socket reading script	
    float SocketResponse()
    {
        string serverSays = myTCP.readSocket();
        float value = 0.0f;
        if (serverSays != "")
        {
            //Debug.Log("SERVERSAYS IS: = " + serverSays);
            string[] words = Regex.Split(serverSays, @"\s+");

            try
            {
                value = float.Parse(words[2]);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                value = 0f;
            }

            if (myTCP.socketReady == true && deviceConnected == true && logToFile == true)
            {
                //streamwriter for writing to file
                using (StreamWriter sw = File.AppendText(savefilename))
                    {
                        sw.WriteLine(serverSays);
                    }
                return value;
            }
            
            else
            {
                //Debug.Log("[SERVER]" + serverSays);
                string serverConnectOK = @"R device_connect OK";
                //Check if server response was device_connect OK
                if (string.CompareOrdinal(Regex.Replace(serverConnectOK, @"\s", ""), Regex.Replace(serverSays.Substring(0, serverConnectOK.Length), @"\s", "")) == 0)
                {
                    deviceConnected = true;
                }
            }
            
        }
        return value;
    }

    //send message to the server	
    public void SendToServer(string str)
    {
        myTCP.writeSocket(str);
        Debug.Log("[CLIENT] " + str);
    }

    //Method To check Stopwatch properties
    void DisplayTimerProperties()
    {
        // Display the timer frequency and resolution.
        if (Stopwatch.IsHighResolution)
        {
            Debug.Log("Operations timed using the system's high-resolution performance counter.");
        }
        else
        {
            Debug.Log("Operations timed using the DateTime class.");
        }

        long frequency = Stopwatch.Frequency;
        Debug.Log(string.Format("Timer frequency in ticks per second = {0}", frequency));
        long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
        Debug.Log(string.Format("Timer is accurate within {0} nanoseconds", nanosecPerTick));
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

            sliderIncCoroutine = StartCoroutine(IncreaseSliderCoroutine());

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
            sliderDecCoroutine = StartCoroutine(DecreaseSliderCoroutine());
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
        while (vSliderValue1 > 0 && vSliderValue1 < 9f)
        {
            Debug.Log("inside inc coroutine while loop" + vSliderValue1);
            //elapsedTime = Time.time - sliderStartTime;
            vSliderValue1 += vSliderValue1 * Time.deltaTime * 0.5f;
            mySlider.value = vSliderValue1;
            if (vSliderValue1 > 5.5)
            {
                background.color = Color.red;
            }
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

    IEnumerator DecreaseSliderCoroutine()
    {
        float sliderStartTime = Time.time;
        //Debug.Log("Inside the coroutine" + vSliderValue1);
        while (vSliderValue1 > 1 && vSliderValue1 != 1.0f)
        {
            Debug.Log("inside decrease coroutine while loop" + vSliderValue1);
            vSliderValue1 -= vSliderValue1 * Time.deltaTime * 0.03f;
            mySlider.value = vSliderValue1;
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
        while (vSliderValue1 > 0 && vSliderValue1 < 5f)
        {
            //Debug.Log("inside inc coroutine while loop" + vSliderValue2);
            //elapsedTime = Time.time - sliderStartTime;
            vSliderValue1 += vSliderValue1 * Time.deltaTime;
            mySlider.value = vSliderValue1;
            //float increaseFactor = elapsedTime / fogDuration;
            //vSliderValue1 = Mathf.Lerp(0f,6f,increaseFactor);
            yield return null;
        }
        //Debug.Log("Outside the while loop");
    }

}

