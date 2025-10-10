using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {

	public static T Instance { get; private set; }

	public Singleton() => Instance = this as T;

}