using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

public enum eTile { blank, blankNoWall, wall1, wall2, piston, item, utility}
public class WFCGenerator : MonoBehaviour
{
    public int dimensions;
    public WFCTile[] tileObjects;
    public List<WFCCell> gridComponents;
    public List<WFCTile> spawnedTiles;
    public WFCCell cellObj;

    public int wallLimit1;
    public int wallLimit2;
    public int pistonLimit;
    public int pickupLimit;
    public int wallCount1;
    public int wallCount2;
    public int pistonCount;
    public int pickupCount;

    int iterations = 0;

    private void Awake()
    {
        //gridComponents = new List<WFCCell>();
        //InitializeGrid();
        StartCoroutine("CheckEntropy");
    }

    void InitializeGrid()
    {
        for (int y = 0; y < dimensions; y++)
        {
            for(int x = 0; x < dimensions; x++)
            {
                WFCCell newCell = Instantiate(cellObj, new Vector3(x, 0, y), quaternion.identity);
                newCell.CreateCell(false, tileObjects);
                gridComponents.Add(newCell);
            }
        }
        StartCoroutine("CheckEntropy");
    }

    IEnumerator CheckEntropy()
    {
        List<WFCCell> tempGrid = new List<WFCCell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);
        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;
        for (int i = 1; i<tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }
        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return new WaitForSeconds(0.01f);
        CollapseCell(tempGrid);
    }

    void CollapseCell(List<WFCCell> tempGrid)
    {
        WFCCell cellToCollapse = FindCell(tempGrid);

        WFCTile foundTile = FindTile(cellToCollapse);
        
        WFCTile tile = Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);
        spawnedTiles.Add(tile);
        UpdateGeneration();
    }

    WFCCell FindCell(List<WFCCell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        return tempGrid[randIndex];
    }

    WFCTile FindTile(WFCCell cellToCollapse)
    {
        cellToCollapse.collapsed = true;
        WFCTile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        selectedTile = CheckTileLimit(selectedTile, cellToCollapse);
        cellToCollapse.tileOptions = new WFCTile[] { selectedTile };
        WFCTile foundTile = cellToCollapse.tileOptions[0];
        return foundTile;
    }

    WFCTile CheckTileLimit(WFCTile foundTile, WFCCell cellToCollapse)
    {
        WFCTile newTile = foundTile;
        switch (newTile.tileType)
        {
            case eTile.wall1:
                if (wallCount1 == wallLimit1)
                {
                        //cellToCollapse.collapsed = false;
                    Debug.Log("Retrying Generation of Tile of Wall1");
                    newTile = FindTile(cellToCollapse);
                    CheckTileLimit(newTile, cellToCollapse);

                }
                else
                {
                    wallCount1++;
                }
                break;
            case eTile.wall2:
                if (wallCount2 == wallLimit2)
                {
                        //cellToCollapse.collapsed = false;
                    Debug.Log("Retrying Generation of Tile of Wall2");
                    newTile = FindTile(cellToCollapse);
                    CheckTileLimit(newTile, cellToCollapse);
                }
                else
                {
                    wallCount2++;
                }
                break;
            case eTile.piston:
                if (pistonCount == pistonLimit)
                {
                        //cellToCollapse.collapsed = false;
                    Debug.Log("Retrying Generation of Tile of piston");
                    newTile = FindTile(cellToCollapse);
                    CheckTileLimit(newTile, cellToCollapse);
                }
                else
                {
                    pistonCount++;
                }
                break;
            case eTile.item:
                if (pickupCount == pickupLimit)
                {
                    //cellToCollapse.collapsed = false;
                    Debug.Log("Retrying Generation of Tile of Pickup");
                    newTile = FindTile(cellToCollapse);
                    CheckTileLimit(newTile, cellToCollapse);
                }
                else
                {
                    pickupCount++;
                }
                break;
            default:
                break;

        }
        
        return newTile;
    }
    void UpdateGeneration()
    {
        List<WFCCell> newGenerationCell = new List<WFCCell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                if (gridComponents[index].collapsed)
                {
                    //Debug.Log("called");
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    List<WFCTile> options = new List<WFCTile>();
                    foreach (WFCTile t in tileObjects)
                    {
                        options.Add(t);
                    }

                    //update above
                    if (y > 0)
                    {
                        WFCCell up = gridComponents[x + (y - 1) * dimensions];
                        List<WFCTile> validOptions = new List<WFCTile>();

                        foreach (WFCTile possibleOptions in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < dimensions - 1)
                    {
                        WFCCell right = gridComponents[x + 1 + y * dimensions];
                        List<WFCTile> validOptions = new List<WFCTile>();

                        foreach (WFCTile possibleOptions in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (y < dimensions - 1)
                    {
                        WFCCell down = gridComponents[x + (y + 1) * dimensions];
                        List<WFCTile> validOptions = new List<WFCTile>();

                        foreach (WFCTile possibleOptions in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0)
                    {
                        WFCCell left = gridComponents[x - 1 + y * dimensions];
                        List<WFCTile> validOptions = new List<WFCTile>();

                        foreach (WFCTile possibleOptions in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    WFCTile[] newTileList = new WFCTile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGenerationCell;
        iterations++;

        if (iterations < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }

    }

    void CheckValidity(List<WFCTile> optionList, List<WFCTile> validOption)
    {
        for (int i = optionList.Count - 1; i >= 0; i--)
        {
            var element = optionList[i];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(i);
            }
        }
    }

}
