using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell : MonoBehaviour
{
    public bool collapsed;
    public WFCTile[] tileOptions;


    public void CreateCell(bool _collapseState, WFCTile[] tiles)
    {
        collapsed = _collapseState;
        tileOptions = tiles;
    }

    public void RecreateCell(WFCTile[] tiles)
    {
        tileOptions = tiles;
    }
}
