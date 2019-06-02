using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompoundEdit : Edit
{
    List<Edit> edits;

    public CompoundEdit()
    {
        edits = new List<Edit>();
    }

    public CompoundEdit(List<Edit> edits)
    {
        this.edits = edits;
    }

    //Executes each edit in edits
    public override void execute()
    {
        foreach (Edit edit in edits)
        {
            edit.execute();
        }
    }

    //Undoes each edit in edits
    public override void undo()
    {
        foreach (Edit edit in edits)
        {
            edit.undo();
        }
    }

    //Redoes each edit in edits
    public override void redo()
    {
        foreach (Edit edit in edits)
        {
            edit.redo();
        }
    }

    //Prints a string representation of the compound edit
    public override string toString()
    {
        string editsString = "Compound Edit: [";

        foreach (Edit edit in edits)
        {
            editsString += "  " + edit.toString() + "\n";
        }

        editsString += "]";

        return editsString;
    }

    //Adds an edit to edits
    public void addEdit(Edit edit)
    {
        edits.Add(edit);
    }

    //Removes and edit from edits
    public bool removeEdit(Edit edit)
    {
        return edits.Remove(edit);
    }

    //Gets the transform of the edits involved in the edit (this will need to be overhauled once multi object editing is implemented)
    public Transform getTransformEdited()
    {
        Transform transformEdited = null;
        foreach (Edit edit in edits)
        {
            /*if (edit.transformEdited)
            {

            }*/
            /*if (!edit.isTransformTool) //Don't want to return the transform tool
            {
                transformEdited = keyValue.Key;
            }
            break;*/
        }

        return transformEdited;
    }
}
