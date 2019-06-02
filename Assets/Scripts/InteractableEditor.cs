using System.Collections.Generic;
using UnityEngine;

public class InteractableEditor : MonoBehaviour {

    public enum EditorState { IDLE, READY, EDITING }
    internal EditorState interactableEditorState = EditorState.IDLE;
    internal EditorState transformEditorState = EditorState.IDLE;

    //Interactables
    internal Transform interactableEditing;
    Dictionary<Transform, Material> interactableMaterials = new Dictionary<Transform, Material>(); //Materials of the currently highlighted interactables
    internal Transform interactableReady;
    public Material highlightedInteractable; //Material that highlights an interactable

    //Transform tool
    public TransformTool transformTool;
    public Material highlightedTransformer; //Material that highlights transformers in the transform tool when it is interacting with the controller

    TransformEditor transformEditor;

    //Transformers
    Transformer transformerReady;
    Transformer activeTransformer;
    Dictionary<Transform, Material> transformerMaterials = new Dictionary<Transform, Material>(); //Materials of the currently highlighted transformers

    //Testing Only
    public TextMesh transformToolStats;


    // Use this for initializatio
    void Start () {
        transformEditor = TransformEditor.Instance;
	}

    #region Interactables

    //Selects the interactable
    public void selectInteractable()
    {
        Debug.Log("Selected: " + interactableReady);
        interactableEditorState = EditorState.EDITING; //Change interactableEditorState

        //Get transform of interactable being edited
        Helpers.resetMaterials(interactableMaterials); //Reset material
        interactableEditing = interactableReady;
        interactableReady = null;

        //Moving and activating transform tool to position of controller
        transformTool.transform.position = transform.position;
        transformTool.transform.rotation = interactableEditing.rotation;
        transformTool.gameObject.SetActive(true);
    }

    //Deselects the interactable
    public void deselectInteractable()
    {
        if (interactableEditorState.Equals(EditorState.EDITING) && !transformEditorState.Equals(EditorState.EDITING)) //Controller is colliding with an interactable
        {
            interactableEditorState = EditorState.IDLE; //Reset interactableEditorState
            interactableEditing = null; //Remove reference to the selected transform
            transformTool.gameObject.SetActive(false); //Hide transform tool
        }
    }

    //Handles when the controller enters an interactable
    void interactableEntered(Transform interactable)
    {
        if (!interactableEditorState.Equals(EditorState.EDITING))
        {
            interactableEditorState = EditorState.READY;
            interactableReady = Helpers.getNamedParent(interactable, ModelInitializer.Instance.modelParentTag);

            Renderer interactableRenderer = interactableReady.GetComponent<Renderer>();
            if (!interactableMaterials.ContainsKey(interactableReady))
            {
                interactableMaterials.Add(interactableReady, interactableRenderer.material); //Store material of interactable
            }
            interactableRenderer.material = highlightedInteractable; //Highlight interactable
        }
    }

    //Handles when the controller leaves an interactable
    void interactableExited()
    {
        if (!interactableEditorState.Equals(EditorState.EDITING))
        {
            interactableEditorState = EditorState.IDLE;
        }

        Helpers.resetMaterials(interactableMaterials); //Reset material
    }

    #endregion Interactables

    #region Transformers

    //Handles the manipulation of a translator
    void dragTransformer()
    {
        if (!transformEditorState.Equals(EditorState.IDLE)) //A transformer is colliding with the controller
        {
            if (!transformEditorState.Equals(EditorState.EDITING))
            {
                transformEditorState = EditorState.EDITING; //Adjust editor state
                activeTransformer = transformerReady;
            }

            activeTransformer.drag(interactableEditing, transform);
        }
    }

    //Handles the deselection of a translator
    void releaseTransformer()
    {
        if (transformEditorState.Equals(EditorState.EDITING))
        {
            activeTransformer.release();

            transformEditorState = EditorState.IDLE;
            activeTransformer = null;

            Helpers.resetMaterials(transformerMaterials);
        }
    }

