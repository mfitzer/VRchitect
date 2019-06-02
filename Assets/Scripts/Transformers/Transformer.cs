using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transformer : MonoBehaviour {

    //IDLE: Transformer is not transforming a transform
    //TRANSFORMING: Transformer is actively transforming a transform
    protected enum TransformerState { IDLE, TRANSFORMING }
    protected TransformerState transformerState { get; private set; }

    public enum Axis { x, y, z } //Indicates an axis on a transform

    protected EditTracker editTracker; //Stores edit information

    protected TransformTool transformTool;

    // Use this for initialization
    void Start()
    {
        editTracker = EditTracker.Instance;
        transformTool = TransformTool.Instance;
    }

    public abstract void drag(Transform transformEditing, Transform controller);

    public abstract void release();

    protected virtual void setTransformerState(TransformerState newTransformerState)
    {
        transformerState = newTransformerState;
    }

    #region Helpers

    //Returns a vector representation of the given Axis
    protected Vector3 getVectorForAxis(Axis axis)
    {
        return new Vector3(axis.Equals(Axis.x) ? 1f : 0f, axis.Equals(Axis.y) ? 1f : 0f, axis.Equals(Axis.z) ? 1f : 0f);
    }

    //Returns a 2D vector from origin to positionLookingAt in the 2D plane excluding the given axis
    protected Vector3 get2DDirectionalVector(Vector3 origin, Vector3 positionLookingAt, Axis axisToZeroOut)
    {
        switch (axisToZeroOut)
        {
            case Axis.x:
                positionLookingAt = new Vector3(origin.x, positionLookingAt.y, positionLookingAt.z);
                break;
            case Axis.y:
                positionLookingAt = new Vector3(positionLookingAt.x, origin.y, positionLookingAt.z);
                break;
            case Axis.z:
                positionLookingAt = new Vector3(positionLookingAt.x, positionLookingAt.y, origin.z);
                break;
        }

        return positionLookingAt - origin;
    }

    #endregion Helpers
}
