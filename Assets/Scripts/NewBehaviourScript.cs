using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log(name);
        Debug.Log("Position : " + transform.position);
        Debug.Log("LocalPos : " + transform.localPosition);
        Debug.Log("Forward : " + transform.forward);
        Debug.Log("Right : " + transform.right);
        Debug.Log("TransformVector : " + transform.InverseTransformVector(new Vector3(0f,0f,1f)));

	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(name);
        Debug.Log("Position : " + transform.position);
        Debug.Log("LocalPos : " + transform.localPosition);
        Debug.Log("Forward : " + transform.forward);
        Debug.Log("Right : " + transform.right);
        Debug.Log("TransformVector : " + transform.InverseTransformVector(new Vector3(0f, 0f, 1f)));
    }
}
