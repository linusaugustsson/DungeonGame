using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct attribute
{
    public int score;
    internal int minScore;
    internal int maxScore;
}

public class Attributes : MonoBehaviour
{
    [Header("ATTRIBUTES")]
    public attribute Strength;
    [Space]
    public attribute Intelligence;
    [Space]
    public attribute Dexterity;
    [Space]
    public attribute Constitution;
    [Space]
    public attribute Wisdom;
}