    //Handles when controller enters a transformer collider
    void transformerEntered(Transformer transformer)
    {
        if (!transformEditorState.Equals(EditorState.EDITING))
        {
            if (transformEditorState.Equals(EditorState.IDLE)) //Not currently transforming or highlighting a transformer
            {
                transformEditorState = EditorState.READY;
                transformerReady = transformer;

                Helpers.resetMaterials(transformerMaterials); //Reset materials of previous transformer
                Helpers.setMaterialOfChildren(transformer.transform, transformerMaterials, highlightedTransformer);
            }
            else if (transformEditorState.Equals(EditorState.READY)) //Already highlighting a transformer
            {
                if (transformerReady != transformer) //New transformer
                {
                    Helpers.resetMaterials(transformerMaterials); //Reset materials of previous transformer
                    Helpers.setMaterialOfChildren(transformer.transform, transformerMaterials, highlightedTransformer); //Set material of active transformer
                    transformerReady = transformer; //Store reference to new transformer
                }
            }
        }
    }

    //Handles when controller exits a transformer collider
    void transformerExited(Transformer transformer)
    {
        if (!transformEditorState.Equals(EditorState.EDITING)) //Not transforming
        {
            transformEditorState = EditorState.IDLE;
            transformerReady = null;
            Helpers.resetMaterials(transformerMaterials); //Reset materials to originals
        }
    }

    #endregion Transformers

    #region General Events

    //Handles the event of an undo edit event
    public void handleEditTrackerUndo()
    {
        Transform editedTransform = transformEditor.handleEditTrackerUndo();

        if (interactableEditorState.Equals(EditorState.EDITING) && editedTransform != null)
        {
            interactableEditing = editedTransform;
        }
    }

    //Handles the event of an undo edit event
    public void handleEditTrackerRedo()
    {
        Transform editedTransform = transformEditor.handleEditTrackerRedo();

        if (interactableEditorState.Equals(EditorState.EDITING) && editedTransform != null)
        {
            interactableEditing = editedTransform;
        }
    }

    //Used for indicating if the player is teleporting
    public void handleTeleportingEvent()
    {
        Helpers.resetMaterials(interactableMaterials);
        Helpers.resetMaterials(transformerMaterials);

        //Stop editing something if edit is active when teleporting
        releaseTransformer();
    }

    //Handles the event of the controller trigger being pulled
    public void handleTriggerPulled()
    {
        switch (interactableEditorState)
        {
            case (EditorState.READY):
                selectInteractable();
                break;
        }
    }

    //Handles the event of the controller trigger being down
    public void handleTriggerDown()
    {
        if (transformEditorState == EditorState.READY || transformEditorState == EditorState.EDITING) //Ready or editing
        {
            Helpers.resetMaterials(interactableMaterials);
            dragTransformer();
        }
    }

    //Handles the event of the controller trigger being down
    public void handleTriggerReachedUpPosition()
    {
        if (transformEditorState.Equals(EditorState.EDITING))
        {
            releaseTransformer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            interactableEntered(other.transform);
        }

        Transformer transformer = other.GetComponent<Transformer>();
        if (transformer) //transformer is not null
        {
            transformerEntered(transformer);
        }
        else
        {
            TransformerPart transformerPart = other.GetComponent<TransformerPart>();
            if (transformerPart) //transformerPart is not null
            {
                transformerEntered(transformerPart.transformer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            interactableExited();
        }

        Transformer transformer = other.GetComponent<Transformer>();
        if (transformer) //transformer is not null
        {
            transformerEntered(transformer);
        }
    }

    #endregion General Events

    // Update is called once per frame
    void Update () {
        string editorStateStats = "InteractableEditorState: " + interactableEditorState + "\nTransformEditorState: " + transformEditorState;
        if (interactableEditing != null)
        {
            string transformStats = "Position: " + interactableEditing.position + "\nLocal Position: " + interactableEditing.localPosition + "\nRotation: " + interactableEditing.rotation.eulerAngles + "\nLocal Rotation: " + interactableEditing.localRotation.eulerAngles + "\nScale: " + interactableEditing.lossyScale + "\nLocal Scale: " + interactableEditing.localScale;
            transformToolStats.text = transformStats + "\n" + editorStateStats;
        }
        else
        {
            transformToolStats.text = editorStateStats;
        }
    }
}
