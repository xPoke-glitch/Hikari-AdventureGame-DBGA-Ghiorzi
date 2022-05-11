using UnityEngine;

public class Player : MonoBehaviour
{ 
    public bool IsMoving { get; private set; }
    public Vector2Int MovingDirection { get; private set; }
    
    [SerializeField]
    private float moveSpeed;

    private int maxHealth;
    private int currentHealth;
    private int experience;
    private Quest[] quests;
    private Weapon heldWeapon;
    private Armour equipedArmour;
    private Item[] consumables;
    private float potionCooldown;

    private AdventureGame _controls;

    private Vector2Int _currentPosition;
    private Vector2Int _targetPosition;
   
    public void SetPosition(Vector2Int position)
    {
        _currentPosition = position;
        transform.position = new Vector3(_currentPosition.x, 0.28f, _currentPosition.y);

    }

    private void Awake()
    {
        IsMoving = false;
        _controls = new AdventureGame();
        _controls.Player.Move.performed += context => BeginMove(context.ReadValue<Vector2>());
    }

    private void Update()
    {
        if (IsMoving)
        {
            Vector3 targetPos = new Vector3(_targetPosition.x, 0.28f, _targetPosition.y);
            if(Vector3.Distance(transform.position, targetPos) > float.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
            }
            else
            {
                _currentPosition = _targetPosition;

                Tile tile = DungeonController.Instance.CurrentRoom.Tiles[_currentPosition.x, _currentPosition.y];
                DungeonController.Instance.EnterTile(tile);

                IsMoving = false;
                
            }
        }
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDestroy()
    {
        _controls.Disable();
    }

    private void BeginMove(Vector2 direction)
    {
        Vector2Int intDirection = new Vector2Int((int)direction.x, (int)direction.y);
        MovingDirection = intDirection;
        Vector2Int position = _currentPosition + intDirection;

        if(position.x<0 || position.y < 0 
            || position.x >= DungeonController.Instance.CurrentRoom.Size.x
            || position.y >= DungeonController.Instance.CurrentRoom.Size.y)
        {
            return;
        }

        Tile tile = DungeonController.Instance.CurrentRoom.Tiles[position.x, position.y];
        if(!IsMoving && tile != null)
        {
            IsMoving = true;
            _targetPosition = position;
        }

    }

    void OnKill()
    {

    }

    void OnLevelUp()
    {

    }
}