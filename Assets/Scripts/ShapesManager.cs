using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;


public class ShapesManager : MonoBehaviour
{
    //public Text DebugText, ScoreText;
    //public bool ShowDebugInfo = false;    

    public ShapesArray shapes;

    //private int score;

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
    public float turnTime = 10f;
    float minutes;
    float seconds;
    public Text bonusText;

    public int p1Health = 100;
    public int p2Health = 100;
    public int damageMultiplier = 5;
    public Scrollbar healthBar1;
    public Scrollbar healthBar2;
    public int damageDealt;
    public Text damageText;

    public bool timedTurns = false;
    public bool AI = false;

    /*void Awake()
    {
        DebugText.enabled = ShowDebugInfo;
    }*/

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
        //InitializeVariables();

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
        /*if (ShowDebugInfo)
            DebugText.text = DebugUtilities.GetArrayContents(shapes);*/

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
        if (timedTurns)
        {
            if (turnTime > 0)
            {
                minutes = turnTime / 60;
                seconds = turnTime % 60;

                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                turnTime -= Time.deltaTime;
            }
            else
            {
                ChangeTurn();
                turnTime = 10f;
            }
        }
    }

    private void ChangeTurn()
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
        int numBalls = 0;

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
            StartCoroutine(DisplayBonusText());
        }

        int timesRun = 1; //can be used for subsequent matching bonus point
        while (totalMatches.Count() >= Constants.MinimumMatches)
        {
            //increase score
            /*IncreaseScore((totalMatches.Count() - 2) * Constants.Match3Score);

            if (timesRun >= 2)
                IncreaseScore(Constants.SubsequentMatchScore);*/

            isBall = false;
            numBalls = 0;

            soundManager.PlayCrincle();

            foreach (var item in totalMatches)
            {
                if (item.GetComponent<Shape>().Type == "Ball")
                {
                    isBall = true;
                    numBalls++;
                }
                shapes.Remove(item);
                RemoveFromScene(item);
            }

            if (isBall)
                DealDamage(numBalls);

            //get the columns that we had a collapse
            var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //the order the 2 methods below get called is important!!!
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
        return CandyPrefabs[Random.Range(0, CandyPrefabs.Length)];
    }

    /*private void InitializeVariables()
    {
        score = 0;
        ShowScore();
    }

    private void IncreaseScore(int amount)
    {
        score += amount;
        ShowScore();
    }

    private void ShowScore()
    {
        ScoreText.text = "Score: " + score.ToString();
    }*/

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

        foreach (var item in potentialMatches)
        {
            animateItems.Add(item);
        }
        animateItems.RemoveAt(animateItems.Count - 1);

        if (potentialMatches != null)
        {
         
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
        else
        {
            Debug.Log("Locked Situation");
            //Restart the board
            InitializeTypesOnPrefabShapes();

            InitializeCandyAndSpawnPositions();

            StartCheckForPotentialMatches();
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

    private IEnumerator DisplayBonusText()
    {
        bonusText.enabled = true;

        yield return new WaitForSeconds(2.0f);

        bonusText.enabled = false;
    }

    private void DealDamage(int damageAmount)
    {
        if (turn == 1)
        {
            damageDealt = damageAmount * damageMultiplier;
            p2Health -= damageDealt;
            healthBar2.size = p2Health / 100f;
            Instantiate(damageText, new Vector3(0f, 0f, 0f), Quaternion.identity);            
        }
        else if (turn == 2)
        {
            damageDealt = damageAmount * damageMultiplier;
            p1Health -= damageDealt;
            healthBar1.size = p1Health / 100f;
            Instantiate(damageText, new Vector3(0f, 0f, 0f), Quaternion.identity); 
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
}
