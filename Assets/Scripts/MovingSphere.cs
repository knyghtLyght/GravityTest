using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0f, 5)] int maxAirJumps = 2;

    bool onGround;
    bool triggerJump;
    int jumpPhase;
    Rigidbody body;
    Vector3 velocity;
    Vector3 desiredVelocity;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        // Get player movement
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical"); 
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        // Jump
        triggerJump |= Input.GetButtonDown("Jump");
    }
    private void FixedUpdate()
    {
        UpdateState();

        // Movement
        float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        // Jump
        if (triggerJump)
        {
            triggerJump = false;
            Jump();
        }
        // Apply movement
        body.velocity = velocity;

        onGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void Jump()
    {
        if (onGround || jumpPhase < maxAirJumps)
        {
            jumpPhase++;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            // Limit how fast we can jump verticaly
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            velocity.y += jumpSpeed;
        }
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            onGround |= normal.y >= 0.9f;
        }
    }

    void UpdateState()
    {
        velocity = body.velocity;
        if (onGround)
        {
            jumpPhase = 0;
        }
    }
}
