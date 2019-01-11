using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditGroup
{
    Dictionary<Transform, List<Edit>> transformEdits; //Organizes all Edits in the EditGroup by the Transform being edited

    //Constructor, initialize transformEdits
    public EditGroup (List<Edit> edits)
    {
        transformEdits = new Dictionary<Transform, List<Edit>>();

        foreach (Edit edit in edits)
        {
            Transform transformEdited = edit.getTransformEdited();

            if (!transformEdits.ContainsKey(transformEdited))
            {
                transformEdits.Add(transformEdited, new List<Edit>());
            }

            transformEdits[transformEdited].Add(edit);
        }

        configureEdits();
    }

    //Confirgure edits to perform translations, rotations, and scales only if there are not Edits of those types present
    void configureEdits()
    {
        foreach (KeyValuePair<Transform, List<Edit>> keyValue in transformEdits)
        {
            bool containsTranslation = false;
            bool containsRotation = false;
            bool containsScale = false;

            //Determine what edits types are present (assuming there will one of each type maximum for each Transform)
            foreach (Edit edit in keyValue.Value)
            {
                Edit.EditType editType = edit.getEditType();

                switch (editType) //Determine the EditType
                {
                    case Edit.EditType.Translation:
                        containsTranslation = true;
                        break;
                    case Edit.EditType.Rotation:
                        containsRotation = true;
                        break;
                    case Edit.EditType.Scale:
                        containsScale = true;
                        break;
                }
            }

            //Indicate in each edit if it should perform translations, rotations, or scale operations
            foreach (Edit edit in keyValue.Value)
            {
                Edit.EditType editType = edit.getEditType();

                switch (editType) //Determine the EditType
                {
                    case Edit.EditType.Translation:
                        edit.setShouldRotate(!containsRotation);
                        edit.setShouldScale(!containsScale);
                        break;
                    case Edit.EditType.Rotation:
                        edit.setShouldTranslate(!containsTranslation);
                        edit.setShouldScale(!containsScale);
                        break;
                    case Edit.EditType.Scale:
                        edit.setShouldTranslate(!containsTranslation);
                        edit.setShouldRotate(!containsRotation);
                        break;
                }
            }
        }
    }


    //Perform each edit
    public void performEdits(EditTracker.EditAction editAction)
    {
        foreach (KeyValuePair<Transform, List<Edit>> keyValue in transformEdits)
        {
            foreach(Edit edit in keyValue.Value)
            {
                edit.performEdit(editAction);
            }
        }
    }

    //Gets the transform of the edits involved in the edit (this will need to be overhauled once multi object editing is implemented)
    public Transform getTransformEdited()
    {
        Transform transformEdited = null;
        foreach (KeyValuePair<Transform, List<Edit>> keyValue in transformEdits)
        {
            foreach (Edit edit in keyValue.Value)
            {
                if (!edit.getIsTransformTool()) //Don't want to return the transform tool
                {
                    transformEdited = keyValue.Key;
                }
                break;
            }
        }

        return transformEdited;
    }
}
