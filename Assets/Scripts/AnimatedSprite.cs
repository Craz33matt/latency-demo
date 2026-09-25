using Unity.VisualScripting;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
    public enum AnimationState
    {
        Run,
        Jump
    }

    public Sprite[] sprites;
    public Sprite[] jumpSprites;
    public AnimationState startingState = AnimationState.Run;
    public bool loopRunAnimation = true;
    public bool loopJumpAnimation = false;

    private SpriteRenderer spriteRenderer;
    private int frame;
    private AnimationState currentState;
    private Sprite[] activeSprites;
    private bool isLooping;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayAnimation(startingState, true);
    }

    private void OnDisable()
    {
        CancelInvoke(); // stops animating when the script is disabled (GAME OVER)
    }

    public void PlayAnimation(AnimationState newState, bool restart = true)
    {
        if (newState == currentState && !restart)
            return;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
            return;

        CancelInvoke();

        currentState = newState;
        activeSprites = GetSpritesForState(currentState);
        frame = -1;
        isLooping = (currentState == AnimationState.Run) ? loopRunAnimation : loopJumpAnimation;

        if (activeSprites != null && activeSprites.Length > 0)
        {
            spriteRenderer.sprite = activeSprites[0];
        }

        Invoke(nameof(Animate), 0f);
    }

    private Sprite[] GetSpritesForState(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Jump:
                return (jumpSprites != null && jumpSprites.Length > 0) ? jumpSprites : sprites;
            case AnimationState.Run:
            default:
                return sprites;
        }
    }

    private void Animate()
    {
        if (activeSprites == null || activeSprites.Length == 0)
        {
            Invoke(nameof(Animate), 1f / GameManager.Instance.gameSpeed);
            return;
        }

        frame++;
        if (frame >= activeSprites.Length) // current frame past array of animation
        {
            if (isLooping)
            {
                frame = 0; //start back at 0
            }
            else
            {
                frame = activeSprites.Length - 1; // keep last frame
            }
        }

        if (frame >= 0 && frame < activeSprites.Length) // check for if you forget to put sprite in array
        {
            spriteRenderer.sprite = activeSprites[frame];
        }

        if (isLooping || frame < activeSprites.Length - 1)
        {
            Invoke(nameof(Animate), 1f / GameManager.Instance.gameSpeed);
        }
        // as gamespeed increases the animation time gets faster ^
    }
}
