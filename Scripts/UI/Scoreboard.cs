using GameKit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Scoreboard : MonoBehaviour
{

    public static Scoreboard instance;
    private CanvasGroup scoreboardCanvas;
    public ScoreboardItem scoreboardItemPrefab;
    public Transform scoreboardParent;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        scoreboardCanvas = GetComponent<CanvasGroup>();
    }
    private void Update()
    {

        if (scoreboardCanvas == null)
        {
            return;
        }

        if(Input.GetKey(KeyCode.Tab))
        {
            scoreboardCanvas.alpha = 1;
        } else
        {
            scoreboardCanvas.alpha = 0;
        }
    }

    public void ClearScoreboard()
    {
        scoreboardParent.DestroyChildren();
    }

    public void ReorderScoreboardItems()
    {
        ScoreboardItem[] scoreboardItems = scoreboardParent.GetComponentsInChildren<ScoreboardItem>();
        ScoreboardItem[] scoreboardItemsOrdered = scoreboardItems.OrderBy(item => item.playerScore.text).ToArray();

        for (int i = 0; i < scoreboardItemsOrdered.Length; i++)
        {
            scoreboardItemsOrdered[i].transform.SetSiblingIndex(i);
        }

    }


}
