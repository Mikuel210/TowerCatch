using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Animator _animator;
    private Transform _ship;
    
    void Start()
    {
        _animator = GetComponent<Animator>();
        _ship = ShipController.Instance.transform;
    }
    
    void Update() => _animator.SetFloat("Altitude", _ship.position.y);
}
