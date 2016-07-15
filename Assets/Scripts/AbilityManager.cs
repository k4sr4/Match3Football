using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AbilityManager : MonoBehaviour {

    public GameObject[] abilities;
    public Text abilityText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.inputString != "")
        {            
            int choice = Int32.Parse(Input.inputString);
            abilityText.text = abilities[choice - 1].GetComponent<Ability>().message;
            StartCoroutine(DisplayText());
            abilities[choice - 1].GetComponent<Ability>().Execute();
        }
	}

    private IEnumerator DisplayText()
    {
        abilityText.enabled = true;

        yield return new WaitForSeconds(1.5f);

        abilityText.enabled = false;
    }
}
