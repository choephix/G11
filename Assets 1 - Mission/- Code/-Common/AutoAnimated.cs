using UnityEngine;
using System.Collections;

public class AutoAnimated : MonoBehaviour {

	public Vector3 rotationSpeed;

	void Update () {
		unchecked {
			transform.eulerAngles = rotationSpeed * GodOfTime.time;
		}
	}

}
