using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllAxisScaler : Transformer
{
    Vector3 initTransformEditingScale; //Initial scale of transformEditing

    Vector3 initTransformEditingWorldPos; //Initial position of transformEditing in world space

    Vector3 initWorldControllerPos; //Initial position of controller in world space
    Vector3 initLocalControllerPos; //Initial position of controller in local space of transformEditing

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

        //Record initial edit vector (world space) for per-axis scaling position offset (used for Edit tracking)
        initTransformEditingWorldPos = transformEditing.position;

        //Record initial position of controller in transformEditing's local space
        transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale
        initLocalControllerPos = transformEditing.InverseTransformPoint(controller.position); //In local space
        transformEditing.localScale = initTransformEditingScale; //Reset the scale

        initWorldControllerPos = controller.position; //Record initial world position of controller
    }

    //Scale transformEditing to match controller movements
    IEnumerator scale(Transform transformEditing, Transform controller)
    {
        initalizeScaling(transformEditing, controller);

        setTransformerState(TransformerState.TRANSFORMING);

        //Perform rotation
        while (transformerState == TransformerState.TRANSFORMING)
        {
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale

            //The position of the controller in the local space of transformEditing
            Vector3 localSpaceControllerPosition = transformEditing.InverseTransformPoint(controller.position);
            
            //unitsToScale is the distance between current controller position and the initial world position on the y axis
            float unitsToScale = controller.position.y - initWorldControllerPos.y;

            //Initial scale plus Vector3(unitsToScale, unitsToScale, unitsToScale)
            Vector3 newScale = initTransformEditingScale + Vector3.one * unitsToScale;

            transformEditing.localScale = newScale; //Scale transformEditing to new scale

            yield return null; //Wait for next frame
        }

        recordEdit(transformEditing);
    }

    //Records edit used in scaling operation
    void recordEdit(Transform transformEditing)
    {
        //Record edit for scaling
        ScaleEdit scaleEdit = new ScaleEdit(transformEditing, initTransformEditingScale, transformEditing.localScale);

        //Create compound edit to store transformEditing and transformTool translations
        CompoundEdit compoundEdit = new CompoundEdit();
        compoundEdit.addEdit(scaleEdit);

        editTracker.trackEdit(compoundEdit); //Start tracking edit in edit tracker
    }

    #endregion Scaling
}