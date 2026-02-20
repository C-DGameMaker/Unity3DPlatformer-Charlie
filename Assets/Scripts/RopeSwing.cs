using Unity.VisualScripting;
using UnityEngine;

public class RopeSwing : PhysicsInteractable
{
    bool _isSwinging = false;

    Transform _currentSwingable;
    ConstantForce _currentConstantForce;

    private MovementController movementController;
    private float defaultPlayerRotationSpeed;

    public override bool OnInteract(InteractionController controller)
    {
        return base.OnInteract(controller);
    }

    public override void OnInteractionEnd(InteractionController controller)
    {
        base.OnInteractionEnd(controller);
    }



}
