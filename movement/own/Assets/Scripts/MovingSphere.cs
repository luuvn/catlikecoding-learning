using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MovingSphere : MonoBehaviour
{

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f;

    Rigidbody body;

    Vector3 velocity, desiredVelocity;

    Vector3 contactNormal;

    bool desiredJump;

    int groundContactCount;

    bool OnGround => groundContactCount > 0;

    int jumpPhase;

    float minGroundDotProduct;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");

        if (groundContactCount > 1)
            GetComponent<Renderer>().material.SetColor(
                "_Color", Color.white * (groundContactCount * 0.25f)
            );
    }

    void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;

            Jump();
        }

        body.velocity = velocity;

        ClearState();
    }

    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    void UpdateState()
    {
        velocity = body.velocity;

        if (OnGround)
        {
            jumpPhase = 0;

            if (groundContactCount > 1)
                contactNormal = contactNormal.normalized;
        }
        else
            contactNormal = Vector3.up;
    }

    void AdjustVelocity()
    {
        float maxAccelerationLocal = OnGround ? maxAcceleration : maxAirAcceleration;
        float deltaAcceleration = maxAccelerationLocal * Time.deltaTime;

        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(xAxis, velocity);
        float currentZ = Vector3.Dot(zAxis, velocity);

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, deltaAcceleration);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, deltaAcceleration);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void Jump()
    {
        if (OnGround && jumpPhase < maxAirJumps)
        {
            float jumpSpeed = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
            float combinedSpeed = Vector3.Dot(velocity, contactNormal);

            if (combinedSpeed > 0f)
                jumpSpeed = Mathf.Max(jumpSpeed - combinedSpeed, 0f);

            velocity += jumpSpeed * contactNormal;

            jumpPhase++;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (normal.y >= minGroundDotProduct)
            {
                groundContactCount++;
                contactNormal += normal;
            }
        }
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }
}