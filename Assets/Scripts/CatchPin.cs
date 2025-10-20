using System;
using UnityEngine;

public class CatchPin : Singleton<CatchPin>
{

    public event Action OnCatch;

    [SerializeField] private float requiredTime = 1;
    private int _inContact;
    private float _contactTime;
    private bool _caught;

    private void Start() => ShipController.Instance.OnRud += () => _caught = true;

    private void FixedUpdate()
    {
        _inContact--;
        if (_inContact <= 0) _contactTime = 0;

        if (_contactTime < requiredTime || _caught) return;
        
        OnCatch?.Invoke();
        _caught = true;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Chopsticks")) return;

        _inContact = 2;
        _contactTime += Time.fixedDeltaTime;
    }

}
