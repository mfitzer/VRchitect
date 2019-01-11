using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditTracker {

    public enum EditAction { Undo, Redo}

    Stack<EditGroup> pastEdits = new Stack<EditGroup>(); //Edits that can be undone from the current edit
    Stack<EditGroup> futureEdits = new Stack<EditGroup>(); //Edits that can be redone from the current edit

    //Stores EditGroup information
    public void trackEdits(EditGroup editGroup)
    {
        pastEdits.Push(editGroup);
        futureEdits.Clear();
    }

    //Goes back one edit in pastEdits (Returns true if an edit was undone, false otherwise)
    public EditGroup undo()
    {
        if (pastEdits.Count > 0) //There are edits to undo
        {
            EditGroup editGroup = pastEdits.Pop();
            editGroup.performEdits(EditAction.Undo);
            futureEdits.Push(editGroup); //Make edit available for redoing

            return editGroup;
        }
        else
        {
            return null;
        }
    }

    //Goes forward one edit in futureEdits (Returns true if an edit was redone, false otherwise)
    public EditGroup redo()
    {
        if (futureEdits.Count > 0) //There are edits to redo
        {
            EditGroup editGroup = futureEdits.Pop(); //Get the next edit
            editGroup.performEdits(EditAction.Redo);
            pastEdits.Push(editGroup); //Make edit available for redoing

            return editGroup;
        }
        else
        {
            return null;
        }
    }
}
