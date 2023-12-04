
using UnityEngine;

public class PatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        Debug.Log("StartPatrol");
        enemy.StartRandomDir();
    }

    public override void OnUpdate(Enemy enemy)
    {
        if (enemy.isFindPlayer && enemy.targetPoint != null)
        {
            if(enemy.canEat)
                enemy.ChangeState(enemy.followState);   //切换至跟随状态
            else
            {
                enemy.ChangeState(enemy.runawayState);  //切换至逃跑状态
            }
        }
        
        enemy.Movement();
    }

    public override void OnQuit(Enemy enemy)
    {
        Debug.Log("QuitPatrol");
        enemy.StopRandomDir();
    }
}
