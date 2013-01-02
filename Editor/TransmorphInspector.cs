// C# example: A custom Inspector for Transform components.
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Transmorph ) )]
public class TransmorphInspector : Editor {

	SerializedObject m_Object;
	SerializedProperty m_Property;

	void OnEnable() {
		m_Object = new SerializedObject( target );
		m_Property = m_Object.FindProperty( "m_LocalPosition" );
	}

	public override void OnInspectorGUI() {

		EditorGUILayout.HelpBox( "Wazzaaaaaaaa", MessageType.None, true );

		// Grab the latest data from the object
		m_Object.Update();

		// Editor UI for the property
		EditorGUILayout.PropertyField( m_Property );

		// Apply the property, handle undo
		m_Object.ApplyModifiedProperties();

		base.OnInspectorGUI();
		
	}

}