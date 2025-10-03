using System;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float gravityConstant;
    [SerializeField] private float areaRatio = 7;
    [SerializeField] private float verticalDrag;
    [SerializeField] private float horizontalDrag;
    
    [Header("Rotation")]
    [SerializeField] private float minTorque;
    [SerializeField] private float maxTorque;
    [SerializeField] private float torqueDrag;
    
    [Header("Thrust")]
    [SerializeField] private float thrustMultiplier;
    [SerializeField, Space] private float deltaThrottle;
    [SerializeField, Range(0, 1)] private float throttle;
    [SerializeField, Range(0, 1)] private float minThrottle;
    
    [Header("Fuel")]
    [SerializeField] private float startingFuel;
    [SerializeField] private float fuel;
    
    private Rigidbody2D _rigidbody;
    
    void Awake()
    {
        fuel = startingFuel;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        UpdateGravity();
        UpdateRotation();
        UpdateThrottle();
        UpdateThrustAndDrag();
        UpdateFuel();
    }

    private void UpdateGravity() => _rigidbody.AddForce(Vector3.down * gravityConstant);

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

    private void UpdateThrustAndDrag()
    {
        float thrustPercentage = Map(throttle, 0, 1, minThrottle, 1);
        float thrust = throttle == 0 ? 0 : thrustPercentage * thrustMultiplier;
        Vector2 thrustForce = -transform.up * thrust;
        
        float sine = Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad);
        float area = Map(Mathf.Abs(sine), 0, 1, 1, areaRatio);
        
        Vector2 dragForce = new(-_rigidbody.linearVelocityX * horizontalDrag, 
                                -_rigidbody.linearVelocityY * verticalDrag * area);
        
        _rigidbody.AddForce(new Vector2(thrustForce.x + dragForce.x, Mathf.Max(thrustForce.y, dragForce.y)));
        
        // Torque drag
        _rigidbody.totalTorque *= torqueDrag;
    }

    private void UpdateFuel()
    {
        fuel -= throttle;
    }

    private float Map(float x, float inputMin, float inputMax, float outputMin, float outputMax)
        => (x - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
}
