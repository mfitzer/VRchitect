using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransformationEdit : Edit
{
    public Transform transformEdited { get; protected set; }

    public Vector3 oldVector { get; protected set; }

    public Vector3 newVector { get; protected set; }

    public bool isTransformTool { get; protected set; }
}
