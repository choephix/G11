using UnityEngine;
using System.Collections;










public class Sprite : MonoBehaviour {

	void Update() {
		//transform.LookAt( Camera.mainCamera.transform.position, Camera.mainCamera.transform.up );
		transform.rotation =( Camera.mainCamera.transform.rotation ) ;
	}

}
