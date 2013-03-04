using UnityEngine;
using System.Collections;

public class InspectionGod : MonoBehaviour {

	public Transform[] prefabs;
	public Color[] colors;

	public Transform targetParent;

	private Transform currentSubject;
	private int index = 0;

	private int clrIndex = 0;

	private int clrLen = 0;
	private int len = 0;

	// Use this for initialization
	void Start () {

		len = prefabs.Length;
		clrLen = colors.Length;

		load();
	
	}
	
	// Update is called once per frame
	void Update() {

		if( Input.GetButtonDown( "Next" ) ) {

			index++;

			load();

		}

		if( Input.GetButtonDown( "Previous" ) ) {

			index--;

			load();

		}

		if( Input.GetButtonDown( "Confirm" ) ) {

			clrIndex++;

			while( clrIndex >= clrLen ) {
				clrIndex -= clrLen;
			}

			setColor();

		}

	}

	private void setColor() {

		if( clrLen == 0 )
			return;

		currentSubject.renderer.material.color = colors[clrIndex];

	}

	void load() {

		if( len == 0 ) 
			return;

		if( currentSubject != null ) {
			Destroy( currentSubject.gameObject );
		}

		while( index < 0 ) {
			index += len;
		}

		while( index >= len ) {
			index -= len;
		}

		currentSubject = (Transform)( Instantiate( prefabs[index], targetParent.position, prefabs[index].rotation ) );
		currentSubject.parent = targetParent;

		if( index < 19 ) {
			currentSubject.localPosition -= Vector3.up * .25f;
		}

	}

}
