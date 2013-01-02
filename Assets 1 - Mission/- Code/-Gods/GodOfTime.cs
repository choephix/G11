using UnityEngine;
using System.Collections;

public class GodOfTime : MonoBehaviour {

	public static float globalSpeed = 1f;
	public static float speed = 1f;

	public static float time { get; private set; }

	public static float deltaTime { get; private set; }

	public static float realTime { get; private set; }

	void Update() {

		deltaTime = Time.deltaTime * speed * globalSpeed;
		time += deltaTime;

	}

}
