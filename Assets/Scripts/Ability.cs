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

    public bool buttonEnabled = true;

    private ShapesManager shapeManager;

    void Update()
    {
        shapeManager = FindObjectOfType<ShapesManager>();
        int turn = shapeManager.GetTurn();

        if (turn == 1)
        {
            //Check to see if the player has enough resources for this ability and enable its button accordingly
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > shapeManager.p1Attr[i])
                {
                    buttonEnabled = false;
                    return;
                }
                else
                {
                    buttonEnabled = true;
                }
            }
        }
        else if (turn == 2)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > shapeManager.p2Attr[i])
                {
                    buttonEnabled = false;
                    return;
                }
                else
                {
                    buttonEnabled = true;
                }
            }
        }
    }

    public void Execute()
    {
        shapeManager = FindObjectOfType<ShapesManager>();
        int turn = shapeManager.GetTurn();
        bool enoughResources = true;

        if (turn == 1)
        {
            //Check to see if the player has enough resources for this ability
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > shapeManager.p1Attr[i])
                {
                    enoughResources = false;
                    GameObject.FindObjectOfType<AbilityManager>().enoughResource = false;                    
                    return;
                }
                else
                {                    
                    GameObject.FindObjectOfType<AbilityManager>().enoughResource = true;
                }
            }

            //Lock other player's abilities for 1 turn
            if (lockAbilities && enoughResources)
            {
                FindObjectOfType<AbilityManager>().p2Lock = true;
            }
        }
        else if (turn == 2)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > shapeManager.p2Attr[i])
                {
                    enoughResources = false;
                    GameObject.FindObjectOfType<AbilityManager>().enoughResource = false;                    
                    return;
                }
                else
                {                    
                    GameObject.FindObjectOfType<AbilityManager>().enoughResource = true;
                }
            }

            if (lockAbilities && enoughResources)
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
