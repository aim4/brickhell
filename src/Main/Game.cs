using Godot;
using System;

public class Game : Node
{

    private HUD _hud;
    private Player _player;
    private PackedScene _enemyScene;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _enemyScene = ResourceLoader.Load("res://src/Actors/Enemy.tscn") as PackedScene;
        _hud = GetNode<HUD>("HUD");
        _player = GetNode<Player>("Player");
        Start();
    }

    public override void _Process(float delta)
    {

    }

    private void Start()
    {
        var startPosition = GetNode<Position2D>("PlayerStartPosition");
        _player.Position = startPosition.Position;

        var timer = GetNode<Timer>("EnemySpawnTimer");
        timer.Start();
    }

    public void OnEnemySpawnTimerTimeout()
    {
        Enemy enemy = _enemyScene.Instance() as Enemy;
        AddChild(enemy);
    }

    public void OnPlayerHit()
    {
        _hud.RemoveLife();
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

        // Remove all enemies from screen
        var enemies = GetTree().GetNodesInGroup("Enemy");
        foreach (Enemy enemy in enemies)
        {
            enemy.QueueFree();
        }

        // Stop spawning enemies
        var timer = GetNode<Timer>("EnemySpawnTimer");
        timer.Stop();
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
