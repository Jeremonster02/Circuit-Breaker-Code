using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCTile : MonoBehaviour
{
    public eTile tileType;
    public WFCTile[] upNeighbours;
    public WFCTile[] rightNeighbours;
    public WFCTile[] downNeighbours;
    public WFCTile[] leftNeighbours;
}
