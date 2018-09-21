using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableEditor : MonoBehaviour {

    public enum EditorState { idle, ready, editing }
    internal EditorState interactableEditorState = EditorState.idle;
    internal EditorState transformEditorState = EditorState.idle;

    EditTracker.EditType editTypeReady; //Type of edit corresponding to the highlighted part of the transform tool
    EditTracker.EditType editTypeActive; //Type of edit corresponding to the edit currently being performed

    //Interactables
    internal Transform interactableEditing;
    Dictionary<Transform, Material> interactableMaterials = new Dictionary<Transform, Material>(); //Materials of the currently highlighted interactables
    internal Transform interactableReady;
    public Material highlightedInteractable; //Material that highlights an interactable

    //Transform tool
    TransformEditor transformEditor;
    public Transform transformTool;
    Quaternion transformToolInitialRotation; //Default rotation of transform tool
    public Material highlightedTransformer; //Material that highlights transformers in the transform tool when it is interacting with the controller

    //Translators
    Translator translatorReady;
    Translator activeTranslator;
    Dictionary<Transform, Material> translatorMaterials = new Dictionary<Transform, Material>(); //Materials of the currently highlighted translators

    //Rotators
    Rotator rotatorReady;
    Rotator activeRotator;
    Dictionary<Transform, Material> rotatorMaterials = new Dictionary<Transform, Material>(); //Materials of the currently highlighted rotators

    //Testing Only
    public TextMesh transformToolStats;

    // Use this for initializatio
    void Start () {
        transformEditor = FindObjectOfType<TransformEditor>();
        transformTool.gameObject.SetActive(false);
        transformToolInitialRotation = transformTool.rotation;
	}

    #region Interactables

    //Selects the interactable
    public void selectInteractable()
    {
        Debug.Log("Selected: " + interactableReady);
        interactableEditorState = EditorState.editing; //Change interactableEditorState

        //Get transform of interactable being edited
        resetMaterials(interactableMaterials); //Reset material
        interactableEditing = interactableReady;
        interactableReady = null;

        //Moving and activating transform tool to position of controller
        transformTool.position = transform.position;
        transformTool.rotation = interactableEditing.rotation;
        transformTool.gameObject.SetActive(true);
    }

    //Deselects the interactable
    public void deselectInteractable()
    {
        if (interactableEditorState.Equals(EditorState.editing) && !transformEditorState.Equals(EditorState.editing)) //Controller is colliding with an interactable
        {
            interactableEditorState = EditorState.idle; //Reset interactableEditorState
            interactableEditing = null; //Remove reference to the selected transform
            transformTool.gameObject.SetActive(false); //Hide transform tool
        }
    }

    //Handles when the controller enters an interactable
    void interactableEntered(Transform interactable)
    {
        if (!interactableEditorState.Equals(EditorState.editing))
        {
            interactableEditorState = EditorState.ready;
            interactableReady = getModelParent(interactable);

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
        if (!interactableEditorState.Equals(EditorState.editing))
        {
            interactableEditorState = EditorState.idle;
        }

        resetMaterials(interactableMaterials); //Reset material
    }

    #endregion Interactables

    #region Translators

    //Handles the selection of a translator
    void dragTranslator()
    {
        if (!transformEditorState.Equals(EditorState.idle)) //A translator is colliding with the controller
        {
            if (!transformEditorState.Equals(EditorState.editing))
            {
                transformEditorState = EditorState.editing; //Adjust editor state
                editTypeActive = EditTracker.EditType.Translation; //Track type of edit being performed
                activeTranslator = translatorReady;
            }

            transformEditor.translate(transformTool, interactableEditing, activeTranslator);
        }
    }

    //Handles the deselection of a translator
    void releaseTranslator()
    {
        if (transformEditorState.Equals(EditorState.editing))
        {
            transformEditor.stopTransforming(transformTool, interactableEditing);

            transformEditorState = EditorState.idle;
            activeTranslator = null;

            resetMaterials(translatorMaterials);
        }
    }

    //Handles when the controller enters a Translator
    void translatorEntered(Collider other)
    {
        if (!transformEditorState.Equals(EditorState.editing))
        {
            Translator translator = other.GetComponent<TranslatorPart>().translator;
            editTypeReady = EditTracker.EditType.Translation; //Track type of edit that's ready

            if (transformEditorState.Equals(EditorState.idle))
            {
                transformEditorState = EditorState.ready;
                translatorReady = translator;

                resetMaterials(translatorMaterials); //Reset materials of previous translator
                setMaterialOfChildren(translator.transform, translatorMaterials, highlightedTransformer);
            }
            else if (transformEditorState.Equals(EditorState.ready))
            {
                if (translatorReady != translator) //New translator
                {
                    resetMaterials(translatorMaterials); //Reset materials of previous translator
                    setMaterialOfChildren(translator.transform, translatorMaterials, highlightedTransformer); //Set material of active translator
                    translatorReady = translator;
                }
            }
        }
    }

    //Handles when the controller exits a Translator
    void translatorExited(Collider other)
    {
        if (!transformEditorState.Equals(EditorState.editing))
        {
            transformEditorState = EditorState.idle;
            translatorReady = null;
            resetMaterials(translatorMaterials);
        }
    }

    #endregion Translators

    #region Rotators

    //Handles the selection of a Rotator
    void dragRotator()
    {
        if (!transformEditorState.Equals(EditorState.idle)) //A Rotator is colliding with the controller
        {
            transformEditorState = EditorState.editing; //Adjust editor state
            editTypeActive = EditTracker.EditType.Rotation; //Track type of edit being performed
            activeRotator = rotatorReady;

            //Transform parent based translation
            transformEditor.rotate(transformTool, interactableEditing, activeRotator);
        }
    }

    //Handles the deselection of a Rotator
    void releaseRotator()
    {
        if (transformEditorState.Equals(EditorState.editing))
        {
            transformEditor.stopTransforming(transformTool, interactableEditing);

            transformEditorState = EditorState.idle;
            activeRotator = null;

            resetMaterials(rotatorMaterials);
            //resetTransformTool(); //Reset transform tool's rotation
        }
    }

    //Handles when the controller enters a Rotator
    void rotatorEntered(Collider other)
    {
        if (!transformEditorState.Equals(EditorState.editing))
        {
            Rotator rotator = other.GetComponent<Rotator>();
            editTypeReady = EditTracker.EditType.Rotation; //Track type of edit that's ready

            if (transformEditorState.Equals(EditorState.idle))
            {
                transformEditorState = EditorState.ready;
                rotatorReady = rotator;

                resetMaterials(rotatorMaterials); //Reset materials of previous Rotator
                setMaterialOfChildren(rotator.transform, rotatorMaterials, highlightedTransformer);
            }
            else if (transformEditorState.Equals(EditorState.ready))
            {
                if (rotatorReady != rotator) //New Rotator
                {
                    resetMaterials(rotatorMaterials); //Reset materials of previous Rotator
                    setMaterialOfChildren(rotator.transform, rotatorMaterials, highlightedTransformer); //Set material of active Rotator
                    rotatorReady = rotator;
                }
            }
        }
    }

    //Handles when the controller exits a Rotator
    void rotatorExited(Collider other)
    {
        if (!transformEditorState.Equals(EditorState.editing))
        {
            transformEditorState = EditorState.idle;
            rotatorReady = null;
            resetMaterials(rotatorMaterials);
        }
    }

    #endregion Rotators

    #region General Events

    //Handles the event of an undo edit event
    public void handleEditTrackerUndo()
    {
        Transform editedTransform = transformEditor.handleEditTrackerUndo();

        if (interactableEditorState.Equals(EditorState.editing))
        {
            interactableEditing = editedTransform;
        }
    }

    //Handles the event of an undo edit event
    public void handleEditTrackerRedo()
    {
        Transform editedTransform = transformEditor.handleEditTrackerRedo();

        if (interactableEditorState.Equals(EditorState.editing))
        {
            interactableEditing = editedTransform;
        }
    }

    //Used for indicating if the player is teleporting
    public void handleTeleportingEvent()
    {
        resetMaterials(interactableMaterials);
        resetMaterials(translatorMaterials);
        resetMaterials(rotatorMaterials);
    }

    //Handles the event of the controller trigger being pulled
    public void handleTriggerPulled()
    {
        switch (interactableEditorState)
        {
            case (EditorState.ready):
                selectInteractable();
                break;
        }
    }

    //Handles the event of the controller trigger being down
    public void handleTriggerDown()
    {
        if (!transformEditorState.Equals(EditorState.idle)) //Ready or editing
        {
            switch (editTypeReady)
            {
                case (EditTracker.EditType.Translation):
                    if (transformEditorState.Equals(EditorState.ready) || editTypeActive.Equals(EditTracker.EditType.Translation)) //If not ready, that means transformEditorState == editing
                    {
                        dragTranslator();
                    }
                    break;
                case (EditTracker.EditType.Rotation):
                    if (transformEditorState.Equals(EditorState.ready) || editTypeActive.Equals(EditTracker.EditType.Rotation)) //If not ready, that means transformEditorState == editing
                    {
                        dragRotator();
                    }
                    break;
                case (EditTracker.EditType.Scale):
                    //if (transformEditorState.Equals(EditorState.ready) || editTypeActive.Equals(EditTracker.EditType.Scaler)) //If not ready, that means transformEditorState == editing
                    //{
                    //    dragScaler();
                    //}
                    break;
            }
        }
    }

    //Handles the event of the controller trigger being down
    public void handleTriggerReachedUpPosition()
    {
        if (transformEditorState.Equals(EditorState.editing))
        {
            switch (editTypeReady)
            {
                case (EditTracker.EditType.Translation):
                    if (transformEditorState.Equals(EditorState.editing) && editTypeActive.Equals(EditTracker.EditType.Translation)) //If performing translation, release translator
                    {
                        releaseTranslator();
                    }
                    break;
                case (EditTracker.EditType.Rotation):
                    if (transformEditorState.Equals(EditorState.editing) && editTypeActive.Equals(EditTracker.EditType.Rotation)) //If performing rotation, release rotator
                    {
                        releaseRotator();
                    }
                    break;
                case (EditTracker.EditType.Scale):
                    //if (transformEditorState.Equals(EditorState.editing) && editTypeActive.Equals(EditTracker.EditType.Scaler)) //If performing scale, release scaler
                    //{
                    //    releaseScaler();
                    //}
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            interactableEntered(other.transform);
        }

        if (other.CompareTag("TranslatorPart"))
        {
            translatorEntered(other);
        }

        if (other.CompareTag("Rotator"))
        {
            rotatorEntered(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            interactableExited();
        }

        if (other.CompareTag("TranslatorPart"))
        {
            translatorExited(other);
        }

        if (other.CompareTag("Rotator"))
        {
            rotatorExited(other);
        }
    }

    #endregion General Events

    #region Helpers

    //Gets the parent of a model if it is nested in another model
    Transform getModelParent(Transform model)
    {
        if (model.parent.CompareTag("ModelParent"))
        {
            return model;
        }
        else
        {
            return getModelParent(model.parent);
        }
    }

    //Takes a dictionary of transform keys and sets their parents to their associated values
    void resetTransformParents(Dictionary<Transform, Transform> transformToReset)
    {
        foreach (KeyValuePair<Transform, Transform> item in transformToReset) //Reset the interactables' materials
        {
            item.Key.parent = item.Value; //Set the parent of the key to the value
        }

        transformToReset.Clear(); //Clear the dictionary
    }

    //Resets the transform tool's rotation after an edit
    void resetTransformTool()
    {
        transformTool.rotation = transformToolInitialRotation;
    }

    //Reset materials of recorded transforms and remove the references to the materials
    void resetMaterials(Dictionary<Transform, Material> materials)
    {
        foreach (KeyValuePair<Transform, Material> item in materials) //Reset the interactables' materials
        {
            item.Key.GetComponent<Renderer>().material = materials[item.Key];
        }

        materials.Clear(); //Clear the dictionary
    }

    //Sets the material of a parent's child transforms and records the original materials in a dictionary of materials
    void setMaterialOfChildren(Transform parent, Dictionary<Transform, Material> materials, Material newMaterial)
    {
        if (parent.childCount == 0) //No children
        {
            Renderer renderer = parent.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (!materials.ContainsKey(parent))
                {
                    materials.Add(parent, renderer.material);
                }

                renderer.material = newMaterial;
            }
        }
        else
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>())
            {
                if (child != parent)
                {
                    setMaterialOfChildren(child, materials, newMaterial);
                }
            }
        }
    }

    #endregion Helpers

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
