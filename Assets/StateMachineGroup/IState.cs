using Unity.VisualScripting;
using UnityEngine;

public interface IState
{
    public void Enter();
    public void Exit();
    public void HandleInput();
    public void Update();
    public void PhysicsUpdate();
    public void onAnimationEnter();
    public void OnAnimationExit();
    public void OnAnimationTransitionToStateary();

    public void OnTriggerEnter(Collider collider);
    public void OnTriggerExit(Collider collider);

}
