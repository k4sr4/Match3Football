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
	
	public void ChooseAbility (int index) {        
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

        if (enableAbilities)
        {
            enableAbilities = false;         
            abilities[index].GetComponent<Ability>().Execute();

            if (enoughResource)
            {
                abilityText.text = abilities[index].GetComponent<Ability>().message;
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
        if (!shapesManager.timedTurns)
            GameObject.FindObjectOfType<ShapesManager>().ChangeTurn();        
    }
}
