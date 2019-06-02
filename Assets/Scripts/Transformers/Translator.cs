using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : Transformer
{
    [SerializeField]
    Axis axis; //Axis of transformation

    Vector3 initialControllerPosition; //Position of controller when translation is initiated in local space of transformEditing
    Vector3 axisVector; //Holds a vector representation of the Transformer axis

    Vector3 initTransformEditingEditVector; //Initial position of transformEditing
    Vector3 initTransformToolEditVector; //Initial position of transformTool

    //Drags translator
    public override void drag(Transform transformEditing, Transform controller)
    {
        if (transformerState == TransformerState.IDLE) //Not currently translating
        {
            StartCoroutine(translate(transformEditing, controller)); //Start translating
        }
    }

    //Releases translator
    public override void release()
    {
        if (transformerState == TransformerState.TRANSFORMING) //Currently translating
        {
            setTransformerState(TransformerState.IDLE); //Stop translating
        }
    }

    #region Translation

    //Perform initial calculations for translation
    void initalizeTranslation(Transform transformEditing, Transform controller)
    {
        //Record initial edit vectors
        initTransformEditingEditVector = transformEditing.position;
        initTransformToolEditVector = transformTool.transform.position;

        Vector3 transformEditingScale = transformEditing.localScale; //Store scale of transformEditing
        transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale

        //Controller position in local space of transformEditing
        initialControllerPosition = transformEditing.InverseTransformPoint(controller.position);

        transformEditing.localScale = transformEditingScale; //Reset the scale

        axisVector = getVectorForAxis(axis); //Store vector form of axis
    }

    //Translate transformEditing to match controller movements
    IEnumerator translate(Transform transformEditing, Transform controller)
    {
        initalizeTranslation(transformEditing, controller);

        setTransformerState(TransformerState.TRANSFORMING);

        //Perform translation
        while (transformerState == TransformerState.TRANSFORMING)
        {
            Vector3 transformEditingScale = transformEditing.localScale; //Store scale of transformEditing
            transformEditing.localScale = Vector3.one; //Set the scale to 1 to avoid issues with InverseTransformPoint() being affected by scale

            //Translation along base.axis
            Vector3 translation = axisVector * getUnitsToTranslate(transformEditing, controller);

            transformEditing.Translate(translation, Space.Self); //Translate transformEditing in local space
            transformTool.transform.Translate(translation); //Translate transformTool with transformEditing

            transformEditing.localScale = transformEditingScale; //Reset the scale

            yield return null; //Wait for next frame
        }

        recordEdits(transformEditing);
    }

    //Records edits used in translation operation
    void recordEdits(Transform transformEditing)
    {
        //Record edits for translation
        TranslationEdit translationEdit = new TranslationEdit(transformEditing, initTransformEditingEditVector, transformEditing.position);
        TranslationEdit transformToolEdit = new TranslationEdit(transformTool.transform, initTransformToolEditVector, transformTool.transform.position);

        //Create compound edit to store transformEditing and transformTool translations
        CompoundEdit compoundEdit = new CompoundEdit();
        compoundEdit.addEdit(translationEdit);
        compoundEdit.addEdit(transformToolEdit);

        editTracker.trackEdit(compoundEdit); //Start tracking edit in edit tracker
    }

    //Gets the number of units to translate based on the axis of translation
    float getUnitsToTranslate(Transform transformEditing, Transform controller)
    {
        //The position of the controller in the local space of transformEditing
        Vector3 localSpaceControllerPosition = transformEditing.InverseTransformPoint(controller.position);

        //Get the number of units to translate on the given axis
        switch (axis)
        {
            case Axis.x:
                return localSpaceControllerPosition.x - initialControllerPosition.x;
            case Axis.y:
                return localSpaceControllerPosition.y - initialControllerPosition.y;
            case Axis.z:
                return localSpaceControllerPosition.z - initialControllerPosition.z;
            default:
                return 0;
        }
    }

    #endregion Translation
}