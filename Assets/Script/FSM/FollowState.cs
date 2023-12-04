
using UnityEngine;

public class FollowState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        Debug.Log("StartFollow");
    }

    public override void OnUpdate(Enemy enemy)
    {
        if(!enemy.canEat && enemy.targetPoint != null) enemy.ChangeState(enemy.runawayState);
        
        if(!enemy.isFindPlayer) enemy.ChangeState(enemy.patrolState);

        enemy.FollowPlayer();
    }

    public override void OnQuit(Enemy enemy)
    {
        Debug.Log("QuitFollow");
    }
}
