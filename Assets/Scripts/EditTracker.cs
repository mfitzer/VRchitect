using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditTracker {

    private static EditTracker instance;
    public static EditTracker Instance
    {
        get
        {
            if (instance == null)
                instance = new EditTracker();
            return instance;
        }
    }

    Stack<Edit> pastEdits = new Stack<Edit>(); //Edits that can be undone from the current edit
    Stack<Edit> futureEdits = new Stack<Edit>(); //Edits that can be redone from the current edit

    //Stores Edit information
    public void trackEdit(Edit edit)
    {
        pastEdits.Push(edit);
        futureEdits.Clear();
    }

    //Goes back one edit in pastEdits (Returns true if an edit was undone, false otherwise)
    public Edit undo()
    {
        if (pastEdits.Count > 0) //There are edits to undo
        {
            Edit edit = pastEdits.Pop();
            edit.undo(); //Undo the edit
            futureEdits.Push(edit); //Make edit available for redoing

            return edit;
        }
        else
        {
            return null;
        }
    }

    //Goes forward one edit in futureEdits (Returns true if an edit was redone, false otherwise)
    public Edit redo()
    {
        if (futureEdits.Count > 0) //There are edits to redo
        {
            Edit edit = futureEdits.Pop(); //Get the next edit
            edit.redo(); //Redo the edit
            pastEdits.Push(edit); //Make edit available for redoing

            return edit;
        }
        else
        {
            return null;
        }
    }
}
