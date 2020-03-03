using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0f, 5)] int maxAirJumps = 2;
    [SerializeField, Range(0f, 100f)] float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 40f;
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;

    bool onGround;
    bool triggerJump;
    float minGroundDotProduct;
    int jumpPhase;
    Rigidbody body;
    Vector3 contactNormal;
    Vector3 desiredVelocity;
    Vector3 velocity;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
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
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            // Limit how fast we can jump verticaly
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += contactNormal * jumpSpeed;
        }
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                onGround = true;
                contactNormal = normal;
            }
        }
    }

    void UpdateState()
    {
        velocity = body.velocity;
        if (onGround)
        {
            jumpPhase = 0;
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }
}
