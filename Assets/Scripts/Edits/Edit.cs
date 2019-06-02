using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Edit
{
    public abstract void execute();

    public abstract void undo();

    public abstract void redo();

    public abstract string toString();
}
