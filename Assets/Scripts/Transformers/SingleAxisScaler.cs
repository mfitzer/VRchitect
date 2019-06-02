using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleAxisScaler : Transformer
{
    [SerializeField]
    Axis axis; //Axis of transformation

    Vector3 initTransformEditingScale; //Initial scale of transformEditing

    Vector3 initTransformEditingWorldPos; //Initial position of transformEditing in world space
    Vector3 initTransformEditingLocalPos; //Initial position of transformEditing in local space
    
    Vector3 initLocalControllerPos; //Initial position of controller in local space of transformEditing

    Vector3 axisVector; //Holds a vector representation of the Transformer axis

    //Drags scaler
    public override void drag(Transform transformEditing, Transform controller)
    {
        if (transformerState == TransformerState.IDLE) //Not currently scaling
        {
            StartCoroutine(scale(transformEditing, controller)); //Start scaling
        }
    }

    //Releases scaler
    public override void release()
    {
        if (transformerState == TransformerState.TRANSFORMING) //Currently scaling
        {
            setTransformerState(TransformerState.IDLE); //Stop scaling
        }
    }

    #region Scaling

    //Perform initial calculations for scaling
    void initalizeScaling(Transform transformEditing, Transform controller)
    {
        initTransformEditingScale = transformEditing.localScale;

        //Record initial position of transformEditing in local space
        initTransformEditingLocalPos = transformEditing.localPosition;

        //Record initial edit vector (world space) for per-axis scaling position offset (used for Edit tracking)
        initTransformEditingWorldPos = transformEditing.position;

        //Record initial position of controller in transformEditing's local space
        transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale
        initLocalControllerPos = transformEditing.InverseTransformPoint(controller.position); //In local space
        transformEditing.localScale = initTransformEditingScale; //Reset the scale
        
        axisVector = getVectorForAxis(axis); //Store vector form of axis
    }

    //Scale transformEditing to match controller movements
    IEnumerator scale(Transform transformEditing, Transform controller)
    {
        initalizeScaling(transformEditing, controller);

        setTransformerState(TransformerState.TRANSFORMING);

        //Perform scaling
        while (transformerState == TransformerState.TRANSFORMING)
        {
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale

            //The position of the controller in the local space of transformEditing
            Vector3 localSpaceControllerPosition = transformEditing.InverseTransformPoint(controller.position);

            //Get number of units to scale
            float unitsToScale = calculateUnitsToScale(transformEditing, controller);

            //Calculate the new scale for transformEditing
            Vector3 newScale = initTransformEditingScale + axisVector * unitsToScale;

            //Offset position
            float unitsToTranslate = unitsToScale / 2; //How many units to translate the transform to offset the position for scaling operation
            Vector3 posOffset = unitsToTranslate * calculatePosOffsetDirection(transformEditing); //Vector position offset for transformEditing
            transformEditing.localPosition = initTransformEditingLocalPos + posOffset; //Translate transformEditing using posOffset

            transformEditing.localScale = newScale; //Scale transformEditing to new scale

            yield return null; //Wait for next frame
        }

        recordEdits(transformEditing);
    }

    //Records edits used in scaling operation
    void recordEdits(Transform transformEditing)
    {
        //Record edits for scaling
        ScaleEdit scaleEdit = new ScaleEdit(transformEditing, initTransformEditingScale, transformEditing.localScale);
        TranslationEdit posOffsetEdit = new TranslationEdit(transformEditing, initTransformEditingWorldPos, transformEditing.position);

        //Create compound edit to store transformEditing and transformTool translations
        CompoundEdit compoundEdit = new CompoundEdit();
        compoundEdit.addEdit(scaleEdit);
        compoundEdit.addEdit(posOffsetEdit);

        editTracker.trackEdit(compoundEdit); //Start tracking edit in edit tracker
    }

    //Calculates the number of units to scale by according to transformer axis
    float calculateUnitsToScale(Transform transformEditing, Transform controller)
    {
        //The position of the controller in the local space of transformEditing
        Vector3 localSpaceControllerPosition = transformEditing.InverseTransformPoint(controller.position);

        switch (axis)
        {
            case Axis.x:
                return localSpaceControllerPosition.x - initLocalControllerPos.x;
            case Axis.y:
                return localSpaceControllerPosition.y - initLocalControllerPos.y;
            case Axis.z:
                return localSpaceControllerPosition.z - initLocalControllerPos.z;
            default:
                return 0f;
        }
    }

    //Calculates direction in which the position should be offset
    Vector3 calculatePosOffsetDirection(Transform transformEditing)
    {
        //Determine position offest according to transformer axis
        switch (axis)
        {
            case Axis.x:
                return transformEditing.right;
            case Axis.y:
                return transformEditing.up;
            case Axis.z:
                return transformEditing.forward;
            default:
                return Vector3.zero;
        }
    }

    #endregion Scaling
}