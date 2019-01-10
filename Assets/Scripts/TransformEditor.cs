﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEditor : MonoBehaviour {

    public enum Axis { x, y, z } //Indicates an axis on a transform

    EditTracker transformEditingEditTracker; //Stores edits for transforms edited
    EditTracker transformToolEditTracker; //Stores edits for transformTools

    Vector3 initialTransformEditingEditVector; //Initial vector value for transform edit
    Vector3 finalTransformEditingEditVector; //Final vector value for transformEditing
    Vector3 initialTransformToolEditVector; //Initial vector value for transformTool
    Vector3 finalTransformToolEditVector; //Final vector value for transformToolEditVector

    //Transformation
    Transform targetTransform;
    Vector3 initialControllerPosition; //Position of controller when translation is initiated in local space of transformEditing
    Vector3 initialPosition;
    Vector3 initialRotation;
    Axis targetAxis;
    Vector3 targetAxisVector; //Holds a Vector3 representation of the targetAxis
    Vector3 worldTargetAxisVector; //Holds the targetAxisVector in the local space of the transformTool
    EditTracker.EditType editType;
    bool shouldTransform = false;

    //Rotation
    Quaternion initialTransformEditingRotation;
    Quaternion initialTransformToolRotation;
    Vector3 initialVectorToController;
    Vector3 prevDirectionToRotateTo;

    //Scale
    Vector3 initialTransformEditingScale;
    Vector3 posOffsetDirection;

    // Use this for initialization
    void Start()
    {
        transformEditingEditTracker = new EditTracker();
        transformToolEditTracker = new EditTracker();
    }

    #region Translation

    //Translates transformTool on the x, y, or z axis; Stores transformToRecord for making and edit when the translation is done
    public void translate(Transform transformTool, Transform transformEditing, Translator translator)
    {
        if (!shouldTransform) //Not constraining the transform yet
        {
            initialTransformEditingEditVector = transformEditing.position;
            initialTransformToolEditVector = transformTool.position;

            targetTransform = transformTool;

            Vector3 transformEditingScale = transformEditing.localScale; //Store scale of transformEditing
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale
            initialControllerPosition = transformEditing.InverseTransformPoint(transform.position);
            transformEditing.localScale = transformEditingScale; //Reset the scale

            initialPosition = transformTool.position;
            initialRotation = transformTool.rotation.eulerAngles;
            targetAxis = translator.axis;
            targetAxisVector = getVectorForAxis(targetAxis);

            editType = EditTracker.EditType.Translation;
            shouldTransform = true;
        }
        else //Ready to transform
        {
            Vector3 transformEditingScale = transformEditing.localScale; //Store scale of transformEditing
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale

            Vector3 localSpaceControllerPosition = transformEditing.InverseTransformPoint(transform.position); //The position of the controller in the local space of transformEditing
            float unitsToTranslate = 0f;

            switch (targetAxis)
            {
                case Axis.x:
                    unitsToTranslate = localSpaceControllerPosition.x - initialControllerPosition.x;
                    break;
                case Axis.y:
                    unitsToTranslate = localSpaceControllerPosition.y - initialControllerPosition.y;
                    break;
                case Axis.z:
                    unitsToTranslate = localSpaceControllerPosition.z - initialControllerPosition.z;
                    break;
            }

            Vector3 translation = targetAxisVector * unitsToTranslate;
            transformEditing.Translate(translation, Space.Self);
            transformTool.Translate(translation); //Move transformTool with transformEditing

            transformEditing.localScale = transformEditingScale; //Reset the scale

            //Record final edit vectors for the EditTracker
            finalTransformEditingEditVector = transformEditing.position;
            finalTransformToolEditVector = transformTool.position;

            Debug.Log("Translation on " + targetAxis + "-axis | Initial: " + initialControllerPosition + " | Current: " + localSpaceControllerPosition + " | Units: " + unitsToTranslate + " | translation: " + translation + " | new pos: " + transformEditing.position);
        }
    }

    #endregion Translation

    #region Rotation

    //Translates the given transform on the x, y, or z axis by following the 
    public void rotate(Transform transformTool, Transform transformEditing, Rotator rotator)
    {
        if (!shouldTransform) //Not ready to rotate, need to setup variables
        {
            initialTransformEditingEditVector = transformEditing.rotation.eulerAngles;
            initialTransformToolEditVector = transformTool.rotation.eulerAngles;

            initialTransformEditingRotation = transformEditing.rotation;
            initialTransformToolRotation = transformTool.rotation;

            targetAxis = rotator.axis;
            targetAxisVector = getVectorForAxis(targetAxis);
            worldTargetAxisVector = transformTool.TransformVector(targetAxisVector);
            editType = EditTracker.EditType.Rotation;

            shouldTransform = true;

            //Assuming this is attached to the controller: Vector between controller position and transformTool position
            initialVectorToController = Vector3.ProjectOnPlane(transform.position - transformTool.position, worldTargetAxisVector); //Record initial relationship between controller and transformTool
        }
        else //Ready to rotate
        {
            //Assuming this is attached to the controller: Vector between controller position and transformTool position
            Vector3 newVectorToController = Vector3.ProjectOnPlane(transform.position - transformTool.position, worldTargetAxisVector); //The vector to the controller in the plane of rotation specified by the target axis

            //Transform Editing
            float degreesToRotate = Vector3.SignedAngle(initialVectorToController, newVectorToController, worldTargetAxisVector); //Number of degrees to rotate on the target axis

            //Perform rotations
            transformTool.rotation = initialTransformToolRotation * Quaternion.AngleAxis(degreesToRotate, targetAxisVector); //Rotate degreesToRotate around the target axis from transformTool's initial rotation
            transformEditing.rotation = initialTransformEditingRotation * Quaternion.AngleAxis(degreesToRotate, targetAxisVector); //Rotate degreesToRotate around the target axis from transformEditing's initial rotation

            //Record final edit vectors for the EditTracker
            finalTransformEditingEditVector = transformEditing.rotation.eulerAngles;
            finalTransformToolEditVector = transformTool.rotation.eulerAngles;
        }
    }

    #endregion Rotation

    #region Scale

    public void scale(Transform transformTool, Transform transformEditing, Scaler scaler)
    {
        if (!shouldTransform) //Not ready to rotate, need to setup variables
        {
            initialTransformEditingEditVector = transformEditing.localScale;
            initialTransformToolEditVector = transformTool.localScale;

            initialTransformEditingScale = transformEditing.localScale;
            initialPosition = transformEditing.localPosition;
            
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale
            initialControllerPosition = transformEditing.InverseTransformPoint(transform.position); //In local space
            transformEditing.localScale = initialTransformEditingScale; //Reset the scale

            targetAxis = scaler.axis;
            targetAxisVector = getVectorForAxis(targetAxis);
            editType = EditTracker.EditType.Scale;

            //Determine direction in which the position should be offset
            switch (targetAxis)
            {
                case Axis.x:
                    posOffsetDirection = transformEditing.right;
                    break;
                case Axis.y:
                    posOffsetDirection = transformEditing.up;
                    break;
                case Axis.z:
                    posOffsetDirection = transformEditing.forward;
                    break;
            }

            shouldTransform = true;
        }
        else //Ready to scale
        {
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale

            Vector3 localSpaceControllerPosition = transformEditing.InverseTransformPoint(transform.position); //The position of the controller in the local space of transformEditing
            float unitsToScale = 0f;
            Vector3 newScale = initialTransformEditingScale;

            if (scaler.allAxisScaler) //Scale on all axes
            {
                //NEED TO FIX THIS!! CAN'T CURRENTLY SCALE DOWN
                unitsToScale = (localSpaceControllerPosition - initialControllerPosition).magnitude;
                newScale = initialTransformEditingScale + Vector3.one * unitsToScale;
            }
            else //Scale on one axis
            {
                switch (targetAxis)
                {
                    case Axis.x:
                        unitsToScale = localSpaceControllerPosition.x - initialControllerPosition.x;
                        break;
                    case Axis.y:
                        unitsToScale = localSpaceControllerPosition.y - initialControllerPosition.y;
                        break;
                    case Axis.z:
                        unitsToScale = localSpaceControllerPosition.z - initialControllerPosition.z;
                        break;
                }

                newScale = initialTransformEditingScale + targetAxisVector * unitsToScale;

                //Offset position
                Debug.Log("Target axis: " + targetAxis);
                float unitsToTranslate = unitsToScale / 2;
                Vector3 posOffset = unitsToTranslate * posOffsetDirection;
                Vector3 newPosition = initialPosition + posOffset;
                transformEditing.localPosition = newPosition;
            }

            transformEditing.localScale = newScale;

            //Record final edit vectors for the EditTracker
            finalTransformEditingEditVector = transformEditing.localScale;
            finalTransformToolEditVector = transformTool.localScale;
        }
    }

    #endregion Scale

    //Performs the extra actions for the active transformation
    void performTransformation()
    {
        switch (editType)
        {
            case EditTracker.EditType.Translation:

                break;
            case EditTracker.EditType.Rotation:

                break;
            case EditTracker.EditType.Scale:

                break;
        }
    }

    //Contrains a transform's movement to a specific axis
    void constrainTransform(Transform transformToConstrain, Vector3 position, Vector3 rotation, Axis axis, EditTracker.EditType typeOfEdit)
    {
        Vector3 newPosition = position;
        Vector3 newRotation = rotation;

        switch (targetAxis) //Assuming this script goes on the controller, match position and rotation of controller on the specified axis
        {
            case Axis.x:
                newPosition = new Vector3(transformToConstrain.position.x, position.y, position.z);
                newRotation = new Vector3(transformToConstrain.rotation.eulerAngles.x, position.y, position.z);
                break;
            case Axis.y:
                newPosition = new Vector3(position.x, transformToConstrain.position.y, position.z);
                newRotation = new Vector3(position.x, transformToConstrain.rotation.eulerAngles.y, position.z);
                break;
            case Axis.z:
                newPosition = new Vector3(position.x, initialPosition.y, transformToConstrain.position.z);
                newRotation = new Vector3(position.x, initialPosition.y, transformToConstrain.rotation.eulerAngles.z);
                break;
        }

        switch (typeOfEdit) //Determine type of edit being made
        {
            case EditTracker.EditType.Translation:
                transformToConstrain.position = newPosition;
                transformToConstrain.rotation = Quaternion.Euler(rotation);
                finalTransformEditingEditVector = transformToConstrain.position; //Track position of transformEditing in case the transformation is done
                break;
        }
    }

    //Sets a flag that indicates that transformation should stop
    public void stopTransforming(Transform transformTool, Transform transformEditing)
    {
        shouldTransform = false;

        //Record edit info
        transformEditingEditTracker.makeEdit(editType, transformEditing, initialTransformEditingEditVector, finalTransformEditingEditVector);
        transformToolEditTracker.makeEdit(editType, transformTool, initialTransformToolEditVector, initialTransformToolEditVector);
    }

    #region Helpers

    //Returns a vector representation of the given Axis
    Vector3 getVectorForAxis(Axis axis)
    {
        return new Vector3(axis.Equals(Axis.x) ? 1f : 0f, axis.Equals(Axis.y) ? 1f : 0f, axis.Equals(Axis.z) ? 1f : 0f);
    }

    //Returns a 2D vector from origin to positionLookingAt in the 2D plane excluding the given axis
    Vector3 get2DDirectionalVector(Vector3 origin, Vector3 positionLookingAt, Axis axisToZeroOut)
    {
        switch (axisToZeroOut)
        {
            case Axis.x:
                positionLookingAt = new Vector3(origin.x, positionLookingAt.y, positionLookingAt.z);
                break;
            case Axis.y:
                positionLookingAt = new Vector3(positionLookingAt.x, origin.y, positionLookingAt.z);
                break;
            case Axis.z:
                positionLookingAt = new Vector3(positionLookingAt.x, positionLookingAt.y, origin.z);
                break;
        }

        return positionLookingAt - origin;
    }

    #endregion Helpers

    #region General Events

    //Handles the event of an undo edit event
    public Transform handleEditTrackerUndo()
    {
        transformToolEditTracker.undo();
        return transformEditingEditTracker.undo();
    }

    //Handles the event of an undo edit event
    public Transform handleEditTrackerRedo()
    {
        transformToolEditTracker.redo();
        return transformEditingEditTracker.redo();
    }

    #endregion General Events

    private void FixedUpdate()
    {
        performTransformation();
    }
}
