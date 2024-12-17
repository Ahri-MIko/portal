
using UnityEngine;

public class StateMachine
{
    protected IState currentState;
    public void ChangeState(IState nextState)
    {
        //ע�������C#8��һ���﷨��,�����жϵ�ǰ��ʵ�������Ƿ�Ϊ��,�����Ϊ�ղŵ��ú���ĺ���
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
