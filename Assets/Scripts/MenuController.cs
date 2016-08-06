using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MenuController : MonoBehaviour {

    private ShapesManager shapesManager;
    public GameObject goalsInput, timeInput;

	// Use this for initialization
	void Start () {
        shapesManager = GameObject.FindObjectOfType<ShapesManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TurnModes()
    {
        if (shapesManager.timedTurns)
        {
            shapesManager.timedTurns = false;
        }
        else
        {
            shapesManager.timedTurns = true;
        }
    }

    public void EndMode()
    {
        if (shapesManager.endWithGoals)
        {
            shapesManager.endWithGoals = false;
            shapesManager.endWithTime = true;
            goalsInput.SetActive(false);
            timeInput.SetActive(true);
        }
        else
        {
            shapesManager.endWithGoals = true;
            shapesManager.endWithTime = false;
            goalsInput.SetActive(true);
            timeInput.SetActive(false);
        }
    }

    public void GoalsChanged()
    {
        shapesManager.goalLimit = Int32.Parse(goalsInput.GetComponent<InputField>().text);
    }

    public void TimeChanged()
    {
        shapesManager.timeLimit = Int32.Parse(timeInput.GetComponent<InputField>().text) * 60;
    }

    public void AIMode()
    {
        if (shapesManager.AI)
        {
            shapesManager.AI = false;
        }
        else
        {
            shapesManager.AI = true;
        }
    }

    public void Play()
    {
        this.gameObject.SetActive(false);
    }
}
