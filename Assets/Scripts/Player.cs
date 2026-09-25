using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private struct DelayedInputState
    {
        public float timestamp;
        public bool jumpPressed;
        public bool jumpHeld;
        public bool jumpReleased;
        public bool downHeld;
    }

    private CharacterController character;
    private AnimatedSprite animatedSprite;
    private Vector3 direction;
    private Vector3 startingPosition;
    private Vector3 startingViewportPosition;
    private Camera mainCamera;
    private bool hasStartingViewportPosition;
    private Queue<DelayedInputState> inputQueue = new Queue<DelayedInputState>();
    private DelayedInputState delayedInput;

    public float inputDelayMs = 200f; // input delay in milliseconds
    public float gravity = 9.81f * 2f; //default, can be changed in editor
    public float jumpForce = 8f;
    public float fastFallMultiplier = 2f; // multiplier applied to gravity when holding down to fall faster
    public float maxJumpHoldTime = 0.15f; // how long extra upward force can be applied while holding jump
    public float jumpHoldForce = 15f; // additional upward force applied while holding jump

    private bool isJumping = false;
    private float jumpHoldTimer = 0f;
    public float jumpBufferTime = 0.12f; // how long a jump press is remembered before landing

    private float lastJumpPressedTime = -Mathf.Infinity;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
        animatedSprite = GetComponent<AnimatedSprite>();
        startingPosition = transform.position;
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            startingViewportPosition = mainCamera.WorldToViewportPoint(startingPosition);
            hasStartingViewportPosition = true;
        }
    }

    private void OnEnable()
    {
        ResetState();
    }

    public void ResetState()
    {
        Vector3 resetPosition = GetResponsiveStartingPosition();
        if (character != null)
        {
            character.enabled = false;
        }

        transform.position = resetPosition;
        direction = Vector3.zero;
        isJumping = false;
        jumpHoldTimer = 0f;
        lastJumpPressedTime = -Mathf.Infinity;
        delayedInput = default;
        inputQueue.Clear();

        if (character != null)
        {
            character.enabled = true;
        }

        animatedSprite = animatedSprite ?? GetComponent<AnimatedSprite>();
        if (animatedSprite != null)
        {
            animatedSprite.PlayAnimation(AnimatedSprite.AnimationState.Run, true);
        }
    }

    private Vector3 GetResponsiveStartingPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return startingPosition;
        }

        if (!hasStartingViewportPosition)
        {
            startingViewportPosition = mainCamera.WorldToViewportPoint(startingPosition);
            hasStartingViewportPosition = true;
        }

        Vector3 viewportPosition = startingViewportPosition;
        viewportPosition.z = startingPosition.z - mainCamera.transform.position.z;

        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
        worldPosition.z = startingPosition.z;
        return worldPosition;
    }

    private void Update()
    {
        // capture raw input and queue it with a timestamp
        DelayedInputState rawInput = new DelayedInputState
        {
            timestamp = Time.time,
            jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W),
            jumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.W),
            jumpReleased = Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.W),
            downHeld = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)
        };
        inputQueue.Enqueue(rawInput);
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;

        // process delayed input once the queued frame is old enough
        delayedInput.jumpPressed = false;
        delayedInput.jumpReleased = false;

        float delaySeconds = inputDelayMs * 0.001f;
        while (inputQueue.Count > 0 && Time.time >= inputQueue.Peek().timestamp + delaySeconds)
        {
            var nextInput = inputQueue.Dequeue();
            delayedInput.jumpPressed |= nextInput.jumpPressed;
            delayedInput.jumpReleased |= nextInput.jumpReleased;
            delayedInput.jumpHeld = nextInput.jumpHeld;
            delayedInput.downHeld = nextInput.downHeld;
        }

        // allow the player to fall faster while holding Down/S (only when airborne)
        float currentGravity = gravity;
        if (!character.isGrounded && delayedInput.downHeld)
        {
            currentGravity *= fastFallMultiplier;
        }

        // record jump input for buffering using delayed input
        if (delayedInput.jumpPressed)
        {
            lastJumpPressedTime = Time.time;
        }

        // Jump start and hold-to-jump behavior with buffering
        if (character.isGrounded)
        {
            direction = Vector3.down; // when player is on the ground apply constant force
            isJumping = false;
            jumpHoldTimer = 0f;
        }

        // If the player pressed jump recently (buffer) and is grounded, start the jump
        if (character.isGrounded && Time.time - lastJumpPressedTime <= jumpBufferTime)
        {
            direction = Vector3.up * jumpForce;
            isJumping = true;
            jumpHoldTimer = 0f;
            lastJumpPressedTime = -Mathf.Infinity; // consume the buffered input
            animatedSprite?.PlayAnimation(AnimatedSprite.AnimationState.Jump);
        }

        // while in the air: if the jump button is held and we haven't exceeded max hold time,
        // apply a small additional upward force to allow variable jump height.
        if (isJumping && delayedInput.jumpHeld && jumpHoldTimer < maxJumpHoldTime && direction.y > 0f)
        {
            direction += Vector3.up * jumpHoldForce * fixedDeltaTime;
            jumpHoldTimer += fixedDeltaTime;
        }

        // releasing the button stops applying extra upward force
        if (delayedInput.jumpReleased)
        {
            isJumping = false;
        }

        // apply gravity after any upward hold force
        direction += Vector3.down * currentGravity * fixedDeltaTime;

        // apply dir to character
        character.Move(direction * fixedDeltaTime);

        if (character.isGrounded && !isJumping)
        {
            animatedSprite?.PlayAnimation(AnimatedSprite.AnimationState.Run, false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}
