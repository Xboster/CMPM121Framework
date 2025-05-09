using UnityEngine;
using TMPro;
using System;

public class RewardScreenManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI timeSpent;
    [SerializeField]
    TextMeshProUGUI damageDealt;
    [SerializeField]
    TextMeshProUGUI damageRecieved;
    [SerializeField]
    DamageTracker damageTracker;
    public GameObject rewardUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            rewardUI.SetActive(true);

            timeSpent.text = "Time Spent: " + Math.Round(GameManager.Instance.timeEnd - GameManager.Instance.timeStart, 2);
            damageDealt.text = "Damage Dealt: " + GameManager.Instance.damageDealt;
            if (damageTracker != null)
            {
                damageRecieved.text = "Damage Recieved: " + damageTracker.GetTotalDamageTaken();
            }
            else
            {
                damageRecieved.text = "Damage Recieved: N/A";
            }
        }
        else
        {
            rewardUI.SetActive(false);
        }
    }
}
