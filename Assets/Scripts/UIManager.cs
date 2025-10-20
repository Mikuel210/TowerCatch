using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[SerializeField] private Slider throttleSlider;
	[SerializeField] private Slider fuelSlider;
	[SerializeField] private float maxFuel = 1000;

	[SerializeField, Space] private TextMeshProUGUI altitudeText;
	[SerializeField] private TextMeshProUGUI velocityText;
	
	[SerializeField, Space] private GameObject winPanel;

	private ShipController _ship;
	private Rigidbody2D _shipRigidbody;

	void Start()
	{
		_ship = ShipController.Instance;
		_shipRigidbody = _ship.GetComponent<Rigidbody2D>();
		
		_ship.OnThrottleChanged += UpdateThrottle;
		_ship.OnFuelChanged += UpdateFuel;
		
		UpdateThrottle(ShipController.Instance.Throttle);
		UpdateFuel(ShipController.Instance.Fuel);

		CatchPin.Instance.OnCatch += () => winPanel.SetActive(true);
	}

	private const float UNITS_RATIO = 13;
	
	void FixedUpdate()
	{
		altitudeText.text = (int)((_ship.transform.position.y + 14) * UNITS_RATIO) + "m";
		velocityText.text = (int)(_shipRigidbody.linearVelocity.magnitude * UNITS_RATIO) + "m/s";
	}
	
	private void UpdateThrottle(float throttle) => throttleSlider.value = throttle;
	private void UpdateFuel(float fuel) => fuelSlider.value = fuel / maxFuel;

	public void NextLevel() => LevelManager.Instance.NextLevel();

}
