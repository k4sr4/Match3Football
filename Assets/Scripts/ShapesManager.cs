﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ShapesManager : MonoBehaviour
{    
    public ShapesArray shapes;

    public Vector2 BottomRight = new Vector2(-1.37f, -2.27f);
    public readonly Vector2 CandySize = new Vector2(0.7f, 0.7f);

    private GameState state = GameState.None;
    private GameObject hitGo = null;
    private Vector2[] SpawnPositions;
    public GameObject[] CandyPrefabs;
    public GameObject[] ExplosionPrefabs;

    private IEnumerator CheckPotentialMatchesCoroutine;
    private IEnumerator AnimatePotentialMatchesCoroutine;

    IEnumerable<GameObject> potentialMatches;

    public SoundManager soundManager;

    public int turn = 1;
    public Text player1text, player2text;
    
    public Text timerText;
    public float turnTime = 30f;
    float minutes;
    float seconds;
    public Text bonusText;
    public Text lockedSituationText;

    public int p1Health = 100;
    public int p2Health = 100;    
    public Scrollbar healthBar1, healthBar2;
    public int damageDealt;
    public Text damageText;
    public Text hp1, hp2;
    public Text hpGainText;
    public int hpGain = 0;
    public Text goals1Text, goals2Text;
    public int goals1 = 0;
    public int goals2 = 0;

    public int[] p1Attr = {0, 0, 0, 0};
    public int[] p2Attr = {0, 0, 0, 0};
    public Scrollbar[] attrBars;
    public Text[] attrTexts;
    public Text attributeGainText;
    public int attributeGain;
    public bool drainAttribute = false;

    private int p1Coins = 0;
    private int p2Coins = 0;
    public Text coinText;
    public int coinGain = 0;

    public int collectBlockAbilityBlocks = 0;

    public bool timedTurns = false;
    public bool AI = false;

    public bool endWithGoals = true;
    public int goalLimit = 2;

    public bool endWithTime = false;
    public float timeLimit = 600f;
    private float timeRemaining;
    public Text totalTimeText;

    public GameObject endPanel;
    public Text endMsg;

    // Use this for initialization
    void Start()
    {
        InitializeTypesOnPrefabShapes();

        InitializeCandyAndSpawnPositions();

        StartCheckForPotentialMatches();

        turn = 1;
        player1text.color = Color.yellow;
        player1text.fontSize = 26;
    }

    /// Initialize shapes
    private void InitializeTypesOnPrefabShapes()
    {
        //just assign the name of the prefab
        foreach (var item in CandyPrefabs)
        {
            item.GetComponent<Shape>().Type = item.name;

        }
    }


    public void InitializeCandyAndSpawnPositions()
    {
        timeRemaining = timeLimit;
        endPanel.SetActive(false);

        if (shapes != null)
            DestroyAllCandy();

        shapes = new ShapesArray();
        SpawnPositions = new Vector2[Constants.Columns];

        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {

                GameObject newCandy = GetRandomCandy();

                //check if two previous horizontal are of the same type
                while (column >= 2 && shapes[row, column - 1].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row, column - 2].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                //check if two previous vertical are of the same type
                while (row >= 2 && shapes[row - 1, column].GetComponent<Shape>()
                    .IsSameType(newCandy.GetComponent<Shape>())
                    && shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>()))
                {
                    newCandy = GetRandomCandy();
                }

                InstantiateAndPlaceNewCandy(row, column, newCandy);

            }
        }

        SetupSpawnPositions();
    }



    private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
    {
        GameObject go = Instantiate(newCandy,
            BottomRight + new Vector2(column * CandySize.x, row * CandySize.y), Quaternion.identity)
            as GameObject;

        //assign the specific properties
        go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
        shapes[row, column] = go;
    }

    private GameObject InstantiateAndPlaceConverted(int row, int column, GameObject newCandy)
    {
        GameObject go = Instantiate(newCandy,
            BottomRight + new Vector2(column * CandySize.x, row * CandySize.y), Quaternion.identity)
            as GameObject;

        //assign the specific properties
        go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
        shapes[row, column] = go;

        return go;
    }

    private void SetupSpawnPositions()
    {
        //create the spawn positions for the new shapes (will pop from the 'ceiling')
        for (int column = 0; column < Constants.Columns; column++)
        {
            SpawnPositions[column] = BottomRight
                + new Vector2(column * CandySize.x, Constants.Rows * CandySize.y);
        }
    }

    /// Destroy all candy gameobjects
    private void DestroyAllCandy()
    {
        for (int row = 0; row < Constants.Rows; row++)
        {
            for (int column = 0; column < Constants.Columns; column++)
            {
                Destroy(shapes[row, column]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == GameState.None)
        {
            //user has clicked or touched
            if (Input.GetMouseButtonDown(0))
            {
                //get the hit position
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null) //we have a hit!!!
                {
                    hitGo = hit.collider.gameObject;
                    state = GameState.SelectionStarted;
                }
                
            }
        }
        else if (state == GameState.SelectionStarted)
        {
            //user dragged
            if (Input.GetMouseButton(0))
            {
                

                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                //we have a hit
                if (hit.collider != null && hitGo != hit.collider.gameObject)
                {

                    //user did a hit, no need to show him hints 
                    StopCheckForPotentialMatches();

                    //if the two shapes are diagonally aligned (different row and column), just return
                    if (!Utilities.AreVerticalOrHorizontalNeighbors(hitGo.GetComponent<Shape>(),
                        hit.collider.gameObject.GetComponent<Shape>()))
                    {
                        state = GameState.None;
                    }
                    else
                    {
                        state = GameState.Animating;
                        FixSortingLayer(hitGo, hit.collider.gameObject);
                        StartCoroutine(FindMatchesAndCollapse(hit.collider.gameObject));                        
                    }
                }
            }
        }

        //handles the change turns in timed turn mode
        if (turnTime > 0)
        {
            timerText.text = turnTime.ToString("0");

            turnTime -= Time.deltaTime;
        }
        else
        {
            ChangeTurn();            
        }
        

        //handles the total time when time is the end condition
        if (endWithTime)
        {
            if (timeRemaining > 0)
            {
                minutes = (timeRemaining / 60);
                seconds = timeRemaining % 60;

                totalTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                timeRemaining -= Time.deltaTime;
            }
            else
            {
                if (goals1 > goals2)
                {
                    endPanel.SetActive(true);
                    endMsg.text = "Player 1 Wins!";
                }
                else if (goals1 < goals2)
                {
                    endPanel.SetActive(true);
                    endMsg.text = "Player 2 Wins!";
                }
                else
                {
                    endPanel.SetActive(true);
                    endMsg.text = "It's a Draw!";
                }
            }
        }
    }

    public void ChangeTurn()
    {
        if (turn == 1)
        {
            turn = 2;

            player2text.color = Color.yellow;
            player2text.fontSize = 26;

            player1text.color = Color.black;
            player1text.fontSize = 20;
        }
        else
        {
            turn = 1;

            player1text.color = Color.yellow;
            player1text.fontSize = 26;

            player2text.color = Color.black;
            player2text.fontSize = 20;
        }

        turnTime = 30f;
    }

    /// Modifies sorting layers for better appearance when dragging/animating
    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        SpriteRenderer sp1 = hitGo.GetComponent<SpriteRenderer>();
        SpriteRenderer sp2 = hitGo2.GetComponent<SpriteRenderer>();
        if (sp1.sortingOrder <= sp2.sortingOrder)
        {
            sp1.sortingOrder = 1;
            sp2.sortingOrder = 0;
        }
    }

    private IEnumerator FindMatchesAndCollapse(GameObject hitGo2)
    {
        bool addBonus = false;

        bool isBall = false;
        bool isRef = false;
        bool isBlue = false;
        bool isGreen = false;
        bool isRed = false;
        bool isCoin = false;
        bool isGoalie = false;

        int numBalls = 0;
        int numRefs = 0;
        int numBlues = 0;
        int numGreens = 0;
        int numReds = 0;
        int numCoins = 0;
        int numGoalies = 0;

        shapes.Swap(hitGo, hitGo2);

        //move the swapped ones
        hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
        hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
        yield return new WaitForSeconds(Constants.AnimationDuration);

        //get the matches via the helper methods
        var hitGomatchesInfo = shapes.GetMatches(hitGo);
        var hitGo2matchesInfo = shapes.GetMatches(hitGo2);

        var totalMatches = hitGomatchesInfo.MatchedBlock
            .Union(hitGo2matchesInfo.MatchedBlock).Distinct();

        //if user's swap didn't create at least a 3-match, undo their swap
        if (totalMatches.Count() < Constants.MinimumMatches)
        {
            hitGo.transform.positionTo(Constants.AnimationDuration, hitGo2.transform.position);
            hitGo2.transform.positionTo(Constants.AnimationDuration, hitGo.transform.position);
            yield return new WaitForSeconds(Constants.AnimationDuration);

            shapes.UndoSwap();
        }

        //if more than 3 matches and no Bonus is contained in the line, we will award a new Bonus
        if ((hitGomatchesInfo.MatchedBlock.Count() >= Constants.MinimumMatchesForBonus || hitGo2matchesInfo.MatchedBlock.Count() >= Constants.MinimumMatchesForBonus) && !timedTurns)
        {
            addBonus = true;
            turnTime = 30f;
            StartCoroutine(DisplayText(bonusText));
        }

        int timesRun = 1; //can be used for subsequent matching bonus point
        while (totalMatches.Count() >= Constants.MinimumMatches)
        {
            isBall = false;
            isRef = false;
            isBlue = false;
            isGreen = false;
            isRed = false;
            isCoin = false;
            isGoalie = false;
            
            numBalls = 0;
            numRefs = 0;
            numBlues = 0;
            numGreens = 0;
            numReds = 0;
            numCoins = 0;
            numGoalies = 0;

            soundManager.PlayCrincle();

            foreach (var item in totalMatches)
            {
                if (item.GetComponent<Shape>().Type == "Ball")
                {
                    isBall = true;
                    numBalls++;
                }
                else if (item.GetComponent<Shape>().Type == "Ref")
                {
                    isRef = true;
                    numRefs++;
                }
                else if (item.GetComponent<Shape>().Type == "player_blue")
                {
                    isBlue = true;
                    numBlues++;
                }
                else if (item.GetComponent<Shape>().Type == "player_green")
                {
                    isGreen = true;
                    numGreens++;
                }
                else if (item.GetComponent<Shape>().Type == "player_red")
                {
                    isRed = true;
                    numReds++;
                }
                else if (item.GetComponent<Shape>().Type == "Coin")
                {
                    isCoin = true;
                    numCoins++;
                }
                else if (item.GetComponent<Shape>().Type == "Goalie")
                {
                    isGoalie = true;
                    numGoalies++;
                }

                shapes.Remove(item);
                RemoveFromScene(item);
            }

            if (isBall)
            {
                DealDamage(numBalls);
            }
            if (isRef)
            {
                AddAttribute(0, numRefs, false);
            }
            if (isBlue)
            {
                AddAttribute(1, numBlues, false);
            }
            if (isGreen)
            {
                AddAttribute(2, numGreens, false);
            }
            if (isRed)
            {
                AddAttribute(3, numReds, false);
            }
            if (isCoin)
            {
                AddCoins (numCoins);
            }
            if (isGoalie)
            {
                RegainHp(numGoalies);
            }

            //get the columns that we had a collapse
            var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //collapse the ones gone
            var collapsedCandyInfo = shapes.Collapse(columns);
            //create new ones
            var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

            int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

            //search if there are matches with the new/collapsed items
            totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).
                Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();

            //if we have not hit bonus and we don't have timed turns, change turn
            if (!timedTurns && !addBonus && totalMatches.Count() < Constants.MinimumMatches)
                ChangeTurn();

            timesRun++;
        }

        state = GameState.None;
        StartCheckForPotentialMatches();
    }

    public IEnumerator ClearBlock(GameObject block)
    {
        bool isBall = false;
        bool isRef = false;
        bool isBlue = false;
        bool isGreen = false;
        bool isRed = false;
        bool isCoin = false;
        bool isGoalie = false;

        int numBalls = 0;
        int numRefs = 0;
        int numBlues = 0;
        int numGreens = 0;
        int numReds = 0;
        int numCoins = 0;
        int numGoalies = 0;

        IEnumerable<GameObject> totalMatches;
        var sameBlocks = shapes.GetBlockInEntireBoard(block);

        foreach (var item in sameBlocks)
        {
            collectBlockAbilityBlocks++;
            shapes.Remove(item);
            RemoveFromScene(item);
        }

        //get the columns that we had a collapse
        var columns = sameBlocks.Select(go => go.GetComponent<Shape>().Column).Distinct();

        //collapse the ones gone
        var collapsedCandyInfo = shapes.Collapse(columns);
        //create new ones
        var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

        int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

        MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
        MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

        //will wait for both of the above animations
        yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

        //search if there are matches with the new/collapsed items
        totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).
            Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();

        while (totalMatches.Count() >= Constants.MinimumMatches)
        {
            isBall = false;
            isRef = false;
            isBlue = false;
            isGreen = false;
            isRed = false;
            isCoin = false;
            isGoalie = false;

            numBalls = 0;
            numRefs = 0;
            numBlues = 0;
            numGreens = 0;
            numReds = 0;
            numCoins = 0;
            numGoalies = 0;

            soundManager.PlayCrincle();

            foreach (var item in totalMatches)
            {
                if (item.GetComponent<Shape>().Type == "Ball")
                {
                    isBall = true;
                    numBalls++;
                }
                else if (item.GetComponent<Shape>().Type == "Ref")
                {
                    isRef = true;
                    numRefs++;
                }
                else if (item.GetComponent<Shape>().Type == "player_blue")
                {
                    isBlue = true;
                    numBlues++;
                }
                else if (item.GetComponent<Shape>().Type == "player_green")
                {
                    isGreen = true;
                    numGreens++;
                }
                else if (item.GetComponent<Shape>().Type == "player_red")
                {
                    isRed = true;
                    numReds++;
                }
                else if (item.GetComponent<Shape>().Type == "Coin")
                {
                    isCoin = true;
                    numCoins++;
                }
                else if (item.GetComponent<Shape>().Type == "Goalie")
                {
                    isGoalie = true;
                    numGoalies++;
                }

                shapes.Remove(item);
                RemoveFromScene(item);
            }

            if (isBall)
            {
                DealDamage(numBalls);
            }
            if (isRef)
            {
                AddAttribute(0, numRefs, false);
            }
            if (isBlue)
            {
                AddAttribute(1, numBlues, false);
            }
            if (isGreen)
            {
                AddAttribute(2, numGreens, false);
            }
            if (isRed)
            {
                AddAttribute(3, numReds, false);
            }
            if (isCoin)
            {
                AddCoins(numCoins);
            }
            if (isGoalie)
            {
                RegainHp(numGoalies);
            }

            //get the columns that we had a collapse
            columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //collapse the ones gone
            collapsedCandyInfo = shapes.Collapse(columns);
            //create new ones
            newCandyInfo = CreateNewCandyInSpecificColumns(columns);

            maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

            MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
            MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

            //will wait for both of the above animations
            yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

            //search if there are matches with the new/collapsed items
            totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).
                Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();
        }

        StartCheckForPotentialMatches();
    }

    public IEnumerator ConvertBlock(GameObject block, GameObject convertTo)
    {
        bool isBall = false;
        bool isRef = false;
        bool isBlue = false;
        bool isGreen = false;
        bool isRed = false;
        bool isCoin = false;
        bool isGoalie = false;

        int numBalls = 0;
        int numRefs = 0;
        int numBlues = 0;
        int numGreens = 0;
        int numReds = 0;
        int numCoins = 0;
        int numGoalies = 0;

        var sameBlocks = shapes.GetBlockInEntireBoard(block);
        List<GameObject> convertedBlocks = new List<GameObject>();
        IEnumerable<GameObject> totalMatches;

        foreach (var item in sameBlocks)
        {
            int row = item.GetComponent<Shape>().Row;
            int column = item.GetComponent<Shape>().Column;
            shapes.Remove(item);
            RemoveFromScene(item);
            convertedBlocks.Add(InstantiateAndPlaceConverted(row, column, convertTo));
        }

        yield return new WaitForSeconds(0.2f);

        foreach (GameObject c in convertedBlocks)
        {
            if (c != null)
            {
                //get the matches via the helper methods
                MatchesInfo convertMatchesInfo = shapes.GetMatches(c);

                totalMatches = convertMatchesInfo.MatchedBlock;


                while (totalMatches.Count() >= Constants.MinimumMatches)
                {
                    isBall = false;
                    isRef = false;
                    isBlue = false;
                    isGreen = false;
                    isRed = false;
                    isCoin = false;
                    isGoalie = false;

                    numBalls = 0;
                    numRefs = 0;
                    numBlues = 0;
                    numGreens = 0;
                    numReds = 0;
                    numCoins = 0;
                    numGoalies = 0;

                    soundManager.PlayCrincle();

                    foreach (var item in totalMatches)
                    {
                        if (item.GetComponent<Shape>().Type == "Ball")
                        {
                            isBall = true;
                            numBalls++;
                        }
                        else if (item.GetComponent<Shape>().Type == "Ref")
                        {
                            isRef = true;
                            numRefs++;
                        }
                        else if (item.GetComponent<Shape>().Type == "player_blue")
                        {
                            isBlue = true;
                            numBlues++;
                        }
                        else if (item.GetComponent<Shape>().Type == "player_green")
                        {
                            isGreen = true;
                            numGreens++;
                        }
                        else if (item.GetComponent<Shape>().Type == "player_red")
                        {
                            isRed = true;
                            numReds++;
                        }
                        else if (item.GetComponent<Shape>().Type == "Coin")
                        {
                            isCoin = true;
                            numCoins++;
                        }
                        else if (item.GetComponent<Shape>().Type == "Goalie")
                        {
                            isGoalie = true;
                            numGoalies++;
                        }

                        shapes.Remove(item);
                        RemoveFromScene(item);
                    }

                    if (isBall)
                    {
                        DealDamage(numBalls);
                    }
                    if (isRef)
                    {
                        AddAttribute(0, numRefs, false);
                    }
                    if (isBlue)
                    {
                        AddAttribute(1, numBlues, false);
                    }
                    if (isGreen)
                    {
                        AddAttribute(2, numGreens, false);
                    }
                    if (isRed)
                    {
                        AddAttribute(3, numReds, false);
                    }
                    if (isCoin)
                    {
                        AddCoins(numCoins);
                    }
                    if (isGoalie)
                    {
                        RegainHp(numGoalies);
                    }

                    //get the columns that we had a collapse
                    var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

                    //collapse the ones gone
                    var collapsedCandyInfo = shapes.Collapse(columns);
                    //create new ones
                    var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

                    int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

                    MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
                    MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

                    //will wait for both of the above animations
                    yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

                    //search if there are matches with the new/collapsed items
                    totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).
                        Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();
                }
            }
        }

        yield return new WaitForSeconds(1f);

        foreach (GameObject c in GameObject.FindGameObjectsWithTag(convertTo.GetComponent<Shape>().Type))
        {
            //get the matches via the helper methods
            MatchesInfo convertMatchesInfo = shapes.GetMatches(c);

            totalMatches = convertMatchesInfo.MatchedBlock;


            while (totalMatches.Count() >= Constants.MinimumMatches)
            {
                isBall = false;
                isRef = false;
                isBlue = false;
                isGreen = false;
                isRed = false;
                isCoin = false;
                isGoalie = false;

                numBalls = 0;
                numRefs = 0;
                numBlues = 0;
                numGreens = 0;
                numReds = 0;
                numCoins = 0;
                numGoalies = 0;

                soundManager.PlayCrincle();

                foreach (var item in totalMatches)
                {
                    if (item.GetComponent<Shape>().Type == "Ball")
                    {
                        isBall = true;
                        numBalls++;
                    }
                    else if (item.GetComponent<Shape>().Type == "Ref")
                    {
                        isRef = true;
                        numRefs++;
                    }
                    else if (item.GetComponent<Shape>().Type == "player_blue")
                    {
                        isBlue = true;
                        numBlues++;
                    }
                    else if (item.GetComponent<Shape>().Type == "player_green")
                    {
                        isGreen = true;
                        numGreens++;
                    }
                    else if (item.GetComponent<Shape>().Type == "player_red")
                    {
                        isRed = true;
                        numReds++;
                    }
                    else if (item.GetComponent<Shape>().Type == "Coin")
                    {
                        isCoin = true;
                        numCoins++;
                    }
                    else if (item.GetComponent<Shape>().Type == "Goalie")
                    {
                        isGoalie = true;
                        numGoalies++;
                    }

                    shapes.Remove(item);
                    RemoveFromScene(item);
                }

                if (isBall)
                {
                    DealDamage(numBalls);
                }
                if (isRef)
                {
                    AddAttribute(0, numRefs, false);
                }
                if (isBlue)
                {
                    AddAttribute(1, numBlues, false);
                }
                if (isGreen)
                {
                    AddAttribute(2, numGreens, false);
                }
                if (isRed)
                {
                    AddAttribute(3, numReds, false);
                }
                if (isCoin)
                {
                    AddCoins(numCoins);
                }
                if (isGoalie)
                {
                    RegainHp(numGoalies);
                }

                //get the columns that we had a collapse
                var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

                //collapse the ones gone
                var collapsedCandyInfo = shapes.Collapse(columns);
                //create new ones
                var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

                int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

                MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
                MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

                //will wait for both of the above animations
                yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

                //search if there are matches with the new/collapsed items
                totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).
                    Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();
            }
        }

        StartCheckForPotentialMatches();
    }
    /// Spawns new candy in columns that have missing ones
    private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
    {
        AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

        //find how many null values the column has
        foreach (int column in columnsWithMissingCandy)
        {
            var emptyItems = shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                var go = GetRandomCandy();
                GameObject newCandy = Instantiate(go, SpawnPositions[column], Quaternion.identity)
                    as GameObject;

                newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

                if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
                    newCandyInfo.MaxDistance = Constants.Rows - item.Row;

                shapes[item.Row, item.Column] = newCandy;
                newCandyInfo.AddCandy(newCandy);
            }
        }
        return newCandyInfo;
    }

    /// Animates gameobjects to their new position
    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
    {
        foreach (var item in movedGameObjects)
        {
            item.transform.positionTo(Constants.MoveAnimationMinDuration * distance, BottomRight +
                new Vector2(item.GetComponent<Shape>().Column * CandySize.x, item.GetComponent<Shape>().Row * CandySize.y));
        }
    }

    /// Destroys the item from the scene and instantiates a new explosion gameobject
    private void RemoveFromScene(GameObject item)
    {
        GameObject explosion = GetRandomExplosion();
        var newExplosion = Instantiate(explosion, item.transform.position, Quaternion.identity) as GameObject;
        Destroy(newExplosion, Constants.ExplosionDuration);
        Destroy(item);
    }

    /// Get a random candy
    private GameObject GetRandomCandy()
    {
        int randomChoice = Random.Range(0, CandyPrefabs.Length);

        float generateChance = CandyPrefabs[randomChoice].GetComponent<Shape>().chance;        

        //Handle rarity
        if (Random.value <= generateChance)
        {
            return CandyPrefabs[randomChoice];
        }
        else
        {
            return GetRandomCandy();
        }                     
    }

    /// Get a random explosion
    private GameObject GetRandomExplosion()
    {
        return ExplosionPrefabs[Random.Range(0, ExplosionPrefabs.Length)];
    }

    /// Starts the coroutines, keeping a reference to stop later
    private void StartCheckForPotentialMatches()
    {
        StopCheckForPotentialMatches();
        //get a reference to stop it later
        CheckPotentialMatchesCoroutine = CheckPotentialMatches();
        StartCoroutine(CheckPotentialMatchesCoroutine);
    }

    /// Stops the coroutines
    private void StopCheckForPotentialMatches()
    {
        if (AnimatePotentialMatchesCoroutine != null)
            StopCoroutine(AnimatePotentialMatchesCoroutine);
        if (CheckPotentialMatchesCoroutine != null)
            StopCoroutine(CheckPotentialMatchesCoroutine);
        ResetOpacityOnPotentialMatches();
    }

    /// Resets the opacity on potential matches (probably user dragged something?)
    private void ResetOpacityOnPotentialMatches()
    {
        if (potentialMatches != null)
            foreach (var item in potentialMatches)
            {
                if (item == null) break;

                Color c = item.GetComponent<SpriteRenderer>().color;
                c.a = 1.0f;
                item.GetComponent<SpriteRenderer>().color = c;
            }
    }

    /// Finds potential matches
    private IEnumerator CheckPotentialMatches()
    {
        if (AI)
        {
            yield return new WaitForSeconds(Constants.AIWaitTime);
        }
        else
        {
            yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
        }
        potentialMatches = Utilities.GetPotentialMatches(shapes);
        List<GameObject> animateItems = new List<GameObject>();

        if (potentialMatches == null)
        {
            Debug.Log("Locked Situation");

            StartCoroutine(DisplayText(lockedSituationText));
            //Restart the board
            InitializeTypesOnPrefabShapes();
            
            InitializeCandyAndSpawnPositions();

            StartCheckForPotentialMatches();
        }
        else
        {
            foreach (var item in potentialMatches)
            {
                animateItems.Add(item);
            }
            animateItems.RemoveAt(animateItems.Count - 1);

            if (AI && turn == 2)   {
                AIMatchPotential(potentialMatches);
            }
            else
            {
                while (true)
                {
                    if (AI && turn == 2)
                    {
                        StartCheckForPotentialMatches();
                        break;
                    }
                    AnimatePotentialMatchesCoroutine = Utilities.AnimatePotentialMatches(animateItems);
                    StartCoroutine(AnimatePotentialMatchesCoroutine);
                    yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
                }
            }
        }
    }

    private void AIMatchPotential(IEnumerable<GameObject> potentialMatches)
    {
        List<GameObject> blocks = new List<GameObject>();

        foreach (GameObject item in potentialMatches)
        {
            blocks.Add(item);
        }

        hitGo = blocks.ElementAt(blocks.Count - 2);
        GameObject hitGo2 = blocks.ElementAt(blocks.Count - 1);

        StartCoroutine(FindMatchesAndCollapse(hitGo2));
    }

    private IEnumerator DisplayText(Text toDisplay)
    {
        toDisplay.enabled = true;

        yield return new WaitForSeconds(2.0f);

        toDisplay.enabled = false;
    }

    public void DealDamage(int damageAmount)
    {
        if (turn == 1)
        {
            damageDealt = damageAmount * Constants.damageMultiplier;
            p2Health -= damageDealt;
            healthBar2.size = p2Health / 100f;
            hp2.text = p2Health.ToString();
            Instantiate(damageText, new Vector3(0f, 0f, 0f), Quaternion.identity); //damage text

            //When player 1 scores a goal
            if (p2Health <= 0)
            {
                ScoreGoal();
            }
        }
        else if (turn == 2)
        {
            damageDealt = damageAmount * Constants.damageMultiplier;
            p1Health -= damageDealt;
            healthBar1.size = p1Health / 100f;
            hp1.text = p1Health.ToString();
            Instantiate(damageText, new Vector3(0f, 0f, 0f), Quaternion.identity); //damage text

            //When player 1 scores a goal
            if (p1Health <= 0)
            {
                ScoreGoal();
            }
        }
    }

    public void ScoreGoal()
    {
        if (turn == 1)
        {
            goals1 += 1;
            goals1Text.text = goals1.ToString();
            p2Health = 100;
            healthBar2.size = p2Health / 100f;
            hp2.text = p2Health.ToString();

            if (goals1 == goalLimit && endWithGoals)
            {
                endPanel.SetActive(true);
                endMsg.text = "Player 1 Wins!";
            }
        }
        else if (turn == 2)
        {
            goals2 += 1;
            goals2Text.text = goals2.ToString();
            p1Health = 100;
            healthBar1.size = p1Health / 100f;
            hp1.text = p1Health.ToString();

            if (goals2 == goalLimit && endWithGoals)
            {
                endPanel.SetActive(true);
                endMsg.text = "Player 2 Wins!";
            }
        }
    }

    public void AddAttribute(int type, int amount, bool drain)
    {
        attributeGain = amount;

        if (drain)
        {
            drainAttribute = true;
            if (turn == 1)
            {
                //+4s are there to make sure we change the bars and texts for player 2
                p2Attr[type] -= amount;
                attrBars[type + 4].size = p2Attr[type] / 20f;
                attrTexts[type + 4].text = p2Attr[type].ToString();
                Instantiate(attributeGainText, new Vector3(attrBars[type + 4].transform.localPosition.x + 652f, -165f, 0f), Quaternion.identity);

                if (p2Attr[type] <= 0)
                {
                    p2Attr[type] = 0;
                    attrBars[type + 4].size = 0;
                    attrTexts[type + 4].text = "0";
                }

                drainAttribute = false;
            }
            else if (turn == 2)
            {                
                p1Attr[type] -= amount;
                attrBars[type].size = p1Attr[type] / 20f;
                attrTexts[type].text = p1Attr[type].ToString();
                Instantiate(attributeGainText, new Vector3(attrBars[type].transform.localPosition.x - 645f, -165f, 0f), Quaternion.identity);

                if (p1Attr[type] <= 0)
                {
                    p1Attr[type] = 0;
                    attrBars[type].size = 0;
                    attrTexts[type].text = "0";
                }

                drainAttribute = false;
            }
        }
        else
        {
            if (turn == 1)
            {
                //If the player uses an ability and we want to drain attribute
                if (amount < 0)
                {
                    attributeGain *= -1;
                    drainAttribute = true;
                }
                else
                {
                    drainAttribute = false;
                }

                p1Attr[type] += amount;
                attrBars[type].size = p1Attr[type] / 20f;
                attrTexts[type].text = p1Attr[type].ToString();
                Instantiate(attributeGainText, new Vector3(attrBars[type].transform.localPosition.x - 45f, -165f, 0f), Quaternion.identity);

                if (p1Attr[type] >= 20)
                {
                    p1Attr[type] = 20;
                    attrBars[type].size = 20;
                    attrTexts[type].text = "20";
                }                
            }
            else if (turn == 2)
            {
                if (amount < 0)
                {
                    attributeGain *= -1;
                    drainAttribute = true;
                }
                else
                {
                    drainAttribute = false;
                }

                //+4s are there to make sure we change the bars and texts for player 2
                p2Attr[type] += amount;
                attrBars[type + 4].size = p2Attr[type] / 20f;
                attrTexts[type + 4].text = p2Attr[type].ToString();
                Instantiate(attributeGainText, new Vector3(attrBars[type + 4].transform.localPosition.x + 52f, -165f, 0f), Quaternion.identity);

                if (p2Attr[type] >= 20)
                {
                    p2Attr[type] = 20;
                    attrBars[type + 4].size = 20;
                    attrTexts[type + 4].text = "20";
                }                
            }
        }
    }

    /*public void DisplayAttrDrain(int index)
    {
        if (turn == 1)
        {
            attrBars[index].size = p1Attr[index] / 20f;
            attrTexts[index].text = p1Attr[index].ToString();
            //Instantiate(attributeGainText, new Vector3(attrBars[index].transform.localPosition.x - 45f, -165f, 0f), Quaternion.identity);

            if (p1Attr[index] <= 0)
            {
                p1Attr[index] = 0;
                attrBars[index].size = 0;
                attrTexts[index].text = "0";
            }
        }
        else if (turn == 2)
        {
            attrBars[index + 4].size = p2Attr[index] / 20f;
            attrTexts[index + 4].text = p2Attr[index].ToString();
            //Instantiate(attributeGainText, new Vector3(attrBars[index + 6].transform.localPosition.x + 52f, -165f, 0f), Quaternion.identity);

            if (p2Attr[index] <= 0)
            {
                p2Attr[index] = 0;
                attrBars[index + 4].size = 0;
                attrTexts[index + 4].text = "0";
            }
        }
    }

    public void DisplayAttrGain(int index)
    {
        if (turn == 1)
        {
            attrBars[index].size = p1Attr[index] / 20f;
            attrTexts[index].text = p1Attr[index].ToString();
            Instantiate(attributeGainText, new Vector3(attrBars[index].transform.localPosition.x - 45f, -165f, 0f), Quaternion.identity);

            if (p1Attr[index] >= 20)
            {
                p1Attr[index] = 20;
                attrBars[index].size = 20;
                attrTexts[index].text = "20";
            }
        }
        else if (turn == 2)
        {
            attrBars[index + 4].size = p2Attr[index] / 20f;
            attrTexts[index + 4].text = p2Attr[index].ToString();
            Instantiate(attributeGainText, new Vector3(attrBars[index + 4].transform.localPosition.x + 52f, -165f, 0f), Quaternion.identity);

            if (p2Attr[index] >= 20)
            {
                p2Attr[index] = 20;
                attrBars[index + 4].size = 20;
                attrTexts[index + 4].text = "20";
            }
        }
    }*/

    public void AddCoins(int amount)
    {
        coinGain = amount;
        Instantiate(coinText, new Vector3(0f, 0f, 0f), Quaternion.identity); //Coin text

        if (turn == 1)
        {
            p1Coins += amount;
        }
        else if (turn == 2)
        {
            p2Coins += amount;
        }
    }

    public void RegainHp(int amount)
    {
        hpGain = amount;
        Instantiate(hpGainText, new Vector3(0f, 0f, 0f), Quaternion.identity); //hp regain text

        if (turn == 1)
        {            
            p1Health += amount;

            if (p1Health > 100)
            {
                p1Health = 100;
            }

            healthBar1.size = p1Health / 100f;
            hp1.text = p1Health.ToString();
        }
        else if (turn == 2)
        {
            p2Health += amount;

            if (p2Health > 100)
            {
                p2Health = 100;
            }

            healthBar2.size = p2Health / 100f;
            hp2.text = p2Health.ToString();
        }
    }

    public int GetTurn()
    {
        return turn;
    }

    public int GetDamageDealt()
    {
        return -damageDealt;
    }

    public int GetAttributeGain()
    {
        return attributeGain;
    }

    public int GetCoinGain()
    {
        return coinGain;
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
