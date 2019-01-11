using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edit
{
    public enum EditType { Translation, Rotation, Scale } //Indicates the type of an edit

    EditType editType;
    Transform transformEdited;
    Vector3 oldVector;
    Vector3 newVector;

    Vector3 position;
    Vector3 rotation;
    Vector3 scale;

    bool shouldTranslate = false;
    bool shouldRotate = false;
    bool shouldScale = false;

    bool isTransformTool;

    //Creates and returns a new Edit
    public Edit(EditType editType, Transform transformEdited, Vector3 oldVector, Vector3 newVector, bool isTransformTool = false)
    {
        this.editType = editType;
        this.transformEdited = transformEdited;
        this.oldVector = oldVector;
        this.newVector = newVector;

        position = transformEdited.position;
        rotation = transformEdited.rotation.eulerAngles;
        scale = transformEdited.localScale;

        switch (editType) //Determine the EditType
        {
            case EditType.Translation:
                shouldTranslate = true;
                break;
            case EditType.Rotation:
                shouldRotate = true;
                break;
            case EditType.Scale:
                shouldScale = true;
                break;
        }

        //Indicate if this Edit is editing the transform tool
        this.isTransformTool = isTransformTool;
    }

    //Perform the edit using the specified vector
    public void performEdit(EditTracker.EditAction editAction)
    {
        Vector3 vectorToUse;
        if (editAction.Equals(EditTracker.EditAction.Undo))
        {
            vectorToUse = oldVector;
        }
        else //Redo
        {
            vectorToUse = newVector;
        }

        Vector3 positionToUse;
        Quaternion rotationToUse;
        Vector3 scaleToUse;

        switch (editType) //Determine the EditType
        {
            case EditType.Translation:
                positionToUse = vectorToUse;
                rotationToUse = Quaternion.Euler(rotation);
                scaleToUse = scale;
                break;
            case EditType.Rotation:
                positionToUse = position;
                rotationToUse = Quaternion.Euler(vectorToUse);
                scaleToUse = scale;
                break;
            case EditType.Scale:
                positionToUse = position;
                rotationToUse = Quaternion.Euler(rotation);
                scaleToUse = vectorToUse;
                break;
            default:
                positionToUse = position;
                rotationToUse = Quaternion.Euler(rotation);
                scaleToUse = scale;
                break;
        }

        //Perform transformations
        if (shouldTranslate)
        {
            transformEdited.position = positionToUse;
        }
        if (shouldRotate)
        {
            transformEdited.rotation = rotationToUse;
        }
        if (shouldScale)
        {
            transformEdited.localScale = scaleToUse;
        }

        Debug.Log("Performed edit: " + formatAsString() + " shouldTranslate: " + shouldTranslate + " shouldRotate: " + shouldRotate + " shouldScale: " + shouldScale);
    }

    //Returns a formatted string version of an edit
    public string formatAsString()
    {
        return "Edit Type: " + editType + "  Transform Edited: " + transformEdited.name + "  Old Vector: " + oldVector + "  New Vector: " + newVector;
    }

    #region Getters

    public EditType getEditType()
    {
        return editType;
    }

    public Transform getTransformEdited()
    {
        return transformEdited;
    }

    public Vector3 getOldVector()
    {
        return oldVector;
    }

    public Vector3 getNewVector()
    {
        return newVector;
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public Vector3 getRotation()
    {
        return rotation;
    }

    public Vector3 getScale()
    {
        return scale;
    }

    public bool getIsTransformTool()
    {
        return isTransformTool;
    }

    #endregion Getters

    #region Setters

    public void setShouldTranslate(bool shouldTranslate)
    {
        this.shouldTranslate = shouldTranslate;
    }

    public void setShouldRotate(bool shouldRotate)
    {
        this.shouldRotate = shouldRotate;
    }

    public void setShouldScale(bool shouldScale)
    {
        this.shouldScale = shouldScale;
    }

    #endregion Setter
}
