using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationEdit : TransformationEdit
{
    public Vector3 rotation { get; private set; }

    //Creates and returns a new Edit
    public RotationEdit(Transform transformEdited, Vector3 oldVector, Vector3 newVector, bool isTransformTool = false)
    {
        this.transformEdited = transformEdited;
        this.oldVector = oldVector;
        this.newVector = newVector;

        rotation = transformEdited.rotation.eulerAngles;

        //Indicate if this Edit is editing the transform tool
        this.isTransformTool = isTransformTool;
    }

    //Perform the edit using the specified vector
    public override void execute()
    {
        performEdit(newVector);
    }

    //Undo the rotation edit
    public override void undo()
    {
        performEdit(oldVector);
    }

    //Redo the rotation edit
    public override void redo()
    {
        execute();
    }

    //Returns a formatted string version of an edit
    public override string toString()
    {
        return "Edit Type: Rotation | Transform Edited: " + transformEdited.name + " | Old Vector: " + oldVector + " | New Vector: " + newVector;
    }

    //Performs the edit with the given vector
    void performEdit(Vector3 vectorToUse)
    {
        if (transformEdited) //Transform edited is not null
        {
            transformEdited.rotation = Quaternion.Euler(vectorToUse);

            Debug.Log("Performed edit: " + toString());
        }
        else
        {
            Debug.Log("TransformEdited is null");
        }
    }
}
