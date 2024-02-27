using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MapObject", menuName = "Map Editor Object", order = 1)]
public class MapObject : ScriptableObject
{
    public enum GridAttachmentType
    {
        AnyCellEdge,           // Represents an attachment to any edge of a cell, including between cells or between an edge and an empty cell. Can be used for walls or other objects.
        OnlyWallEdge,               // Represents a solid barrier attached to the edges of a grid, between a cell and an empty cell.
        OnlyCellConnector,      // Represents a connector/link between the edges of two adjacent cells.
        DualCellConnector,  // Represents an attachment that spans two adjacent cells either horizontally or vertically.
        InsideCell,         // Represents an attachment inside a cell, not necessarily at the edges but in the space between(There exists a 9 cell grid inside each cell for object placement).
    }

    public GameObject asset;
    public GridAttachmentType attatchType;
    
}
