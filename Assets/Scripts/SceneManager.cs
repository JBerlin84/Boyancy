using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour {

    public GameObject floater;
    public Text text;

    int counter = 0;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SpawnFloaters() {
        for (int j = 0; j < 2; j++) {
            Vector3 rotation = new Vector3(Random.value * 360, Random.value * 360, Random.value * 360);
            Instantiate(floater, new Vector3(0, 10 + (j * 3), j), Quaternion.Euler(rotation));
            counter++;
        }

        text.text = "Floaters: " + counter;
    }
}
