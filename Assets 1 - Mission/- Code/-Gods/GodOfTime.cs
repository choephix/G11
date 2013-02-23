using UnityEngine;
using System.Collections;

public class GodOfTime : MonoBehaviour {

	private const float MAX_DELTA = .25f;

	public static float globalSpeed = 1f;
	public static float speed = 1f;

	public static float time { get; private set; }

	public static float deltaTime { get; private set; }

	public static float realTime { get; private set; }

	void Update() {

		realTime = Time.deltaTime;

		if( realTime > MAX_DELTA ) {
			realTime = MAX_DELTA;
		}

		deltaTime = realTime * speed * globalSpeed;

		time += deltaTime;

	}

}
