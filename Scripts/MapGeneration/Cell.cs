using System;
using UnityEngine;

[Flags]
public enum DirectionFlag
{
    None = 0,
    Forward = 1,
    Right = 2,
    Back = 4,
    Left = 8
}


/// <summary>
/// Represents a cell of a map or a tile, will set correct state depending on NeighborState flag.
/// </summary>
public class Cell : MonoBehaviourRefSetup
{
    [SerializeField, HideInInspector] GameObject forwardWall;
    [SerializeField, HideInInspector] GameObject backWall;
    [SerializeField, HideInInspector] GameObject rightWall;
    [SerializeField, HideInInspector] GameObject leftWall;

    //TODO: Generate seperatly in map-generator along walls corner to avoid duplicate etc.
    [SerializeField, HideInInspector] GameObject forwardRightPillar;
    [SerializeField, HideInInspector] GameObject forwardLeftPillar;
    [SerializeField, HideInInspector] GameObject backRightPillar;
    [SerializeField, HideInInspector] GameObject backLeftPillar;

    public void SetupCell(DirectionFlag state)
    {
        bool forwardShouldBeActive = (state & DirectionFlag.Forward) == 0;
        bool backShouldBeActive = (state & DirectionFlag.Back) == 0;
        bool rightShouldBeActive = (state & DirectionFlag.Right) == 0;
        bool leftShouldBeActive = (state & DirectionFlag.Left) == 0;

        forwardWall.SetActive(forwardShouldBeActive);
        backWall.SetActive(backShouldBeActive);
        rightWall.SetActive(rightShouldBeActive);
        leftWall.SetActive(leftShouldBeActive);


        forwardRightPillar.SetActive(forwardShouldBeActive || rightShouldBeActive);
        forwardLeftPillar.SetActive(forwardShouldBeActive || leftShouldBeActive);
        backRightPillar.SetActive(backShouldBeActive || rightShouldBeActive);
        backLeftPillar.SetActive(backShouldBeActive || leftShouldBeActive);
    }

    override protected void SetupReferences()
    {
        var pivot = transform.Find("Pivot");
        forwardWall = pivot.Find("ForwardWall").gameObject;
        backWall = pivot.Find("BackWall").gameObject;
        rightWall = pivot.Find("RightWall").gameObject;
        leftWall = pivot.Find("LeftWall").gameObject;
        forwardRightPillar = pivot.Find("FR_Pillar").gameObject;
        forwardLeftPillar = pivot.Find("FL_Pillar").gameObject;
        backRightPillar = pivot.Find("BR_Pillar").gameObject;
        backLeftPillar = pivot.Find("BL_Pillar").gameObject;
    }

    internal Vector3 GetCellPlacement(int x, int y)
    {
        float xPos =  x / 3.0f - 0.33f, yPos = y / 3.0f - 0.33f;
        return transform.position + Vector3.right * xPos + Vector3.forward * yPos;
    }

    internal Vector3 GetWallPosition(EditorCell.WallPosition wallPos, out Transform wallTransform)
    {
        switch (wallPos)
        {
            case EditorCell.WallPosition.Forward:
                wallTransform = forwardWall.transform;
                break;
            case EditorCell.WallPosition.Back:
                wallTransform = backWall.transform;
                break;
            case EditorCell.WallPosition.Right:
                wallTransform = rightWall.transform;
                break;
            case EditorCell.WallPosition.Left:
                wallTransform = leftWall.transform;
                break;
            default:
                wallTransform = null;
                break;
        }

        return wallTransform.position;

    }
}
