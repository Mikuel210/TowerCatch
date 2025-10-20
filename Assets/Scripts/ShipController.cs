using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShipController : Singleton<ShipController>
{
    [Header("Physics")]
    [SerializeField] private float gravityConstant;
    [SerializeField] private float areaRatio = 7;
    [SerializeField] private float verticalDrag;
    [SerializeField] private float horizontalDrag;
    [SerializeField] private float slidingFactor;
    [SerializeField, Range(0, 1)] private float dryMass = 0.79f;
    
    [SerializeField, Space] private float rudForce;
    [SerializeField] private ParticleSystem rudParticles;
    
    [Header("Rotation")]
    [SerializeField] private float minTorque;
    [SerializeField] private float maxTorque;
    
    [Header("Thrust")]
    [field: SerializeField] public bool EnginesRunning { get; private set; }
    [SerializeField] private float thrustMultiplier;
    [SerializeField] private float gimbalThrustDivider = 1;
    [SerializeField] private float horizontalThrustDivider = 1;
    
    [field: SerializeField, Range(0, 1), Space] public float Throttle { get; private set; } = 1;
    [SerializeField, Range(0, 1)] private float minThrottle;
    [SerializeField] private float deltaThrottle;
    public event Action<float> OnThrottleChanged;

    [Header("Fuel")]
    [field: SerializeField] public float Fuel { get; private set; }
    public event Action<float> OnFuelChanged;

    [Header("Engines")] 
    [SerializeField] private float maxParticleSpeed;
    [SerializeField] private float engineDispersion;
    [SerializeField] private float engineGimbal;
    [SerializeField] private float maxEngineGimbal;
    [SerializeField, Range(0, 1)] private float gimbalSmoothing;
    
    [SerializeField, Space] private List<ParticleSystem> engines;
    [SerializeField] private List<Image> engineGraphics;
    [SerializeField] private GameObject engineShutdownParticles;
    
    // Private fields
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private float previousThrottle = 1;
    private float _startingFuel;
    
    void Awake()
    {
        var level = LevelManager.Instance.GetLevel();
        transform.position = level.startingPosition;
        transform.eulerAngles = new(0, 0, level.startingAngle);
        
        _startingFuel = level.startingFuel;
        Fuel = _startingFuel;
        
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
            EnginesRunning = !EnginesRunning;
    }

    void FixedUpdate() 
    {
        UpdateMass();
        UpdateGravity();
        UpdateRotation();
        UpdateThrottle();
        UpdateThrustAndDrag();
        UpdateSliding();
        UpdateFuel();
        UpdateEngineVisuals();
    }

    private void UpdateMass() => _rigidbody.mass = Map(Fuel, 0, _startingFuel, dryMass, 1);

    private void UpdateGravity() => _rigidbody.AddForce(Vector3.down * gravityConstant);

    private void UpdateRotation()
    {
        float input = Input.GetAxis("Horizontal");
        float thrustPercentage = Map(Throttle, 0, 1, minThrottle, 1);
        thrustPercentage = Mathf.Approximately(thrustPercentage, minThrottle) ? 0 : thrustPercentage;
        thrustPercentage = EnginesRunning ? thrustPercentage : 0;
        
        float multiplier = Map(thrustPercentage, 0, 1, minTorque, maxTorque);
        _rigidbody.AddTorque(-input * multiplier);
        
        _animator.SetInteger("Input", (int)Input.GetAxisRaw("Horizontal"));
    }

    private void UpdateThrottle() {
        float input = Input.GetAxisRaw("Vertical");
        Throttle = Mathf.Clamp01(Throttle + input * deltaThrottle);

        if (input != 0) OnThrottleChanged?.Invoke(Throttle);
    }

    private void UpdateThrustAndDrag()
    {
        Vector2 thrustForce = Vector2.zero;

        if (EnginesRunning) {
            float thrustPercentage = Map(Throttle, 0, 1, minThrottle, 1);
            float thrust = Throttle == 0 ? 0 : thrustPercentage * thrustMultiplier;
        
            thrustForce = -transform.up * thrust;
            thrustForce = new(thrustForce.x / horizontalThrustDivider, thrustForce.y);
            
            float divider = gimbalThrustDivider * Mathf.Abs(Input.GetAxis("Horizontal"));
            divider = Map(divider, 0, gimbalThrustDivider, 1, gimbalThrustDivider);
            thrustForce /= divider;
        }
        
        float sine = Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad);
        float area = Map(Mathf.Abs(sine), 0, 1, 1, areaRatio);
        
        Vector2 dragForce = new(-_rigidbody.linearVelocityX * horizontalDrag, 
                                -_rigidbody.linearVelocityY * verticalDrag * area);
        
        _rigidbody.AddForce(new(thrustForce.x + dragForce.x, Mathf.Max(thrustForce.y, dragForce.y)));
    }

    private void UpdateSliding()
    {
        float sine = Mathf.Sin(transform.eulerAngles.z * 2 * Mathf.Deg2Rad);
        _rigidbody.AddForce( _rigidbody.linearVelocityY * sine * slidingFactor * Vector3.left);
    }

    private void UpdateFuel()
    {
        float throttle = EnginesRunning ? Throttle : 0;
        Fuel = Mathf.Max(Fuel - throttle, 0);
        if (Fuel == 0) EnginesRunning = false;

        if (Throttle == 0) return;
        OnFuelChanged?.Invoke(Fuel);
    }

    private void UpdateEngineVisuals()
    {
        UpdateEngineThrottle();
        UpdateEngineGimbal();
    }

    private void UpdateEngineThrottle()
    {
        if (Throttle == 0 || !EnginesRunning)
        {
            foreach (ParticleSystem engine in engines)
            {
                if (!engine.isStopped)
                    StopEngine(engine, previousThrottle > 0);
            }
            
            goto End;
        }

        float engineStep = 1 / (float)engines.Count;
        
        for (int i = 0; i < engines.Count; i++)
        {
            ParticleSystem engine = engines[i];
            float threshold = engineStep * i;
            bool state = Throttle >= threshold;

            if (state && engine.isStopped) {
                engine.Play();
                
                // Show graphic
                engineGraphics[i].gameObject.SetActive(true);
            }
            
            if (!state && engine.isPlaying)
            {
                StopEngine(engine, previousThrottle > threshold);
                continue;
            }

            float engineThrottle = Map(Throttle, threshold, threshold + engineStep, minThrottle, 1);
            engineThrottle = Mathf.Clamp01(engineThrottle);

            var mainModule = engine.main;
            mainModule.startSpeedMultiplier = maxParticleSpeed * engineThrottle;
        }

        End:
        previousThrottle = Throttle;
        if (!EnginesRunning) previousThrottle = 0;
    }

    private void StopEngine(ParticleSystem engine, bool showParticles)
    {
        engine.Stop();
        
        // Hide graphic
        int engineNumber = engines.IndexOf(engine);
        engineGraphics[engineNumber].gameObject.SetActive(false);
                
        // Particle effect
        if (!showParticles) return;
                
        var particles = Instantiate(engineShutdownParticles, transform);
        particles.transform.localPosition = new(0, 0.5f, 0);
        particles.transform.parent = null;
        particles.transform.localScale = new(1, 1, 1);
        Destroy(particles, 3f);
    }

    private void UpdateEngineGimbal()
    {
        float engineStep = 1 / (float)engines.Count;
        float dispersion = engineStep - Throttle % engineStep;
        dispersion = Map(dispersion, 0, engineStep, 0, engineDispersion);
        dispersion = Throttle < engineStep ? 0 : dispersion;

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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.relativeVelocity.magnitude >= rudForce)
            RapidlyDisassemble();
    }

    private void RapidlyDisassemble() {
        // Particles
        var particles = Instantiate(rudParticles);
        particles.transform.position = transform.position;
        Destroy(particles, 10f);
        
        // Engine graphics
        foreach (var graphic in engineGraphics)
            graphic.gameObject.SetActive(false);
        
        // Disable ship
        gameObject.SetActive(false);
    }

    private float Map(float x, float inputMin, float inputMax, float outputMin, float outputMax)
        => (x - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
}