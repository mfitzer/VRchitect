using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEditor : MonoBehaviour {

    public enum Axis { x, y, z } //Indicates an axis on a transform

    EditTracker editTracker;

    Transform transformEditing;
    Vector3 initialEditVector; //Initial vector value for transform edit
    Vector3 finalEditVector; //Final vector value for transform edit

    //Transformation
    Transform targetTransform;
    Vector3 initialPosition;
    Vector3 initialRotation;
    Axis targetAxis;
    EditTracker.EditType editType;
    bool shouldTransform = false;

    //Rotation
    Vector3 initialVectorToController;
    Vector3 previousControllerPosition; //Position of the controller last frame
    Vector3 prevDirectionToRotateTo;
    public float rotatorSensitivity = 1f; //Sensitivity of rotator tool

    // Use this for initialization
    void Start()
    {
        editTracker = FindObjectOfType<EditTracker>();
    }

    #region Translation

    //Translates transformToMove on the x, y, or z axis; Stores transformToRecord for making and edit when the translation is done
    public void translate(Transform transformToMove, Transform transformToRecord, Translator translator)
    {
        if (!shouldTransform) //Not constraining the transform yet
        {
            transformEditing = transformToRecord;
            initialEditVector = transformEditing.position;

            targetTransform = transformToMove;
            initialPosition = transformToMove.position;
            initialRotation = transformToMove.rotation.eulerAngles;
            targetAxis = translator.axis;

            editType = EditTracker.EditType.Translation;
            shouldTransform = true;
        }
    }

    #endregion Translation

    #region Rotation

    //Translates the given transform on the x, y, or z axis by following the 
    public void rotate(Transform transformToMove, Transform transformToRecord, Rotator rotator)
    {

        if (!shouldTransform) //Not ready to rotate, need to setup variables
        {
            transformEditing = transformToRecord;
            initialEditVector = transformEditing.rotation.eulerAngles;
            targetAxis = rotator.axis;
            editType = EditTracker.EditType.Rotation;
            shouldTransform = true;

            //Assuming this is attached to the controller: Vector between controller position and transformToMove position
            initialVectorToController = get2DDirectionalVection(transformToMove.position, transform.position, targetAxis); //Record initial relationship between controller and transformTool
        }
        else //Ready to rotate
        {            
            Vector3 transformToolToDirection = get2DDirectionalVection(transformToMove.position, transform.position, targetAxis); //Assuming this is attached to the controller: Vector between controller position and transformToMove position
            transformToMove.rotation = Quaternion.FromToRotation(initialVectorToController, transformToolToDirection);
            finalEditVector = transformEditing.rotation.eulerAngles;
        }
    }

    #endregion Rotation

    //Performs the extra actions for the active transformation
    void performTransformation()
    {
        switch (editType)
        {
            case EditTracker.EditType.Translation:
                constrainTransform(targetTransform, initialPosition, initialRotation, targetAxis, editType);
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
        if (shouldTransform)
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
                    finalEditVector = transformEditing.position; //Track position of transformEditing in case the transformation is done
                    break;
                //case EditTracker.EditType.Rotation:
                //    transformToConstrain.position = position;
                //    transformToConstrain.rotation = Quaternion.Euler(newRotation);
                //    finalEditVector = transformEditing.rotation.eulerAngles; //Track rotation of transformEditing in case the transformation is done
                //    break;
            }
        }
    }

    //Sets a flag that indicates that transformation should stop
    public void stopTransforming()
    {
        shouldTransform = false;
        editTracker.makeEdit(editType, transformEditing, initialEditVector, finalEditVector); //Record edit info
    }

    #region Helpers

    //Returns a vector representation of the given Axis
    Vector3 getVectorForAxis(Axis axis)
    {
        return new Vector3(axis.Equals(Axis.x) ? 1f : 0f, axis.Equals(Axis.y) ? 1f : 0f, axis.Equals(Axis.z) ? 1f : 0f);
    }

    //Returns a 2D vector from origin to positionLookingAt in the 2D plane excluding the given axis
    Vector3 get2DDirectionalVection(Vector3 origin, Vector3 positionLookingAt, Axis axisToZeroOut)
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

    private void FixedUpdate()
    {
        performTransformation();
    }
}
