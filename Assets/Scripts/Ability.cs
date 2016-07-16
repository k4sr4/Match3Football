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
    public bool lockAbilities = false;
    public bool convertBlocks = false;
    public GameObject originalBlock, toConvertBlock;

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
        int turn = shapeManager.GetTurn();
        bool enoughResources = true;

        if (turn == 1)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > shapeManager.p1Attr[i])
                    enoughResources = false;
            }

            //Lock other player's abilities for 1 turn
            if (lockAbilities)
            {
                FindObjectOfType<AbilityManager>().p2Lock = true;
            }
        }
        else if (turn == 2)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > shapeManager.p2Attr[i])
                    enoughResources = false;
            }

            if (lockAbilities)
            {
                FindObjectOfType<AbilityManager>().p1Lock = true;
            }
        }        

        if (enoughResources)
        {
            //Reduce attributes going towards resources
            for (int i = 0; i < resources.Length; i++)
            {                
                if (resources[i] != 0)
                    shapeManager.AddAttribute(i, -resources[i], false);
            }   

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
                if (attrAmount > 0)
                {
                    shapeManager.AddAttribute(toClearIndex, attrAmount, false);
                    shapeManager.collectBlockAbilityBlocks = 0;
                }
            }

            //Drain from opponent's attribute
            if (affectOppenentsAttribute)
            {
                shapeManager.AddAttribute(opponentAttr, drainAmount, true);
            }

            //Convert one block to another
            if (convertBlocks)
            {
                StartCoroutine(shapeManager.ConvertBlock(originalBlock, toConvertBlock));
            }
                    
        }
    }    
}
