using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AbilityManager : MonoBehaviour {

    public GameObject[] abilities;
    public Text abilityText;

    public bool p1Lock = false;
    public bool p2Lock = false;

    public bool enoughResource = true;

    private bool enableAbilities = true;
    private int locked = 0;
    private ShapesManager shapesManager;

	// Use this for initialization
	void Start () {
        shapesManager = FindObjectOfType<ShapesManager>();
	}
	
	// Update is called once per frame
	void Update () {
        int turn = shapesManager.GetTurn();

        if (locked != turn && locked != 0)
        {
            p1Lock = false;
            p2Lock = false;
        }

        if (turn == 1 && p1Lock)
        {
            locked = 1;
            return;
        }
        if (turn == 2 && p2Lock)
        {
            locked = 2;
            return;
        }

        if (Input.inputString != "" && enableAbilities)
        {
            enableAbilities = false;
            int choice = Int32.Parse(Input.inputString);            
            abilities[choice - 1].GetComponent<Ability>().Execute();

            if (enoughResource)
            {
                abilityText.text = abilities[choice - 1].GetComponent<Ability>().message;
                StartCoroutine(DisplayText());
                StartCoroutine(WaitBeforeChangeTurn());
            }

            enableAbilities = true;
        }
	}

    private IEnumerator DisplayText()
    {
        abilityText.enabled = true;

        yield return new WaitForSeconds(1.5f);

        abilityText.enabled = false;
    }

    IEnumerator WaitBeforeChangeTurn()
    {        
        yield return new WaitForSeconds(1.5f);
        GameObject.FindObjectOfType<ShapesManager>().ChangeTurn();        
    }
}
