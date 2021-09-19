using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	public int id;
	public string endPoint;

	public bool isLocalPlayer;

	// Use this for initialization
	void Start () {
			
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isLocalPlayer) {
			NetworkManager.instance.SendTransform (id, transform.position);
			Move ();
		}
	}

	public float speed = 5f;

	private void Move(){
		float x = Input.GetAxis ("Horizontal");
		float y = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (x, y, 0f);
		if(movement.sqrMagnitude > 0.0001f)
			transform.Translate (movement * speed * Time.deltaTime, Space.Self);
	}
}
