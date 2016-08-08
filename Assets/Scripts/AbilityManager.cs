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
    private ShapesManager shapesManager;
    private int turn = 1;
    private bool allowLockSwitch = false;

	// Use this for initialization
	void Start () {
        shapesManager = FindObjectOfType<ShapesManager>();        
	}

    void Update()
    {
        turn = shapesManager.GetTurn();

        if (turn == 1 && p2Lock && allowLockSwitch)
        {
            p2Lock = false;
            allowLockSwitch = false;
        }
        if (turn == 2 && p1Lock && allowLockSwitch)
        {
            p1Lock = false;
            allowLockSwitch = false;
        }
    }
	
	public void ChooseAbility (int index) {        
        if (turn == 1 && p1Lock)
        {
            return;
        }
        if (turn == 2 && p2Lock)
        {
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
        {
            GameObject.FindObjectOfType<ShapesManager>().ChangeTurn();
            if (p1Lock || p2Lock)
                allowLockSwitch = true;            
        }
    }
}
