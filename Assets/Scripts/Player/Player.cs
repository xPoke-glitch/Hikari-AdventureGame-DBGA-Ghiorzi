using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Player : Actor
{ 
    public int MaxDarkValue { get => maxDarkValue; }
    public int DarkValue { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }
    public Vector2Int MovingDirection { get; private set; }
    public Vector3 AttackDirection { get; private set; }
    public Vector2Int TargetPosition { get => _targetPosition; }
    
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private int initialMaxHealth = 20;
    [SerializeField]
    private int maxDarkValue = 20;

    private int experience;
    private Quest[] quests;
    private Weapon heldWeapon;
    private Armour equipedArmour;
    private Item[] consumables;
    private float potionCooldown;

    private AdventureGame _controls;
    private PlayerAnimation _playerAnimation;

    private Vector2Int _currentPosition;
    private Vector2Int _targetPosition;
    private Tile _lastTile;

    private float _attackStartTime;
    private float _attackDuration = 0.5f;

    private List<Actor> _targets = new List<Actor>();

    private int _currentTargetIndex = 0;

    public void AddTarget(Actor target)
    {
        bool isFirst = false;
        if(_targets.Count == 0)
            isFirst = true;

        _targets.Add(target);

        if (isFirst)
        {
            Monster monst = (Monster)_targets[0];
            monst.TargetIndicator.Active();
        }
    }

    public void RemoveTarget(Actor target)
    {
        _targets.Remove(target);
        if (_targets.Count == 0)
        {
            GameController.Instance.EndBattle();
            return;
        }

        _currentTargetIndex = 0;
        Monster monst = (Monster)_targets[_currentTargetIndex];
        monst.TargetIndicator.Active();
    }

    public void SetPosition(Vector2Int position)
    {
        _currentPosition = position;
        transform.position = new Vector3(_currentPosition.x, transform.position.y, _currentPosition.y);
    }

    public void SetHeight(float height)
    {
        transform.position = new Vector3(_currentPosition.x, height, _currentPosition.y);
    }

    public void StopMoving()
    {
        IsMoving = false;
    }

    private void Awake()
    {
        DarkValue = 0;
        currentHealth = initialMaxHealth;
        maxHealth = initialMaxHealth;
        IsMoving = false;
        IsInBattle = false;
        _controls = new AdventureGame();
        _playerAnimation = GetComponentInChildren<PlayerAnimation>();
        _controls.Player.Move.performed += context => BeginMove(context.ReadValue<Vector2>());
        _controls.Player.Attack.performed += context => Attack();
        _controls.Player.SwitchTarget.performed += context => SwitchTarget();
    }

    private void Update()
    {
        if (IsMoving)
        {
            if (!IsInBattle)
            {
                Vector3 targetPos = new Vector3(_targetPosition.x, 0.28f, _targetPosition.y);
                if (Vector3.Distance(transform.position, targetPos) > float.Epsilon)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                }
                else
                {
                    _currentPosition = _targetPosition;

                    Tile tile = DungeonController.Instance.CurrentRoom.Tiles[_currentPosition.x, _currentPosition.y];
                    if (tile != null && tile.IsWalkable)
                    {
                        if (_lastTile != null)
                            _lastTile.UnsetCharacterObject();

                        tile.EnterTile();
                        tile.SetCharacterObject(this);
                        _lastTile = tile;



                        if (_controls.Player.Move.inProgress && IsMoving)
                        {
                            FindNewTargetPosition(MovingDirection);
                            MonsterController.Instance.MoveMonsters();
                            return;
                        }
                    }
                    MonsterController.Instance.MoveMonsters();
                    IsMoving = false;
                }
            }
            else
            {
                IsMoving = false;
            }
           
        }

        if (IsAttacking)
        {
            if (!IsInBattle)
            {
                float t = (Time.time - _attackStartTime) / _attackDuration;
                Vector3 dir = new Vector3(MovingDirection.x,0.28f,MovingDirection.y);
                transform.position = DungeonController.Instance.GetTile(_currentPosition).TileObj.transform.position + dir * Mathf.PingPong(t,0.5f);
                if (t > 1f)
                {
                    // Finish
                    // procees damages and stuff
                    MonsterController.Instance.MoveMonsters();
                    IsAttacking = false;
                }
            }
            else if(GameController.Instance.State == GameController.eGameState.PlayerTurn)
            {
                float t = (Time.time - _attackStartTime) / _attackDuration;
                Vector3 attackPos = new Vector3(_targets[_currentTargetIndex].transform.position.x, 0.0f, _targets[_currentTargetIndex].transform.position.z);
                Vector3 dir = attackPos - transform.position;
                AttackDirection = dir;
                transform.position = DungeonController.Instance.GetTile(_currentPosition).TileObj.transform.position + dir * Mathf.PingPong(t, 0.5f);
                if (t > 1f)
                {
                    // Finish
                    // procees damages and stuff
                    _targets[_currentTargetIndex].Damage(4);
                    AddDarkness(2);
                    MonsterController.Instance.MoveMonsters();
                    IsAttacking = false;
                    GameController.Instance.EndTurn();
                }
            }
        }
    }

    private void OnEnable()
    {
        _controls.Enable();
        UIController.Instance.SetUpPlayer(this);
    }

    private void OnDestroy()
    {
        _controls.Disable();
    }

    private void FindNewTargetPosition(Vector2 direction)
    {
        Vector2Int intDirection = new Vector2Int((int)direction.x, (int)direction.y);
        MovingDirection = intDirection;
        Vector2Int position = _currentPosition + intDirection;

        if (position.x < 0 || position.y < 0
            || position.x >= DungeonController.Instance.CurrentRoom.Size.x
            || position.y >= DungeonController.Instance.CurrentRoom.Size.y
            || DungeonController.Instance.CurrentRoom.Tiles[position.x,position.y]==null
            || !DungeonController.Instance.GetTile(position).IsWalkable)
        {
            return;
        }

        IsMoving = true;
        _targetPosition = position;
    }

    private void BeginMove(Vector2 direction)
    {
        if (CinematicController.Instance.IsPlaying)
            return;

        Vector2Int intDirection = new Vector2Int((int)direction.x, (int)direction.y);
        MovingDirection = intDirection;
        Vector2Int position = _currentPosition + intDirection;

        if(position.x<0 || position.y < 0 
            || position.x >= DungeonController.Instance.CurrentRoom.Size.x
            || position.y >= DungeonController.Instance.CurrentRoom.Size.y
            || DungeonController.Instance.CurrentRoom.Tiles[position.x, position.y] == null
            || !DungeonController.Instance.GetTile(position).IsWalkable)
        {
            return;
        }

        Tile tile = DungeonController.Instance.CurrentRoom.Tiles[position.x, position.y];
        if(!IsMoving && tile != null && tile.IsWalkable)
        {
            IsMoving = true;
            _targetPosition = position;
        }
    }

    private void Attack()
    {
        if (IsMoving || IsAttacking)
            return;

        if (CinematicController.Instance.IsPlaying)
            return;
        if (GameController.Instance.State != GameController.eGameState.PlayerTurn && IsInBattle)
            return;

        _attackStartTime = Time.time;
        _playerAnimation.AttackAnimation();
        IsAttacking = true;
    }

    private void SwitchTarget()
    {
        if (_targets.Count <= 1)
            return;

        Monster monst = (Monster)_targets[_currentTargetIndex];
        monst.TargetIndicator.Deactive();
        
        _currentTargetIndex = (_currentTargetIndex+1)%_targets.Count;

        monst = (Monster)_targets[_currentTargetIndex];
        monst.TargetIndicator.Active();
    }

    protected override void OnKill()
    {
        // Handle Game Over Here
        foreach(Actor target in _targets)
        {
            Monster monst = (Monster)target;
            monst.TargetIndicator.Deactive();
        }
        GameController.Instance.EndBattle();
        base.OnKill();
    }

    private void AddDarkness(int amount)
    {
        if (amount > maxDarkValue || (DarkValue+amount)>maxDarkValue)
            DarkValue = maxDarkValue;
        DarkValue += amount;
    }

    void OnLevelUp()
    {

    }
}
