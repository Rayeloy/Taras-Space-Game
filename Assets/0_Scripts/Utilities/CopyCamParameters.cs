using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCamParameters : MonoBehaviour
{
    public Camera cameraToCopy;
    Camera myCam;

    private void Start()
    {
        myCam = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        myCam.fieldOfView = cameraToCopy.fieldOfView;
    }
}
