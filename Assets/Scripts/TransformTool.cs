using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTool : MonoBehaviour
{
    private static TransformTool instance;
    public static TransformTool Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<TransformTool>();
            return instance;
        }
    }

    Quaternion initialRotation; //Default rotation of transform tool

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        initialRotation = transform.rotation;
    }

    //Resets the transform tool to its initial state
    public void resetTransformTool()
    {
        transform.rotation = initialRotation;
    }
}
