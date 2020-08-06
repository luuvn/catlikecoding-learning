using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    private Rigidbody body;

    private float floatDelay;

    [SerializeField] 
    private bool floatToSleep = false;

    // Use this for initialization
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (floatToSleep)
        {
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)
                {
                    return;
                }
            }
            else
            {
                floatDelay = 0f;
            }
        }

        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
    }
}
