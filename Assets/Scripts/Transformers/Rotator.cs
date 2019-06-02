using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : Transformer
{
    [SerializeField]
    Axis axis; //Axis of transformation

    Vector3 initialVectorToController; //Initial vector from transformTool to controller
    Vector3 axisVector; //Holds a vector representation of the Transformer axis
    Vector3 worldAxisVector; //World space vector representation of axisVector

    Vector3 initTransformEditingEditVector; //Initial rotation of transformEditing
    Vector3 initTransformToolEditVector; //Initial rotation of transformTool

    //Drags rotator
    public override void drag(Transform transformEditing, Transform controller)
    {
        if (transformerState == TransformerState.IDLE) //Not currently rotating
        {
            StartCoroutine(rotate(transformEditing, controller)); //Start rotating
        }
    }

    //Releases rotator
    public override void release()
    {
        if (transformerState == TransformerState.TRANSFORMING) //Currently rotating
        {
            setTransformerState(TransformerState.IDLE); //Stop rotating
        }
    }

    #region Rotation

    //Perform initial calculations for rotation
    void initalizeRotation(Transform transformEditing, Transform controller)
    {
        //Record initial edit vectors
        initTransformEditingEditVector = transformEditing.rotation.eulerAngles;
        initTransformToolEditVector = transformTool.transform.rotation.eulerAngles;

        axisVector = getVectorForAxis(axis); //Store vector form of axis

        //Store world space vector representation of axis
        //axisVector is transformed from the local space of the transform tool to world space
        worldAxisVector = transformTool.transform.TransformVector(axisVector);

        //Vector between transform tool and controller on the plane of rotation where worldTargetAxis is the normal vector of the plane
        //Record initial relationship between controller and transformTool
        initialVectorToController = Vector3.ProjectOnPlane(controller.position - transformTool.transform.position, worldAxisVector);
    }

    //Translate transformEditing to match controller movements
    IEnumerator rotate(Transform transformEditing, Transform controller)
    {
        initalizeRotation(transformEditing, controller);

        setTransformerState(TransformerState.TRANSFORMING);

        //Perform rotation
        while (transformerState == TransformerState.TRANSFORMING)
        {
            //Vector between transform tool and controller on the plane of rotation where worldTargetAxis is the normal vector of the plane
            //Record initial relationship between controller and transformTool
            Vector3 newVectorToController = Vector3.ProjectOnPlane(controller.position - transformTool.transform.position, worldAxisVector);

            //Number of degrees to rotate transformEditing on the transformer axis
            float degreesToRotate = Vector3.SignedAngle(initialVectorToController, newVectorToController, worldAxisVector);
            
            //Rotate degreesToRotate around the transformer axis from transformTool's initial rotation
            transformTool.transform.rotation = Quaternion.Euler(initTransformToolEditVector) * Quaternion.AngleAxis(degreesToRotate, axisVector);
            
            //Rotate degreesToRotate around the transformer axis from transformEditing's initial rotation
            transformEditing.rotation = Quaternion.Euler(initTransformEditingEditVector) * Quaternion.AngleAxis(degreesToRotate, axisVector);

            yield return null; //Wait for next frame
        }

        recordEdits(transformEditing);
    }

    //Records edits used in rotation operation
    void recordEdits(Transform transformEditing)
    {
        //Record edits for rotation
        RotationEdit rotationEdit = new RotationEdit(transformEditing, initTransformEditingEditVector, transformEditing.rotation.eulerAngles);
        RotationEdit transformToolRotationEdit = new RotationEdit(transformTool.transform, initTransformToolEditVector, transformTool.transform.rotation.eulerAngles);
        //TranslationEdit transformToolTranslationEdit = new TranslationEdit(transformTool.transform, transformTool.transform.position, transformTool.transform.position);

        //Create compound edit to store transformEditing and transformTool rotations
        CompoundEdit compoundEdit = new CompoundEdit();
        compoundEdit.addEdit(rotationEdit);
        compoundEdit.addEdit(transformToolRotationEdit);
        //compoundEdit.addEdit(transformToolTranslationEdit);

        editTracker.trackEdit(compoundEdit); //Start tracking edit in edit tracker
    }

    #endregion Rotation
}