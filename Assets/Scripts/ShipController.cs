using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float gravityConstant;
    [SerializeField] private float areaRatio = 7;
    [SerializeField] private float verticalDrag;
    [SerializeField] private float horizontalDrag;
    [SerializeField] private float slidingFactor;
    
    [Header("Rotation")]
    [SerializeField] private float minTorque;
    [SerializeField] private float maxTorque;
    
    [Header("Thrust")]
    [SerializeField] private float thrustMultiplier;
    [SerializeField] private float gimbalThrustDivider = 1;
    [SerializeField] private float horizontalThrustDivider = 1;
    
    [SerializeField, Space] private float deltaThrottle;
    [SerializeField, Range(0, 1)] private float throttle;
    [SerializeField, Range(0, 1)] private float minThrottle;
    
    [Header("Fuel")]
    [SerializeField] private float startingFuel;
    [SerializeField] private float fuel;

    [Header("Engines")] 
    [SerializeField] private float maxParticleSpeed;
    [SerializeField] private float engineDispersion;
    [SerializeField] private float engineGimbal;
    [SerializeField] private float maxEngineGimbal;
    [SerializeField, Range(0, 1)] private float gimbalSmoothing;
    [SerializeField] private List<ParticleSystem> engines;
    
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    
    void Awake()
    {
        fuel = startingFuel;
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        UpdateGravity();
        UpdateRotation();
        UpdateThrottle();
        UpdateThrustAndDrag();
        UpdateSliding();
        UpdateFuel();
        UpdateEngineVisuals();
    }

    private void UpdateGravity() => _rigidbody.AddForce(Vector3.down * gravityConstant);

    private void UpdateRotation()
    {
        float input = Input.GetAxis("Horizontal");
        float thrustPercentage = Map(throttle, 0, 1, minThrottle, 1);
        thrustPercentage = Mathf.Approximately(thrustPercentage, minThrottle) ? 0 : thrustPercentage;
        
        float multiplier = Map(thrustPercentage, 0, 1, minTorque, maxTorque);
        _rigidbody.AddTorque(-input * multiplier);
        
        _animator.SetInteger("Input", (int)Input.GetAxisRaw("Horizontal"));
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
        thrustForce = new(thrustForce.x / horizontalThrustDivider, thrustForce.y);

        float divider = gimbalThrustDivider * Mathf.Abs(Input.GetAxis("Horizontal"));
        divider = Map(divider, 0, gimbalThrustDivider, 1, gimbalThrustDivider);
        thrustForce /= divider;
        
        float sine = Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad);
        float area = Map(Mathf.Abs(sine), 0, 1, 1, areaRatio);
        
        Vector2 dragForce = new(-_rigidbody.linearVelocityX * horizontalDrag, 
                                -_rigidbody.linearVelocityY * verticalDrag * area);
        
        _rigidbody.AddForce(new Vector2(thrustForce.x + dragForce.x, Mathf.Max(thrustForce.y, dragForce.y)));
    }

    private void UpdateSliding()
    {
        float sine = Mathf.Sin(transform.eulerAngles.z * 2 * Mathf.Deg2Rad);
        _rigidbody.AddForce( _rigidbody.linearVelocityY * sine * slidingFactor * Vector3.left);
    }

    private void UpdateFuel()
    {
        fuel -= throttle;
    }

    private void UpdateEngineVisuals()
    {
        UpdateEngineThrottle();
        UpdateEngineGimbal();
    }

    private void UpdateEngineThrottle()
    {
        if (throttle == 0)
        {
            foreach (ParticleSystem engine in engines)
            {
                if (!engine.isStopped)
                    engine.Stop();
            }
                
            return;
        }

        float engineStep = 1 / (float)engines.Count;
        
        for (int i = 0; i < engines.Count; i++)
        {
            ParticleSystem engine = engines[i];
            float threshold = engineStep * i;
            bool state = throttle >= threshold; 
            
            if (state && engine.isStopped)
                engine.Play();
            
            if (!state && engine.isPlaying)
            {
                engine.Stop();
                continue;
            }

            float engineThrottle = Map(throttle, threshold, threshold + engineStep, minThrottle, 1);
            engineThrottle = Mathf.Clamp01(engineThrottle);

            var mainModule = engine.main;
            mainModule.startSpeedMultiplier = maxParticleSpeed * thrustMultiplier;
        }
    }

    private void UpdateEngineGimbal()
    {
        float engineStep = 1 / (float)engines.Count;
        float dispersion = engineStep - throttle % engineStep;
        dispersion = Map(dispersion, 0, engineStep, 0, engineDispersion);
        dispersion = throttle < engineStep ? 0 : dispersion;

        float input = Input.GetAxisRaw("Horizontal");
        float angle = input * engineGimbal;

        for (int i = 0; i < engines.Count; i++)
        {
            Transform engine = engines[i].transform;
            
            float engineAngle = angle + dispersion * (1 - i / (float)engines.Count * 3f);
            engineAngle = Mathf.Clamp(engineAngle, -maxEngineGimbal, maxEngineGimbal);
            engineAngle = Mathf.LerpAngle(engine.localEulerAngles.z, engineAngle, gimbalSmoothing);
            engine.localEulerAngles = new(0, 0, engineAngle);
        }
    }

    private float Map(float x, float inputMin, float inputMax, float outputMin, float outputMax)
        => (x - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
}