
using UnityEngine;

public class RunawayState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        Debug.Log("StartRunaway");
        enemy.StartRandomRunawayDir();
    }

    public override void OnUpdate(Enemy enemy)
    {
        if(!enemy.isFindPlayer || enemy.targetPoint == null) enemy.ChangeState(enemy.patrolState);
        if(enemy.canEat && enemy.targetPoint != null) enemy.ChangeState(enemy.followState);
        
        enemy.RunAway();
    }

    public override void OnQuit(Enemy enemy)
    {
        Debug.Log("QuitRunaway");
        enemy.StopRandomRunawayDir();
    }
}
