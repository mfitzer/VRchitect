using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEditor : MonoBehaviour {

    public enum Axis { x, y, z } //Indicates an axis on a transform

    enum Octant { I, II, III, IV, V, VI, VII, VIII } //Specifies an octant in a 3d coordinate system

    EditTracker editTracker; //Stores edit information

    Vector3 initialTransformEditingEditVector; //Initial vector value for transform edit
    Vector3 finalTransformEditingEditVector; //Final vector value for transformEditing
    Vector3 initialTransformToolEditVector; //Initial vector value for transformTool
    Vector3 finalTransformToolEditVector; //Final vector value for transformToolEditVector

    //Transformation
    Vector3 initialControllerPosition; //Position of controller when translation is initiated in local space of transformEditing
    Vector3 initialPosition;
    Vector3 initialRotation;
    Axis targetAxis;
    Vector3 targetAxisVector; //Holds a Vector3 representation of the targetAxis
    Vector3 worldTargetAxisVector; //Holds the targetAxisVector in the local space of the transformTool
    Edit.EditType editType;
    bool shouldTransform = false;

    //Rotation
    Quaternion initialTransformEditingRotation;
    Quaternion initialTransformToolRotation;
    Vector3 initialVectorToController;
    Vector3 prevDirectionToRotateTo;

    //Scale
    Scaler scaler;
    Vector3 posOffsetDirection;
    Vector3 initialPositionEditVector;
    Vector3 finalPositionEditVector;
    Vector3 initialWorldControllerPosition;

    // Use this for initialization
    void Start()
    {
        editTracker = new EditTracker();
    }

    #region Translation

    //Translates transformTool on the x, y, or z axis; Stores transformToRecord for making and edit when the translation is done
    public void translate(Transform transformTool, Transform transformEditing, Translator translator)
    {
        if (!shouldTransform) //Not constraining the transform yet
        {
            initialTransformEditingEditVector = transformEditing.position;
            initialTransformToolEditVector = transformTool.position;

            Vector3 transformEditingScale = transformEditing.localScale; //Store scale of transformEditing
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale
            initialControllerPosition = transformEditing.InverseTransformPoint(transform.position);
            transformEditing.localScale = transformEditingScale; //Reset the scale

            initialPosition = transformTool.position;
            initialRotation = transformTool.rotation.eulerAngles;
            targetAxis = translator.axis;
            targetAxisVector = getVectorForAxis(targetAxis);

            editType = Edit.EditType.Translation;
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
            editType = Edit.EditType.Rotation;

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
        if (!shouldTransform) //Not ready to scale, need to setup variables
        {
            initialTransformEditingEditVector = transformEditing.localScale;
            initialTransformToolEditVector = transformTool.localScale;
            
            initialPosition = transformEditing.localPosition; //Record initial position of transformEditing in local space
            initialPositionEditVector = transformEditing.position; //Record initial edit vector (world space) for per-axis scaling position offset (used for Edit tracking)

            //Record initial position of controller in transformEditing's local space
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale
            initialControllerPosition = transformEditing.InverseTransformPoint(transform.position); //In local space
            transformEditing.localScale = initialTransformEditingEditVector; //Reset the scale

            initialWorldControllerPosition = transform.position;

            targetAxis = scaler.axis;
            targetAxisVector = getVectorForAxis(targetAxis);
            editType = Edit.EditType.Scale;
            this.scaler = scaler;

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
            Vector3 newScale = initialTransformEditingEditVector;

            if (scaler.allAxisScaler) //Scale on all axes
            {
                //NEED TO FIX THIS!! CAN'T CURRENTLY SCALE DOWN
                //Determine what octant controller is in in relation to transform tool, then scale down when the controller is in the opposite octant(s)
                Vector3 posVector = localSpaceControllerPosition - initialControllerPosition;
                float unitModifier = calculateUnitsModifier(transform.position - initialWorldControllerPosition); //Used to adjust between scaling up and scaling down (positive or negative units)

                unitsToScale = posVector.magnitude * unitModifier;
                newScale = initialTransformEditingEditVector + Vector3.one * unitsToScale;
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

                newScale = initialTransformEditingEditVector + targetAxisVector * unitsToScale; //Calculate the new scale for transformEditing

                //Offset position
                float unitsToTranslate = unitsToScale / 2; //How many units to translate the transform to offset the position for the per-axis scaling operation
                Vector3 posOffset = unitsToTranslate * posOffsetDirection; //Vector position offset for transformEditing
                transformEditing.localPosition = initialPosition + posOffset;

                //Record final edit vector for the EditTracker
                finalPositionEditVector = transformEditing.position; //World space position for edit tracking
            }

            transformEditing.localScale = newScale;

            //Record final edit vectors for the EditTracker
            finalTransformEditingEditVector = transformEditing.localScale;
            finalTransformToolEditVector = transformTool.localScale;
        }
    }

    //Determines if units to scale for uniform scaling should be negative or positive based on the quadrant its in
    float calculateUnitsModifier(Vector3 positionVector)
    {
        float unitModifier;

        Octant octant = getOctant(positionVector);
        Debug.Log(positionVector + " Octant: " + octant);

        switch (octant)
        {
            case Octant.I:
                unitModifier = 1f;
                break;
            case Octant.II:
                unitModifier = 1f;
                break;
            case Octant.III:
                unitModifier = 1f;
                break;
            case Octant.IV:
                unitModifier = 1f;
                break;
            case Octant.V:
                unitModifier = -1f;
                break;
            case Octant.VI:
                unitModifier = -1f;
                break;
            case Octant.VII:
                unitModifier = -1f;
                break;
            case Octant.VIII:
                unitModifier = -1f;
                break;
            default:
                unitModifier = 1f;
                break;
        }

        return unitModifier;
    }

    #endregion Scale

    //Sets a flag that indicates that transformation should stop
    public void stopTransforming(Transform transformTool, Transform transformEditing)
    {
        if (shouldTransform)
        {
            shouldTransform = false;

            //Record transformEditing edit info
            List<Edit> edits = new List<Edit>();
            edits.Add(new Edit(editType, transformEditing, initialTransformEditingEditVector, finalTransformEditingEditVector)); //Transform editing
            if (editType.Equals(Edit.EditType.Scale) && !scaler.allAxisScaler) //Position is also edited
            {
                edits.Add(new Edit(Edit.EditType.Translation, transformEditing, initialPositionEditVector, finalPositionEditVector)); //Transform editing position offset for per-axis scaling
            }
            edits.Add(new Edit(editType, transformTool, initialTransformToolEditVector, finalTransformToolEditVector, true)); //Transform tool

            EditGroup editGroup = new EditGroup(edits);
            editTracker.trackEdits(editGroup);
        }
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

    //Returns the octant in which a Vector3 is located in a 3D coordinate system
    Octant getOctant(Vector3 positionVector)
    {
        Octant octant;

        float x = positionVector.x;
        float y = positionVector.y;
        float z = positionVector.z;

        if (x < 0)
        {
            if (y < 0)
            {
                if (z < 0) // -, -, -
                {
                    octant = Octant.VII;
                }
                else // -, -, +
                {
                    octant = Octant.III;
                }
            }
            else
            {
                if (z < 0) // -, +, -
                {
                    octant = Octant.VI;
                }
                else // -, +, +
                {
                    octant = Octant.II;
                }
            }
        }
        else
        {
            if (y < 0)
            {
                if (z < 0) // +, -, -
                {
                    octant = Octant.VIII;
                }
                else // +, -, +
                {
                    octant = Octant.IV;
                }
            }
            else
            {
                if (z < 0) // +, +, -
                {
                    octant = Octant.V;
                }
                else // +, +, +
                {
                    octant = Octant.I;
                }
            }
        }
        

        return octant;
    }

    #endregion Helpers

    #region General Events

    //Handles the event of an undo edit event
    public Transform handleEditTrackerUndo()
    {
        EditGroup editGroup = editTracker.undo();

        if (editGroup != null)
        {
            return editGroup.getTransformEdited();
        }

        return null;
    }

    //Handles the event of an undo edit event
    public Transform handleEditTrackerRedo()
    {
        EditGroup editGroup = editTracker.redo();

        if (editGroup != null)
        {
            return editGroup.getTransformEdited();
        }

        return null;
    }

    #endregion General Events
}
