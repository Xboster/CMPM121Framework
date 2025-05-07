using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using System;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Easy");

        selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 90);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Medium");

        selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 50);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Endless");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        GameManager.Instance.difficulty = levelname;
        GameManager.Instance.wave = 1;
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
        GameManager.Instance.wave++;
    }

    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;

        //scaling player stats
        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();
        UpdatePlayerStats(player);

        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        GameManager.Instance.timeStart = Time.time;

        var stage = GameManager.Instance.level_types[GameManager.Instance.difficulty];

        foreach (var spawn in stage.spawns)
        {
            Enemy baseEnemy = GameManager.Instance.enemy_types[spawn.enemy];
            if (baseEnemy == null)
            {
                Debug.LogWarning($"Enemy type {spawn.enemy} not found!");
                continue;
            }

            var vars = new Dictionary<string, float> {
                { "base", baseEnemy.hp },
                { "wave", GameManager.Instance.wave }
            };

            int totalEnemiesRemaining = (int)RPNEvaluator.Evaluate(spawn.count, vars);
            int delay = spawn.delay != null ? (Int32.Parse(spawn.delay) > 0 ? Int32.Parse(spawn.delay) : 2) : 2;
            int seqIndex = 0;

            Enemy resultEnemy = baseEnemy;
            resultEnemy.hp = spawn.hp != null ? (int)RPNEvaluator.Evaluate(spawn.hp, vars) : baseEnemy.hp;
            resultEnemy.speed = spawn.speed != null ? (int)RPNEvaluator.Evaluate(spawn.speed, vars) : baseEnemy.speed;
            resultEnemy.damage = spawn.damage != null ? (int)RPNEvaluator.Evaluate(spawn.damage, vars) : baseEnemy.damage;

            while (totalEnemiesRemaining > 0)
            {
                int batchCount = spawn.sequence != null ? Mathf.Min(spawn.sequence[seqIndex % spawn.sequence.Length], totalEnemiesRemaining) : 1;
                seqIndex++;

                for (int i = 0; i < batchCount; i++)
                {
                    SpawnEnemy(resultEnemy, spawn.location);
                }

                totalEnemiesRemaining -= batchCount;

                yield return new WaitForSeconds(delay);
            }
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        GameManager.Instance.timeEnd = Time.time;
    }

    void SpawnEnemy(Enemy e, string location = "random")
    {
        List<SpawnPoint> filteredSpawnPoints;
        SpawnPoint spawnPoint;
        if (location.StartsWith("random"))
        {
            string[] parts = location.Split(' ');
            string type = parts.Length > 1 ? parts[1] : null;

            if (string.IsNullOrEmpty(type))
            {
                filteredSpawnPoints = SpawnPoints.ToList();
            }
            else
            {
                filteredSpawnPoints = SpawnPoints.Where(p => p.kind.ToString().ToLower() == type.ToLower()).ToList();
            }

            if (filteredSpawnPoints.Count == 0)
            {
                spawnPoint = SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)];
            }
            else
            {
                spawnPoint = filteredSpawnPoints[UnityEngine.Random.Range(0, filteredSpawnPoints.Count)];
            }
        }
        else
        {
            spawnPoint = SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)];
        }


        Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(e.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();

        en.hp = new Hittable(e.hp, Hittable.Team.MONSTERS, new_enemy);

        en.speed = e.speed;
        en.damage = e.damage;

        GameManager.Instance.AddEnemy(new_enemy);
    }

    public void UpdatePlayerStats(PlayerController player)
    {
        var variables = new Dictionary<string, float>
        {
            { "wave", GameManager.Instance.wave }
        };

        float newHP = RPNEvaluator.Evaluate("95 wave 5 * +", variables);
        float newMana = RPNEvaluator.Evaluate("90 wave 10 * +", variables);
        float newRegen = RPNEvaluator.Evaluate("10 wave +", variables);
        float newSpellPower = RPNEvaluator.Evaluate("wave 10 *", variables);
        float newSpeed = RPNEvaluator.Evaluate("5", variables);

        player.SetMaxHP((int)newHP); // Preserves HP %
        player.SetMaxMana((int)newMana);
        player.SetManaRegen(newRegen);
        player.SetSpellPower((int)newSpellPower);
        player.SetMoveSpeed(newSpeed);
        // player.spellcaster.spell.SetProperties();


        Debug.Log($"[Wave {GameManager.Instance.wave}] Stats updated:");
        Debug.Log($"  HP: {(int)newHP}");
        Debug.Log($"  Max Mana: {(int)newMana}");
        Debug.Log($"  Mana Regen: {(int)newRegen}");
        Debug.Log($"  Spell Power: {(int)newSpellPower}");
        Debug.Log($"  Move Speed: {newSpeed}");
    }
}

