using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoloSpawnEvent : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private TakeCoverAI _enemyPrefab;
    [SerializeField] private float _spawnInterval;
    [SerializeField] private float _maxEnemiesNumber;
    [SerializeField] private Player _player;

    private List<TakeCoverAI> spawnedEnemies = new List<TakeCoverAI>();
    private float timeSinceLastSpawn;

    private void Start()
    {
        timeSinceLastSpawn = _spawnInterval;
    }

    private void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _player.gameObject)
        {
            SpawnEnemy();
            GameEvents.instance.LightsTrigger();
        }
    }

    private void SpawnEnemy()
    {
        if (timeSinceLastSpawn > _spawnInterval)
        {
            timeSinceLastSpawn = 0f;
            if (spawnedEnemies.Count < _maxEnemiesNumber)
            {
                int spawnPointindex = spawnedEnemies.Count % _spawnPoints.Length;
                TakeCoverAI enemy = Instantiate(_enemyPrefab, _spawnPoints[spawnPointindex].position, transform.rotation);
                enemy.Init(_player, _spawnPoints[spawnPointindex]);
                spawnedEnemies.Add(enemy);
            }
        }
    }
}
