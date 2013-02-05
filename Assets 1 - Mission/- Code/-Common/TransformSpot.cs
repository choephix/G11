using UnityEngine;
using System.Collections;

public class TransformSpot : MonoBehaviour {

	public void Attach( Transform child, bool scaleToo = false ) {

		child.parent = transform;
		child.localPosition = Vector3.zero;
		child.localRotation = Quaternion.identity;

		if( scaleToo ) {
			child.localScale = Vector3.one;
		}

	}

}
