using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : Singleton<MonsterController>
{
    public List<Monster> ActiveMonsters { get=>_activeMonsters;}

    [SerializeField]
    private MonsterCollection monsterCollection;

    private List<Monster> _activeMonsters = new List<Monster>();

    public Monster AddMonster(eMonsterID monsterID)
    {
        Monster newMonster = monsterCollection.GetMonster(monsterID);
        ActiveMonsters.Add(newMonster);
        return newMonster;
    }

    public void RemoveMonster(Monster monster)
    {
        ActiveMonsters.Remove(monster);
    }

    public void DestroyAllMonster()
    {
        foreach(Monster monster in ActiveMonsters)
        {
            DungeonController.Instance.GetTile(new Vector2Int((int)monster.transform.position.x,(int)monster.transform.position.z)).UnsetCharacterObject();
            Destroy(monster.gameObject);
        }
        _activeMonsters.Clear();
    }

    public void MoveMonsters()
    {
        foreach(Monster monster in ActiveMonsters)
        {
            if (!monster.CheckForPlayer())
            {
                monster.Move();
                monster.CheckForPlayer();
            }
        }
    }
}
