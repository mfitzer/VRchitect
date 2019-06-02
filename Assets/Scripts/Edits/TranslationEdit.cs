using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationEdit : TransformationEdit
{
    public Vector3 position { get; private set; }

    //Creates and returns a new Edit
    public TranslationEdit(Transform transformEdited, Vector3 oldVector, Vector3 newVector, bool isTransformTool = false)
    {
        this.transformEdited = transformEdited;
        this.oldVector = oldVector;
        this.newVector = newVector;

        position = transformEdited.position;

        //Indicate if this Edit is editing the transform tool
        this.isTransformTool = isTransformTool;
    }

    //Perform the edit using the specified vector
    public override void execute()
    {
        performEdit(newVector);
    }

    void performEdit(Vector3 vectorToUse)
    {
        transformEdited.position = vectorToUse;

        Debug.Log("Performed edit: " + toString());
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
        return "Edit Type: Translation | Transform Edited: " + transformEdited.name + " | Old Vector: " + oldVector + " | New Vector: " + newVector;
    }
}
