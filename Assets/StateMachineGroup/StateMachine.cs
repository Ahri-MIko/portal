
using UnityEngine;

public class StateMachine
{
    protected IState currentState;
    public void ChangeState(IState nextState)
    {
        //注意这个是C#8的一种语法糖,就是判断当前的实例对象是否为空,如果不为空才调用后面的函数
        currentState?.Exit();
        currentState = nextState;
        nextState.Enter();
    }

    public void HandleInput()
    {
        currentState?.HandleInput();
    }

    public void Update()
    {
        currentState?.Update();
    }

    public void PhysicsUpdate()
    {
        currentState?.PhysicsUpdate();
    }

    #region Animation Methonds
    public void OnAnimationEnter()
    {
        currentState?.onAnimationEnter();
    }

    public void OnAnimationExit()
    {
        currentState?.OnAnimationExit();
    }

    public void OnAnimationTransitionStateary()
    {
        currentState?.OnAnimationTransitionToStateary();
    }

    public void OnTriggerEnter(Collider collider)
    {
        currentState?.OnTriggerEnter(collider);
    }

    public void OnTriggerExit(Collider collider)
    {
        currentState?.OnTriggerExit(collider);
    }

    #endregion
}
