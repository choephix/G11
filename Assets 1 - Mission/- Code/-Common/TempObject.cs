using UnityEngine;
using System.Collections;

public class TempObject : WorldObject {

	public bool randomRotationY = false;

	public EventHandler eventDeath  = delegate { };

	void Start() {
		if( randomRotationY ) {
			transform.eulerAngles = Vector3.up * rand * 360;
		}
	}

	public void Die() {

		eventDeath.Invoke();
		eventDeath = delegate { };
		GameObject.Destroy( gameObject );

	}

}
