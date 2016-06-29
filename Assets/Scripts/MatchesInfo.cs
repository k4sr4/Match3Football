using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MatchesInfo
{
    private List<GameObject> matchedBlocks;

    /// Returns distinct list of matched candy
    public IEnumerable<GameObject> MatchedBlock
    {
        get
        {
            return matchedBlocks.Distinct();
        }
    }

    public void AddObject(GameObject go)
    {
        if (!matchedBlocks.Contains(go))
            matchedBlocks.Add(go);
    }

    public void AddObjectRange(IEnumerable<GameObject> gos)
    {
        foreach (var item in gos)
        {
            AddObject(item);
        }
    }

    public MatchesInfo()
    {
        matchedBlocks = new List<GameObject>();        
    }    
}

