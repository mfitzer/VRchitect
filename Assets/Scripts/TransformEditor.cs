using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEditor : MonoBehaviour {

    private static TransformEditor instance;
    public static TransformEditor Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<TransformEditor>();
            return instance;
        }
    }

    // Use this for initialization
    void Start()
    {
        
    }

    #region General Events

    //Handles the event of an undo edit event
    public Transform handleEditTrackerUndo()
    {
        TransformationEdit edit = (TransformationEdit) EditTracker.Instance.undo();

        if (edit != null) //Edit is not null
        {
            return edit.transformEdited;
        }

        return null;
    }

    //Handles the event of an undo edit event
    public Transform handleEditTrackerRedo()
    {
        TransformationEdit edit = (TransformationEdit) EditTracker.Instance.redo();

        if (edit != null) //Edit is not null
        {
            return edit.transformEdited;
        }

        return null;
    }

    #endregion General Events
}
