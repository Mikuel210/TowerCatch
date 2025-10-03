using System;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float gravityConstant;
    [SerializeField] private float areaRatio = 7;
    [SerializeField] private float terminalVelocityMultiplier;
    
    [Header("Rotation")]
    [SerializeField] private float minTorque;
    [SerializeField] private float maxTorque;
    
    [Header("Thrust")]
    [SerializeField] private float thrustMultiplier;
    [SerializeField] private float horizontalThrustDivider;
    
    [SerializeField, Space] private float deltaThrottle;
    [SerializeField, Range(0, 1)] private float throttle;
    [SerializeField, Range(0, 1)] private float minThrottle;
    
    private Rigidbody2D _rigidbody;
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        UpdateGravity();
        UpdateRotation();
        UpdateThrottle();
        UpdateThrust();
        UpdateDrag();
    }

    private void UpdateGravity()
    {
        float cosine = Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad);
        float terminalVelocity = Map(Mathf.Abs(cosine), 0, 1, 1, areaRatio);
        terminalVelocity *= -terminalVelocityMultiplier;

        float currentVelocity = _rigidbody.linearVelocityY;
        float correction = (terminalVelocity - currentVelocity) * gravityConstant;
        _rigidbody.AddForce(Vector3.up * correction);
    }

    private void UpdateRotation()
    {
        float input = Input.GetAxis("Horizontal");
        float multiplier = Map(throttle, 0, 1, minTorque, maxTorque);
        _rigidbody.AddTorque(-input * multiplier);
    }

    private void UpdateThrottle()
    {
        float input = Input.GetAxisRaw("Vertical");
        throttle = Mathf.Clamp01(throttle + input * deltaThrottle);
    }

    private void UpdateThrust()
    {
        float thrustPercentage = Map(throttle, 0, 1, minThrottle, 1);
        float thrust = throttle == 0 ? 0 : thrustPercentage * thrustMultiplier;

        Vector2 force = -transform.up * thrust;
        force = new Vector2(force.x / horizontalThrustDivider, force.y);
        _rigidbody.AddForce(force);
    }

    private void UpdateDrag()
    {
        // TODO Horizontal air drag
    }

    private float Map(float x, float inputMin, float inputMax, float outputMin, float outputMax)
        => (x - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
}
