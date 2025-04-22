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
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
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
        for (int i = 0; i < 10; ++i)
        {
            yield return SpawnZombie();
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
