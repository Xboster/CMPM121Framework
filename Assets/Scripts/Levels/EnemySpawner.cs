using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;



    private List<EnemyData> enemyTypes;
    private List<LevelData> levels;
    private LevelData currentLevel;

    List<SpawnPoint> filtered;

    void Awake()
    {
        // Read enemies.json
        string enemiesJson = Resources.Load<TextAsset>("enemies").text;
        enemyTypes = JsonConvert.DeserializeObject<List<EnemyData>>(enemiesJson);

        // Read levels.json
        string levelsJson = Resources.Load<TextAsset>("levels").text;
        levels = JsonConvert.DeserializeObject<List<LevelData>>(levelsJson);
    
        Debug.Log("First enemy is: " + enemyTypes[0].name); // Should say zombie
        Debug.Log("First level name: " + levels[0].name); // Should say Easy
    }    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int spacing = 100;
        for (int i = 0; i < levels.Count; ++i)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, 130 -i  * spacing, 0);

            string levelName = levels[i].name;
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(levelName);
        }

        //testing the rpn
        var vars = new Dictionary<string, int> {
            { "base", 20 },
            { "wave", 3 }
        };

        string expr = "base 5 wave * +";
        int result = RPNEvaluator.Evaluate(expr, vars);
        Debug.Log($"Evaluated result: {result}"); // Should log 35

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelName)
    {
        currentLevel = levels.Find(l => l.name == levelName);
        GameManager.Instance.wave = 1;
        level_selector.gameObject.SetActive(false);
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
    }


    public void NextWave()
    {
        GameManager.Instance.wave++;
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;

        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;

        int waveNum = GameManager.Instance.wave; // current wave index (starts at 1)

        foreach (var spawn in currentLevel.spawns)
        {
            // Get the base enemy type data
            EnemyData baseEnemy = enemyTypes.Find(e => e.name == spawn.enemy);
            if (baseEnemy == null)
            {
                Debug.LogWarning($"Enemy type {spawn.enemy} not found!");
                continue;
            }

            // Prepare variable values
            var vars = new Dictionary<string, int> {
                { "base", baseEnemy.hp },
                { "wave", waveNum }
            };

            // Evaluate how many enemies to spawn
            int totalCount = RPNEvaluator.Evaluate(spawn.count, vars);
            int hp = spawn.hp != null ? RPNEvaluator.Evaluate(spawn.hp, vars) : baseEnemy.hp;
            int speed = spawn.speed != null ? RPNEvaluator.Evaluate(spawn.speed, vars) : baseEnemy.speed;
            int damage = spawn.damage != null ? RPNEvaluator.Evaluate(spawn.damage, vars) : baseEnemy.damage;

            List<int> sequence = spawn.sequence ?? new List<int> { 1 };
            int delay = spawn.delay > 0 ? spawn.delay : 2;
            int seqIndex = 0;

            while (totalCount > 0)
            {
                int batchCount = Mathf.Min(sequence[seqIndex % sequence.Count], totalCount);
                seqIndex++;

                for (int i = 0; i < batchCount; i++)
                {
                    SpawnEnemy(baseEnemy.sprite, hp, speed, damage, spawn.location);
                }

                totalCount -= batchCount;

                yield return new WaitForSeconds(delay);
            }
        }

        // Wait until all enemies are dead
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }


    void SpawnEnemy(int spriteIndex, int hp, int speed, int damage, string locationType)
    {
        SpawnPoint spawnPoint;

        if (locationType.StartsWith("random"))
        {
            string[] parts = locationType.Split(' ');
            string type = parts.Length > 1 ? parts[1] : null;

            if (string.IsNullOrEmpty(type))
            {
                filtered = SpawnPoints.ToList();
            } else
            {
                filtered = SpawnPoints.Where(p => p.kind.ToString().ToLower() == type.ToLower()).ToList();
            }

            if (filtered.Count == 0)
                spawnPoint = SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)];
            else
                spawnPoint = filtered[UnityEngine.Random.Range(0, filtered.Count)];
        }
        else
        {
            spawnPoint = SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)];
        }

        Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.8f;
        Vector3 pos = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);

        GameObject new_enemy = Instantiate(enemy, pos, Quaternion.identity);
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(spriteIndex);

        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = speed;

        GameManager.Instance.AddEnemy(new_enemy);
    }


}
