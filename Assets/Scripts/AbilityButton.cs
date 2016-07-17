using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AbilityButton : MonoBehaviour {

    public int player = 1;
    public GameObject ability;

    private ShapesManager shapesManager;

	// Use this for initialization
	void Start () {
        shapesManager = FindObjectOfType<ShapesManager>();
	}
	
	// Update is called once per frame
	void Update () {
        int turn = shapesManager.GetTurn();
        bool enabled = ability.GetComponent<Ability>().buttonEnabled;
        
        if (turn - player != 0)
        {
            this.GetComponent<Button>().interactable = false;
        }
        else
        {
            if (enabled)
            {                
                this.GetComponent<Button>().interactable = true;
            }
            else
            {                
                this.GetComponent<Button>().interactable = false;
            }
        }
	}
}
