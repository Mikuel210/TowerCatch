using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	[SerializeField] private Slider throttleSlider;
	[SerializeField] private Slider fuelSlider;
	[SerializeField] private float maxFuel = 1000;

	private void Start()
	{
		ShipController.Instance.OnThrottleChanged += UpdateThrottle;
		ShipController.Instance.OnFuelChanged += UpdateFuel;
		UpdateFuel(ShipController.Instance.Fuel);
	}
	
	private void UpdateThrottle(float throttle) => throttleSlider.value = throttle;
	private void UpdateFuel(float fuel) => fuelSlider.value = fuel / maxFuel;

}
