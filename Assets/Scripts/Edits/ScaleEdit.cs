using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEdit : TransformationEdit
{
    public Vector3 scale { get; private set; }

    //Creates and returns a new Edit
    public ScaleEdit(Transform transformEdited, Vector3 oldVector, Vector3 newVector, bool isTransformTool = false)
    {
        this.transformEdited = transformEdited;
        this.oldVector = oldVector;
        this.newVector = newVector;

        scale = transformEdited.localScale;

        //Indicate if this Edit is editing the transform tool
        this.isTransformTool = isTransformTool;
    }

    //Perform the edit using the specified vector
    public override void execute()
    {
        performEdit(newVector);
    }

    public override void undo()
    {
        performEdit(oldVector);
    }

    public override void redo()
    {
        execute();
    }

    //Returns a formatted string version of an edit
    public override string toString()
    {
        return "Edit Type: Scale | Transform Edited: " + transformEdited.name + " | Old Vector: " + oldVector + " | New Vector: " + newVector;
    }

    //Performs the edit with the given vector
    void performEdit(Vector3 vectorToUse)
    {
        if (transformEdited)
        {
            transformEdited.localScale = vectorToUse;

            Debug.Log("Performed edit: " + toString());
        }
        else
        {
            Debug.Log("TransformEdited is null");
        }
    }
}
