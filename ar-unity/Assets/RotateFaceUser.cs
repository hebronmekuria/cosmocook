using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateFaceUser : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (mainCameraTransform != null)
        {
            Vector3 directionToCamera = mainCameraTransform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
        else
        {
            Debug.LogWarning("Main camera not found in the scene!");
        }
    }
}

