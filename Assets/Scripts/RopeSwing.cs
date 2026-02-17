using UnityEngine;

public class RopeSwing : PhysicsInteractable
{
    private MovementController _movementController;
    private float _swingSpeed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool CanInteract(InteractionController controller)
    {
        return base.CanInteract(controller);
    }
    public override void OnInteractionEnd(InteractionController controller)
    {
        base.OnInteractionEnd(controller);
    }
    protected void PusherJumped()
    {
        if (currentInteractor) OnInteractionEnd(currentInteractor);
    }
    /*
    public override bool OnInteract(InteractionController controller)
    {
        if (!base.OnInteract(controller)) return false;

        currentInteractor = controller;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Lock player rotation while pushing/pulling
        _movementController = controller.GetComponent<MovementController>();
        if (_movementController)
        {
            _movementController.overrideCanJump = true;
            if (_movementController is AdvancedMoveController)
            {
                (_movementController as AdvancedMoveController).onJumpPerformed.AddListener(PusherJumped);
            }
            defaultPlayerRotationSpeed = movementController.rotationSpeed;
            movementController.rotationSpeed = 0;
        }

        AttachToController<FixedJoint>(controller);
        return true;
    }

    public override void OnInteractionEnd(InteractionController controller)
    {
        if (currentInteractor == null) return;

        DetachFromController();
        rb.interpolation = defaultInterpolation;

        // Restore player's original rotation speed
        if (_movementController != null)
        {
            _movementController.overrideCanJump = false;
            if (_movementController is AdvancedMoveController)
            {
                (_movementController as AdvancedMoveController).onJumpPerformed.RemoveListener(PusherJumped);
            }
            _movementController.rotationSpeed = defaultPlayerRotationSpeed;
            _movementController = null;
        }
        base.OnInteractionEnd(controller);
    }
    */
}
