using System;
using System.Numerics;
using UnityEditor.iOS.Extensions.Common;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MovingSphere : MonoBehaviour
{
    [SerializeField]
    private Transform playerInputSpace = default;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;

    Vector3 velocity, desiredVelocity, connectionVelocity;

    Vector3 contactNormal, steepNormal;

    int groundContactCount, steepContactCount;

    bool desiredJump;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    bool OnGround => groundContactCount > 0;

    private bool OnSteep => steepContactCount > 0;

    private Rigidbody body, connectedBody, previousConnectedBody;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct;

    private Vector3 upAxis, rightAxis, forwardAxis;

    private Vector3 connectionWorldPosition, connectionLocalPosition;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        OnValidate();
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectOnContactPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectOnContactPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectOnContactPlane(Vector3.right, upAxis);
            forwardAxis = ProjectOnContactPlane(Vector3.forward, upAxis);
        }

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");

        GetComponent<Renderer>().material.SetColor(
            "_Color", OnGround ? Color.black : Color.white
        );
    }

    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;

            Jump(gravity);
        }

        velocity += gravity * Time.deltaTime;

        body.velocity = velocity;

        ClearState();
    }

    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;

        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;

            // Use to avoid reset too soon
            // After Jump(), still be OnGround one more time, then must use > 1, not >= 1
            if (stepsSinceLastJump > 1)
                jumpPhase = 0;

            if (groundContactCount > 1)
                contactNormal = contactNormal.normalized;
        }
        else
        {
            contactNormal = upAxis;
        }

        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = body.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    void AdjustVelocity()
    {
        float maxAccelerationLocal = OnGround ? maxAcceleration : maxAirAcceleration;
        float deltaAcceleration = maxAccelerationLocal * Time.deltaTime;

        Vector3 xAxis = ProjectOnContactPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectOnContactPlane(forwardAxis, contactNormal);

        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, deltaAcceleration);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, deltaAcceleration);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

       
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
            jumpDirection = contactNormal;
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            // Use when falling from plane
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
            return;

        stepsSinceLastJump = 0;
        jumpPhase += 1;

        float jumpSpeed = Mathf.Sqrt(2 * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float combinedSpeed = Vector3.Dot(velocity, jumpDirection);

        if (combinedSpeed > 0f)
            jumpSpeed = Mathf.Max(jumpSpeed - combinedSpeed, 0f);

        velocity += jumpSpeed * jumpDirection;
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
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);

            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else if (upDot > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
                if (groundContactCount == 0)
                {
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }

    Vector3 ProjectOnContactPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
            return false;

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
            return false;

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);

        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        connectedBody = hit.rigidbody;
        return true;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot > minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }

        return false;
    }
}