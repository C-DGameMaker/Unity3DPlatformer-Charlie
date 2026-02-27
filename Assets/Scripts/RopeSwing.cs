using Unity.VisualScripting;
using UnityEngine;

//As it is a Physics interactable, it is apart of that thingy thing. We all know the thingy thing. I forget the name of it. 
public class RopeSwing : PhysicsInteractable
{
    //Before we begin I would like to note that the PushableInteractable script has helped me out a bunch in trying to figure out how to make the swing work. I decided for fun to test it out on the rope to see how it worked and it worked like a doll. If the code looks somewhat familar, that's why.

    [Tooltip("Force required to break the connection with the player")]
    [SerializeField] private float jointBreakForce = 200f;
    [Tooltip("Rotational force required to break the connection with the player")]
    [SerializeField] private float jointBreakTorque = 200f;

    //This stuff came from the interactable script, but also calls the movement controller and the players rotation speed. It's not like the player can rotate anyways. 
    private MovementController movementController;
    private float defaultPlayerRotationSpeed;

    public override bool OnInteract(InteractionController controller)
    {
        if (!base.OnInteract(controller)) return false;

        currentInteractor = controller;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Lock player rotation while pushing/pulling
        movementController = controller.GetComponent<MovementController>();
        if (movementController)
        {
            movementController.overrideCanJump = true;
            if (movementController is AdvancedMoveController)
            {
                (movementController as AdvancedMoveController).onJumpPerformed.AddListener(SwingJump);
            }
            defaultPlayerRotationSpeed = movementController.rotationSpeed;
            movementController.rotationSpeed = 0;
        }

        AttachToController<FixedJoint>(controller);
        return true;
    }
    private void OnJointBreak(float breakForce)
    {
        if (currentInteractor != null)
            currentInteractor.EndCurrentInteraction();
    }


    public override void OnInteractedAlreadyInteracting(InteractionController controller)
    {
        base.OnInteractedAlreadyInteracting(controller);
        SwingJump();
    }

    protected void SwingJump()
    {
        Debug.Log("Start");
        if (currentInteractor == null) return;
        Rigidbody playerRb = currentInteractor.GetComponent<Rigidbody>();
        Vector3 launchVelocity = playerRb.linearVelocity;
        launchVelocity.y = 0f;

        float jumpForce = 8f;

        DetachFromController();

        playerRb.linearVelocity = launchVelocity + Vector3.up * jumpForce;
        Debug.Log("End");
    }

    protected override void AttachToController<T>(InteractionController controller)
    {
        base.AttachToController<T>(controller);
        if (physicsJoint != null)
        {
            physicsJoint.breakForce = jointBreakForce;
            physicsJoint.breakTorque = jointBreakTorque;
            physicsJoint.massScale = 3.0f;
            physicsJoint.connectedMassScale = 3.0f;
        }
    }

    public override void OnInteractionEnd(InteractionController controller)
    {
        if (currentInteractor == null) return;

        DetachFromController();
        rb.interpolation = defaultInterpolation;

        // Restore player's original rotation speed
        if (movementController != null)
        {
            movementController.overrideCanJump = false;
            if (movementController is AdvancedMoveController)
            {
                (movementController as AdvancedMoveController).onJumpPerformed.RemoveListener(SwingJump);
            }
            movementController.rotationSpeed = defaultPlayerRotationSpeed;
            movementController = null;
        }
        base.OnInteractionEnd(controller);
    }



}
