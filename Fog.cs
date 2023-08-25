using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    public float fogDensity = 0.01f;

    void Start()
    {
        RenderSettings.fogDensity = fogDensity;
    }

    void update() {

        Debug.Log("In update");
        Fog fogController = FindObjectOfType<Fog>();
        fogController.SetFogDensity(0.5f);
    }

    public void SetFogDensity(float newDensity)
    {
        fogDensity = newDensity;
        RenderSettings.fogDensity = fogDensity;
    }
}
