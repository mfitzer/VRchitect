using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    //Vive Controller Input
    public SteamVR_TrackedObject leftTrackedObj;
    public SteamVR_TrackedController leftTrackedController;
    public SteamVR_Controller.Device leftController
    {
        get { return SteamVR_Controller.Input((int)leftTrackedObj.index); }
    }
    public SteamVR_TrackedObject rightTrackedObj;
    public SteamVR_TrackedController rightTrackedController;
    public SteamVR_Controller.Device rightController
    {
        get { return SteamVR_Controller.Input((int)rightTrackedObj.index); }
    }

    //Class Interactions
    public InteractableEditor interactableEditor;

    void rightTriggerClicked(object sender, ClickedEventArgs e)
    {
        interactableEditor.handleTriggerPulled();
    }

    void rightTriggerDown() //Fires continuously when trigger is in down position
    {
        interactableEditor.handleTriggerDown();
    }

    void rightTriggerUp() //Fires continuously when trigger is in up position
    {

    }

    void rightTriggerReachedDownPosition() //Fires once upon reaching down position
    {

    }

    void rightTriggerReachedUpPosition() //Fires once upon reaching up position
    {
        interactableEditor.handleTriggerReachedUpPosition();
    }

    void rightPadClicked(object sender, ClickedEventArgs e)
    {
        float xAxis = leftController.GetAxis().x;
        float yAxis = leftController.GetAxis().y;
        bool centerX = false; //Click is centered on the x-axis
        bool centerY = false; //Click is centered on the y-axis

        if (xAxis < -0.5f) //Click left
        {
            
        }
        else if (xAxis > 0.5f) //Click right
        {
            
        }
        else //-0.5f < xAxis < 0.5f
        {
            centerX = true;
        }

        if (yAxis < -0.5f) //Click down
        {

        }
        else if (yAxis > 0.5f) //Click up
        {

        }
        else //-0.5f < yAxis < 0.5f
        {
            centerY = true;
        }

        if (centerX && centerY) //Center click
        {

        }
    }

    void rightGripped(object sender, ClickedEventArgs e)
    {
        interactableEditor.deselectInteractable();
    }

    void leftTriggerClicked(object sender, ClickedEventArgs e)
    {

    }

    void leftTriggerReachedDownPosition() //Fires once upon reaching down position
    {
        
    }

    void leftTriggerReachedUpPosition() //Fires once upon reaching up position
    {

    }

    void leftTriggerDown() //Fires continuously when trigger is in down position
    {

    }

    void leftTriggerUp() //Fires continuously when trigger is in up position
    {

    }

    void leftPadClicked(object sender, ClickedEventArgs e)
    {
        float xAxis = leftController.GetAxis().x;
        float yAxis = leftController.GetAxis().y;
        bool centerX = false; //Click is centered on the x-axis
        bool centerY = false; //Click is centered on the y-axis
        
        if (xAxis < -0.5f) //Click left
        {
            interactableEditor.handleEditTrackerUndo();
        }
        else if (xAxis > 0.5f) //Click right
        {
            interactableEditor.handleEditTrackerRedo();
        }
        else //-0.5f < xAxis < 0.5f
        {
            centerX = true;
        }

        if (yAxis < -0.5f) //Click down
        {

        }
        else if (yAxis > 0.5f) //Click up
        {

        }
        else //-0.5f < yAxis < 0.5f
        {
            centerY = true;
        }

        if (centerX && centerY) //Center click
        {

        }
    }    

    void leftGripped(object sender, ClickedEventArgs e)
    {
        
    }

    public void triggerHapticFeedBack(bool left, ushort leftDuration, bool right, ushort rightDuration)
    {
        if (left)
        {
            leftController.TriggerHapticPulse(leftDuration);
        }
        if (right)
        {
            rightController.TriggerHapticPulse(rightDuration);
        }
    }

    // Use this for initialization
    void Start()
    {
        //Registering functions with input events
        leftTrackedController.PadClicked += leftPadClicked;
        leftTrackedController.TriggerClicked += leftTriggerClicked;
        leftTrackedController.Gripped += leftGripped;
        rightTrackedController.PadClicked += rightPadClicked;
        rightTrackedController.TriggerClicked += rightTriggerClicked;
        rightTrackedController.Gripped += rightGripped;
    }

    // Update is called once per frame
    void Update()
    {
        bool leftPadPressed = leftController.GetPress(SteamVR_Controller.ButtonMask.Touchpad); //Determines if left pad is pressed down
        
        bool rightPadPressed = rightController.GetPress(SteamVR_Controller.ButtonMask.Touchpad); //Determines if right pad is pressed down


        if (leftController.GetHairTriggerDown())
        {
            leftTriggerReachedDownPosition();
        }
        if (rightController.GetHairTriggerDown())
        {
            rightTriggerReachedDownPosition();
        }
        if (leftController.GetHairTriggerUp())
        {
            leftTriggerReachedUpPosition();
        }
        if (rightController.GetHairTriggerUp())
        {
            rightTriggerReachedUpPosition();
        }

        if (leftController.GetHairTrigger())
        {
            leftTriggerDown();
        }
        else
        {
            leftTriggerUp();
        }
        if (rightController.GetHairTrigger())
        {
            rightTriggerDown();
        }
        else
        {
            rightTriggerUp();
        }
    }
}
