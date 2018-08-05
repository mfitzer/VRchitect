﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditTracker : MonoBehaviour {

    public enum EditType { Translation, Rotation, Scale } //Indicates the type of an edit

    Stack<Edit> pastEdits = new Stack<Edit>(); //Edits that can be undone from the current edit
    Stack<Edit> futureEdits = new Stack<Edit>(); //Edits that can be redone from the current edit

    //Holds data about an edit
    struct Edit
    {
        public EditType editType;
        public Transform transformEdited;
        public Vector3 oldVector;
        public Vector3 newVector;

        public string formatAsString() //Returns a formatted string version of an edit
        {
            return "Edit Type: " + editType + "  Transform Edited: " + transformEdited.name + "  Old Vector: " + oldVector + "  New Vector: " + newVector;
        }
    }

    // Use this for initialization
    void Start () {
        
	}

    //Makes a new edit and updates the past and future edits
    public void makeEdit(EditType editType, Transform transformEdited, Vector3 oldVector, Vector3 newVector)
    {
        Debug.Log("Made edit: " + "Type: " + editType + "  Transform: " + transformEdited.name + "  Old: " + oldVector + "  New: " + newVector);
        pastEdits.Push(createEdit(editType, transformEdited, oldVector, newVector));
        futureEdits.Clear();
    }

    //Goes back one edit in pastEdits (Returns true if an edit was undone, false otherwise)
    public bool undo()
    {
        if (pastEdits.Count > 0) //There are edits to undo
        {
            Edit edit = pastEdits.Pop(); //Get the last edit
            performEdit(edit, edit.oldVector);
            futureEdits.Push(edit); //Make edit available for redoing

            //Debug.Log("Undo edit: " + edit.formatAsString());

            return true;
        }
        else
        {
            return false;
        }
    }

    //Goes forward one edit in futureEdits (Returns true if an edit was redone, false otherwise)
    public bool redo()
    {
        if (futureEdits.Count > 0) //There are edits to redo
        {
            Edit edit = futureEdits.Pop(); //Get the next edit
            performEdit(edit, edit.newVector);
            pastEdits.Push(edit); //Make edit available for redoing

            //Debug.Log("Redo edit: " + edit.formatAsString());

            return true;
        }
        else
        {
            return false;
        }
    }

    //Creates and returns a new Edit
    Edit createEdit(EditType editType, Transform transformEdited, Vector3 oldVector, Vector3 newVector)
    {
        Edit edit = new Edit();
        edit.editType = editType;
        edit.transformEdited = transformEdited;
        edit.oldVector = oldVector;
        edit.newVector = newVector;
        return edit;
    }

    //Perform the edit using the specified vector
    void performEdit(Edit edit, Vector3 editedVector)
    {
        Transform transformEditing = edit.transformEdited;

        switch (edit.editType) //Determine the EditType
        {
            case EditType.Translation:
                transformEditing.position = editedVector;
                break;
            case EditType.Rotation:
                transformEditing.rotation = Quaternion.Euler(editedVector);
                break;
            case EditType.Scale:
                transformEditing.localScale = editedVector;
                break;
        }
    }

    // Update is called once per frame
    void Update () {

	}
}
