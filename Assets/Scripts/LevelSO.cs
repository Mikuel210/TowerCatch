using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Level", fileName = "New Level")]
public class LevelSO : ScriptableObject
{

	public Vector2 startingPosition;
	public float startingAngle;
	public float startingFuel;
	
}