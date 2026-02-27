using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioSource))]
//As it is a Physics interactable, it is apart of that thingy thing. We all know the thingy thing. I forget the name of it. 
public class RopeSwing : PhysicsInteractable
{
    //Before we begin I would like to note that the PushableInteractable script has helped me out a bunch in trying to figure out how to make the swing work. I decided for fun to test it out on the rope to see how it worked and it worked like a doll. If the code looks somewhat familar, that's why.

    [Tooltip("Force required to break the connection with the player")]
    [SerializeField] private float jointBreakForce = 200f;
    [Tooltip("Rotational force required to break the connection with the player")]
    [SerializeField] private float jointBreakTorque = 200f;

    //As stated, this applies force for when you jump off the vine. 
    [Tooltip("Force applied when you jump off the vine")]
    [SerializeField] float jumpForce = 15f;

    //It'll play audio as you swing
    [Tooltip("Plays Audio for when you're swinging")]
    [SerializeField] AudioSource _ropeSounds;
    [SerializeField] float minSwingVelocity = 0.5f;

    //This stuff came from the interactable script, but also calls the movement controller and the players rotation speed. It's not like the player can rotate anyways. 
    private MovementController movementController;
    private float defaultPlayerRotationSpeed;

    //This starts the ineraction, and says that it is interacting. 
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
            RotationSpeed();
        }

        AttachToController<FixedJoint>(controller);
        return true;
    }

    //This just ends the interaction. 
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
            RotationSpeed();
        }
        base.OnInteractionEnd(controller);
    }

    //If the joints break, ya gone
    private void OnJointBreak(float breakForce)
    {
        if (currentInteractor != null)
            currentInteractor.EndCurrentInteraction();
    }

    //This is here to check if you can swing jump 
    private void Update()
    {
        if (currentInteractor == null) return;

        //Checks to see if it's null. 
        if(_ropeSounds == null)
        {
            //Will add the Component
            _ropeSounds = GetComponent<AudioSource>();
            //then if it's null still, gives a debug log error. 
            if(_ropeSounds == null)
            {
                Debug.Log("How did we get here?");
            }
        }

        float swingSpeed = rb.linearVelocity.magnitude;

        //If the rope is swinging fast enough, will play sounds. 
        if (swingSpeed > minSwingVelocity)
        {
            if (!_ropeSounds.isPlaying)
                _ropeSounds.Play();

            //And changes the pitch
            _ropeSounds.pitch = Mathf.Clamp(swingSpeed / 5f, 0.8f, 1.5f);
            _ropeSounds.volume = Mathf.Clamp(swingSpeed / 5f, 0.2f, 1f);
        }
        else
        {
            if (_ropeSounds.isPlaying)
                _ropeSounds.Stop();
        }


        //This will allow you to jump off the rope as it is mid swing
        var playerInput = currentInteractor.GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions["Jump"].WasPressedThisFrame())
        {
            SwingJump();
        }
    }

    //This just has it so it is contsantly checking to see if drop down gets activated. 
    public override void OnInteractedAlreadyInteracting(InteractionController controller)
    {
        base.OnInteractedAlreadyInteracting(controller);
        DropDown();
    }

    //this will have it so that when you just end the interaction normally, it will just fall dowm
    protected void DropDown()
    {
        if (currentInteractor) OnInteractionEnd(currentInteractor);
    }

    //This will have it that when you Jump off the rope, it will send you flying. 
    protected void SwingJump()
    {
        if (currentInteractor == null) return;

        //Gets the player RigidBody, so you can send it .
        Rigidbody playerRb = currentInteractor.GetComponent<Rigidbody>();

        //Gets the colliders for both player and rope
        Collider playerCollider = currentInteractor.GetComponent<Collider>();
        Collider ropeCollider = GetComponent<Collider>();

        // Temporarily ignore collision between player and rope
        if (playerCollider != null && ropeCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, ropeCollider, true);
        }
            

        //Makes a new vector 3 for the launch, with the player's linear velocity. 
        Vector3 launchVelocity = playerRb.linearVelocity;


        //Detatches the joints from the controller. 
        DetachFromController();

        //I dunno how else to describe it than "Does some mathy math to get the direction, then launcehs the player" so yeah it does that.
        Vector3 swingDirection = (transform.position - currentInteractor.transform.position).normalized;
        Vector3 jumpDir = swingDirection + Vector3.up * 0.3f; 
        playerRb.linearVelocity = launchVelocity + jumpDir.normalized * jumpForce;

        //stops the interaction
        if (currentInteractor) OnInteractionEnd(currentInteractor);

        //Reembales the collider
        StartCoroutine(ReenableCollision(playerCollider, ropeCollider, 1f));
    }

    //Reemables the collision for the player and the rope
    private IEnumerator ReenableCollision(Collider playerCollider, Collider ropeCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerCollider != null && ropeCollider != null)
            Physics.IgnoreCollision(playerCollider, ropeCollider, false);
    }

    //This just attachs the thing to the controller. 
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

    //This check your rotation speed, and resets it if it's not there. (Mostly for the SwingJump method so the player cam still rotate even if they don't hit the interact button. 
    private void RotationSpeed()
    {
        //If its above zero, it will set the rotation speed to zero so the player cannot rotate while swinging
        if(movementController.rotationSpeed > 0)
        {
            defaultPlayerRotationSpeed = movementController.rotationSpeed;
            movementController.rotationSpeed = 0;
        }
        //If its below zero however, it will reset it back to normal. 
        else
        {
            movementController.rotationSpeed = defaultPlayerRotationSpeed;
            movementController = null;
        }
    }

  



}
