using UnityEngine;

public class TowerController : MonoBehaviour
{

    [Header("Chopsticks")]
    [SerializeField] private Transform _chopstickParent;
    [SerializeField] private Transform _chopsticks;
    [SerializeField] private float inMin;
    [SerializeField] private float inMax;
    [SerializeField] private float outMin;
    [SerializeField] private float outMax;

    private Transform _ship;
    
    void Start() => _ship = ShipController.Instance.transform;
    
    void Update()
    {
        float distance = Vector2.Distance(_ship.position, _chopsticks.position);
        float scale = MapClamp(distance, inMin, inMax, outMin, outMax);
        _chopstickParent.localScale = new(Mathf.Max(scale, _chopstickParent.localScale.x), 1);
    }

    private float MapClamp(float x, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        float mapping = (x - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;

        if (outputMin < outputMax) return Mathf.Clamp(mapping, outputMin, outputMax);
        return Mathf.Clamp(mapping, outputMax, outputMin);
    }
}