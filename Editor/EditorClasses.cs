using UnityEngine;
using UnityEditor;
using System.Collections;
 
 
public class EditorExtensions : ScriptableObject
{
 
    [MenuItem ("Extended Actions/Create Empty Object")]
    static void MenuAddObject()
    {
        GameObject newChild = new GameObject("_Empty");
		newChild.transform.localPosition = Vector3.zero;
    }
 
    [MenuItem ("Extended Actions/Create Empty Object As Child")]
    static void MenuAddChild()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);
 
        foreach(Transform transform in transforms)
        {
            GameObject newChild = new GameObject("_Empty");
            newChild.transform.parent = transform;
			newChild.transform.localPosition = Vector3.zero;
        }
    }
	
    [MenuItem ("Extended Actions/Create Empty Object As Parent")]
    static void MenuInsertParent()
    {
        Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel |SelectionMode.OnlyUserModifiable);
 
        GameObject newParent = new GameObject("_Empty");
        Transform newParentTransform = newParent.transform;
 
        if(transforms.Length == 1)
        {
            Transform originalParent = transforms[0].parent;
            transforms[0].parent = newParentTransform;
            if(originalParent)
                newParentTransform.parent = originalParent;
        }
        else
        {
            foreach(Transform transform in transforms)
                transform.parent = newParentTransform;
        }
    }
 
}