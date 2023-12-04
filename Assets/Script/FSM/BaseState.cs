//状态机基类
public abstract class BaseState
{
    public abstract void OnEnter(Enemy enemy);
    public abstract void OnUpdate(Enemy enemy);
    public abstract void OnQuit(Enemy enemy);
}
