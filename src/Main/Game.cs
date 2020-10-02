using Godot;
using System;

public class Game : Node
{

    private readonly float _minEnemySpawnTime = 0.5f;
    private int _level;
    private int _score;
    private float _enemySpawnTime = 4f;
    private float _spawnTimeDecreasePerc = 0.1f;
    private Random rnd = new Random();
    private HUD _hud;
    private Player _player;
    private PackedScene[] _enemyScenes = new PackedScene[4];
    private PackedScene[] _powerUpScenes = new PackedScene[3];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        LoadEnemyScenes();
        LoadPowerUpScenes();

        _hud = GetNode<HUD>("HUD");
        _player = GetNode<Player>("Player");
        Start();
    }

    private void LoadEnemyScenes()
    {
        // TODO: make this dynamic
        _enemyScenes[0] = ResourceLoader.Load("res://src/Actors/Enemies/Enemy.tscn") as PackedScene;
        _enemyScenes[1] = ResourceLoader.Load("res://src/Actors/Enemies/WaveEnemy.tscn") as PackedScene;
        _enemyScenes[2] = ResourceLoader.Load("res://src/Actors/Enemies/CornerEnemy.tscn") as PackedScene;
        _enemyScenes[3] = ResourceLoader.Load("res://src/Actors/Enemies/AimEnemy.tscn") as PackedScene;
    }

    private void LoadPowerUpScenes()
    {
        _powerUpScenes[0] = ResourceLoader.Load("res://src/Objects/PowerUps/SpeedPowerUp.tscn") as PackedScene;
        _powerUpScenes[1] = ResourceLoader.Load("res://src/Objects/PowerUps/HealthPowerUp.tscn") as PackedScene;
        _powerUpScenes[2] = ResourceLoader.Load("res://src/Objects/PowerUps/ShieldPowerUp.tscn") as PackedScene;
    }

    private void Start()
    {
        // Remove any remaining bullets
        var bullets = GetTree().GetNodesInGroup("Bullet");
        foreach (Bullet bullet in bullets)
        {
            bullet.QueueFree();
        }

        var startPosition = GetNode<Position2D>("PlayerStartPosition");
        _player.Position = startPosition.Position;

        var enemyTimer = GetNode<Timer>("EnemySpawnTimer");
        enemyTimer.WaitTime = _enemySpawnTime;
        enemyTimer.Start();

        var powerUpTimer = GetNode<Timer>("PowerUpSpawnTimer");
        powerUpTimer.Start();

        _score = 0;
        _level = 1;
        _player.Start();
        _hud.Start();
    }

    public void OnEnemySpawnTimerTimeout()
    {
        // TODO: pick a random enemy to spawn
        PackedScene scene = _enemyScenes[rnd.Next(0, _enemyScenes.Length)];
        Enemy enemy = scene.Instance() as Enemy;
        enemy.Connect("Death", this, nameof(UpdateScore));
        AddChild(enemy);
    }

    private void UpdateScore()
    {
        _score++;
        _hud.UpdateScore(_score);

        if (_score % 10 == 0)
        {
            IncreaseDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        _level++;
        _hud.UpdateLevel(_level);

        if (_enemySpawnTime > _minEnemySpawnTime)
        {
            _enemySpawnTime -= _enemySpawnTime * _spawnTimeDecreasePerc;
            var enemyTimer = GetNode<Timer>("EnemySpawnTimer");
            enemyTimer.WaitTime = _enemySpawnTime;
            enemyTimer.Start();
        }
    }

    // Spawn a random power up
    public void OnPowerUpSpawnTimerTimeout()
    {
        PackedScene scene = _powerUpScenes[rnd.Next(0, _powerUpScenes.Length)];
        PowerUp powerUp = scene.Instance() as PowerUp;
        AddChild(powerUp);
    }

    public void OnPlayerHit()
    {
        _hud.RemoveLife(_player.Health);
    }

    public void OnPlayerDeath()
    {
        // Show GameOver UI
        var gameOverScene = ResourceLoader.Load("res://src/UserInterface/GameOver.tscn") as PackedScene;
        var gameOverNode = gameOverScene.Instance() as Node;
        AddChild(gameOverNode);

        // Connect signals to NewGame and EndGame
        gameOverNode.Connect("NewGame", this, "OnGameOverNewGame");
        gameOverNode.Connect("EndGame", this, "OnGameOverEndGame");

        // Remove all enemies and power ups from screen, let bullets stay on
        var enemies = GetTree().GetNodesInGroup("Enemy");
        foreach (Enemy e in enemies)
        {
            e.QueueFree();
        }

        var powerUps = GetTree().GetNodesInGroup("PowerUp");
        foreach (PowerUp u in powerUps)
        {
            u.QueueFree();
        }

        // Stop spawning enemies and power ups
        var enemyTimer = GetNode<Timer>("EnemySpawnTimer");
        enemyTimer.Stop();

        var powerUpTimer = GetNode<Timer>("PowerUpSpawnTimer");
        powerUpTimer.Stop();
    }

    public void OnPlayerHitHealthPowerUp()
    {
        _hud.AddLife();
    }

    public void OnGameOverNewGame()
    {
        Start();
    }

    public void OnGameOverEndGame()
    {
        GetTree().Quit();
    }
}
