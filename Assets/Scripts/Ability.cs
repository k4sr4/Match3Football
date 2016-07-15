using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour {

    public string message = "";
    public bool consumesTurn = true; //differentiate between passive and active abilities
    public GameObject toClear;
    public int toClearIndex = 0;
    public bool addAttribute = false;
    public int toClearDenominator = 1;
    public bool affectOppenentsAttribute = false;
    public int opponentAttr = 0;
    public int drainAmount = 0;
    public float chance = 1;
    public int[] resources = { 0, 0, 0, 0 };
    public int damage = 0;
    public bool goal = false;

    private ShapesManager shapeManager;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Execute()
    {
        shapeManager = FindObjectOfType<ShapesManager>();        

        //Deal damage and score goal if any (apply chance)
        if (Random.value <= chance)
        {
            if (damage != 0)
                shapeManager.DealDamage(damage / Constants.damageMultiplier);

            if (goal)
                shapeManager.ScoreGoal();
        }

        //Add to attribute
        if (addAttribute)
        {
            StartCoroutine(shapeManager.ClearBlock(toClear));
            int attrAmount = shapeManager.collectBlockAbilityBlocks;
            shapeManager.AddAttribute(toClearIndex, attrAmount, false);
            shapeManager.collectBlockAbilityBlocks = 0;
        }

        //Drain from opponent's attribute
        if (affectOppenentsAttribute)
        {
            shapeManager.AddAttribute(opponentAttr, drainAmount, true);
        }

        int turn = shapeManager.GetTurn();
        if (turn == 1)
        {    

        }
        else if (turn == 2)
        {

        }

        //Change turn
        if (consumesTurn)
        {
            shapeManager.ChangeTurn();
        }
    }
}
