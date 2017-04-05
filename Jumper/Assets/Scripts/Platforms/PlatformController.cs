﻿using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.Platforms
{
    public class PlatformController : MonoBehaviour
    {
        private readonly PlatformTypes[] _lastPlatformTypes = new PlatformTypes[2];
        private readonly Random _random = new Random();

        private Sprite[] _platforms;
        private Sprite[] _enemies;

        public GameObject Platform;
        public GameObject Enemy;
        public static PlatformController Instance { get; private set; }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            GetSprites();
            for (var i = Constants.LeftBorder; i < Constants.PlatformSpawnPoint; i += Constants.PlatformSize)
                SpawnPlatforms(i, PlatformTypes.Simple);
        }

        private void GetSprites()
        {
            _platforms = Resources.LoadAll<Sprite>(Constants.PlatformSpritesFolder);
            _enemies = Resources.LoadAll<Sprite>(Constants.EnemiesSpriteFolder);
        }

        public void SpawnPlatforms()
        {
            UpdateLastPlatforms();
            var platform = MainSpawnPlatform(Constants.PlatformSpawnPoint);
            SetSprite(platform);
            SetAdditionalFeatures(platform);
            if (_random.NextDouble() > Constants.EnemySpawnChance && _lastPlatformTypes[0] == (PlatformTypes.Empty & PlatformTypes.Carton)) return;
            SpawnEnemies(platform);
        }

        private void SpawnPlatforms(float positionX, PlatformTypes platformType)
        {
            var platform = MainSpawnPlatform(positionX);
            platform.GetComponent<SpriteRenderer>().sprite = _platforms[(int)platformType];
        }

        private void SpawnEnemies(GameObject parentPlatform)
        {
            var enemy = Instantiate(Enemy);
            var index = _random.Next(2);
            enemy.transform.parent = parentPlatform.transform;
            enemy.GetComponent<SpriteRenderer>().sprite = _enemies[index];
            enemy.AddComponent<Enemy>().HealthPoints = index;
        }

        private GameObject MainSpawnPlatform(float positionX)
        {
            var platform = Instantiate(Platform);
            platform.transform.parent = gameObject.transform;
            platform.transform.localPosition = new Vector3(positionX, Constants.PlatformPositionY);
            platform.AddComponent<Platform>();
            return platform;
        }

        private void SetAdditionalFeatures(GameObject platform)
        {
            switch (_lastPlatformTypes[0])
            {
                case PlatformTypes.Dissapearing:
                    platform.AddComponent<DissapearingPlatform>();
                    break;
                case PlatformTypes.Trampoline:
                    platform.AddComponent<Trampoline>();
                    break;
                case PlatformTypes.Freezing:
                    platform.AddComponent<Freezing>();
                    break;
                case PlatformTypes.Carton:
                    platform.AddComponent<EmptyPlatform>();
                    break;
                case PlatformTypes.Empty:
                    platform.AddComponent<EmptyPlatform>();
                    break;
            }
        }

        private void UpdateLastPlatforms()
        {
            var index = GetAvailablePlatformIndex();
            _lastPlatformTypes[1] = _lastPlatformTypes[0];
            _lastPlatformTypes[0] = (PlatformTypes)index;
        }

        private int GetAvailablePlatformIndex()
        {
            if (_lastPlatformTypes[0] != (PlatformTypes.Empty & PlatformTypes.Carton) && _lastPlatformTypes[1] != PlatformTypes.Trampoline)
                if (_random.NextDouble() < Constants.EmptyPlatformChance)
                    return _random.Next((int)PlatformTypes.Carton, (int)PlatformTypes.Empty + 1);
            if (_random.NextDouble() < Constants.SimplePlatformChance)
                return 0;
            var index = Constants.PlatformsNumber;
            while (index == ((int)PlatformTypes.Empty | (int)PlatformTypes.Carton))
                index = _random.Next(1, (int)PlatformTypes.Empty - 1);
            return index;
        }

        private void SetSprite(GameObject platform)
        {
            if ((int)_lastPlatformTypes[0] >= _platforms.Length)
            {
                platform.GetComponent<SpriteRenderer>().sprite = null;
                return;
            }
            platform.GetComponent<SpriteRenderer>().sprite = _platforms[(int)_lastPlatformTypes[0]];
        }
    }
}