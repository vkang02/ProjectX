using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;
    Vector2 movementInput;

    Rigidbody2D rb;

    Animator animator;

    SpriteRenderer spriteRenderer;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (movementInput != Vector2.zero)
        {
            bool success = TryMove(movementInput);

            if (!success)
            {
                success = TryMove(new Vector2(movementInput.x, 0));

                if (!success)
                {
                    success = TryMove(new Vector2(0, movementInput.y));
                }
            }

            // Set movement animation states
            bool isMovingSideways = Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y);
            bool isMovingBackwards = movementInput.y > 0 && !isMovingSideways;

            animator.SetBool("isMovingSideways", isMovingSideways);
            animator.SetBool(
                "isMovingForward",
                movementInput.y < 0 && !isMovingSideways && success
            );
            animator.SetBool("isMovingBackwards", isMovingBackwards && success);
        }
        else
        {
            // Reset movement states when not moving
            animator.SetBool("isMovingSideways", false);
            animator.SetBool("isMovingForward", false);
            animator.SetBool("isMovingBackwards", false);
        }

        // Handle sprite orientation
        if (animator.GetBool("isMovingSideways"))
        {
            // Flip sprite based on horizontal movement
            if (movementInput.x != 0)
            {
                spriteRenderer.flipX = movementInput.x < 0;
            }
        }
        else
        {
            // Reset horizontal flip when moving vertically
            spriteRenderer.flipX = false;
        }
    }

    private bool TryMove(Vector2 direction)
    {
        int count = rb.Cast(
            direction, // The direction to cast in
            movementFilter, // The settings that determine where a collision can occur
            castCollisions, // List of collisions to store the found collisions into after the Cast is finished
            moveSpeed * Time.fixedDeltaTime + collisionOffset // The amount to cast equal to the movement plus an offset
        );

        if (count == 0)
        {
            // If no collisions were found, move the player
            rb.MovePosition(rb.position + direction * (moveSpeed * Time.fixedDeltaTime));
            return true;
        }
        return false;
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
