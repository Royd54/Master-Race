using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;
	[SerializeField] private TakeCoverAI _enemyPrefab;
	[SerializeField] private float _spawnInterval;
	[SerializeField] private float _maxEnemiesNumber;
	[SerializeField] private Player _player;

    [SerializeField] private float _enemyMultiplier = 1.5f;
    [SerializeField] private float _spawnIntervalMultiplier = 0.8f;
    [SerializeField] private float _enemyCount;

    [SerializeField] private float _currentEnemyCount;

    private List<TakeCoverAI> spawnedEnemies = new List<TakeCoverAI>();
	private float timeSinceLastSpawn;

	private void Start()
	{
		timeSinceLastSpawn = _spawnInterval;

        GameEvents.instance.onEnemyDeath += OnEnemyDeath;
	}

	private void Update()
	{
		timeSinceLastSpawn += Time.deltaTime;
		if(timeSinceLastSpawn > _spawnInterval || _currentEnemyCount <= 0)
		{
			timeSinceLastSpawn = 0f;
			if(spawnedEnemies.Count < _maxEnemiesNumber)
			{
				SpawnEnemy();
			}
		}

        if (_enemyCount <= 0)
        {
            NextWave();
        }
    }

	private void SpawnEnemy()
	{
        TakeCoverAI enemy = Instantiate(_enemyPrefab, transform.position, transform.rotation);
		int spawnPointindex = spawnedEnemies.Count % _spawnPoints.Length;
		enemy.Init(_player, _spawnPoints[spawnPointindex]);
		spawnedEnemies.Add(enemy);
        _currentEnemyCount++;
    }

    private void NextWave()
    {
        _maxEnemiesNumber = Mathf.Round( (_maxEnemiesNumber += 2) * _enemyMultiplier);
        spawnedEnemies.Clear();
        _enemyCount = _maxEnemiesNumber;
        _spawnInterval = _spawnInterval * _spawnIntervalMultiplier;
    }

    private void OnEnemyDeath()
    {
        _enemyCount--;
        _currentEnemyCount--;
    }
}
