using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLight : MonoBehaviour
{
    // Start is called before the first frame update
    public Light myLight;

    void Start()
    {
        myLight.color = Color.white;
        myLight.intensity = 1.5f;

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 20f);

    }
}
