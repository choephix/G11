using UnityEngine;

public class ActivateTrigger : MonoBehaviour {
	internal enum Mode {
		Trigger   = 0, // Just broadcast the action on to the attackee
		Replace   = 1, // replace attackee with source
		Activate  = 2, // Activate the attackee GameObject
		Enable    = 3, // Enable a component
		Animate   = 4, // Start animation on attackee
		Deactivate= 5 // Decativate attackee GameObject
	}

	/// The action to accomplish
	internal Mode action = Mode.Activate;

	/// The game object to affect. If none, the trigger work on this game object
	internal Object target;
	internal GameObject source;
	internal int triggerCount = 1;///
	internal bool repeatTrigger = false;
	
	void DoActivateTrigger () {
		triggerCount--;

		if (triggerCount == 0 || repeatTrigger) {
			Object currentTarget = target ?? gameObject;
			Behaviour targetBehaviour = currentTarget as Behaviour;
			GameObject targetGameObject = currentTarget as GameObject;
			if (targetBehaviour != null)
				targetGameObject = targetBehaviour.gameObject;
		
			switch (action) {
				case Mode.Trigger:
					targetGameObject.BroadcastMessage ("DoActivateTrigger");
					break;
				case Mode.Replace:
					if (source != null) {
						Object.Instantiate (source, targetGameObject.transform.position, targetGameObject.transform.rotation);
						DestroyObject (targetGameObject);
					}
					break;
				case Mode.Activate:
					targetGameObject.active = true;
					break;
				case Mode.Enable:
					if (targetBehaviour != null)
						targetBehaviour.enabled = true;
					break;	
				case Mode.Animate:
					targetGameObject.animation.Play ();
					break;	
				case Mode.Deactivate:
					targetGameObject.active = false;
					break;
			}
		}
	}

	void OnTriggerEnter (Collider other) {
		DoActivateTrigger ();
	}
}