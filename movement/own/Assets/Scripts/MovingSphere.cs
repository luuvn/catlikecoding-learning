using UnityEngine;

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
    }

    void UpdateState()
    {
        velocity = body.velocity;

        if (OnGround)
            jumpPhase = 0;
    }

    void AdjustVelocity()
    {
        float maxAccelerationLocal = OnGround ? maxAcceleration : maxAirAcceleration;
        float deltaAcceleration = maxAccelerationLocal * Time.deltaTime;
        float newX = Mathf.MoveTowards(velocity.x, desiredVelocity.x, deltaAcceleration);
        float newZ = Mathf.MoveTowards(velocity.z, desiredVelocity.z, deltaAcceleration);

        velocity.x = newX;
        velocity.z = newZ;
    }

    void Jump()
    {
        if (OnGround && jumpPhase < maxAirJumps)
        {
            float jumpSpeed = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);

            if (velocity.y > 0f)
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);

            velocity.y += jumpSpeed;

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
            ContactPoint con = collision.GetContact(i);
            if (con.normal.y >= minGroundDotProduct)
                groundContactCount++;
        }
    }

    // Vector3 ProjectOnContactPlane (Vector3 vector) {
    // }
}