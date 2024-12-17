using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/*
 * 使用方法
DisableInputForSeconds(PlayerinputActions.Jump, 2f);
// 如果需要提前取消禁用
CancelDisableInput(PlayerinputActions.Jump);
 */
public class MoveInputs : MonoBehaviour
{
    public PlayerInputsActions PlayerInputMaps { get; private set; }
    public PlayerInputsActions.PlayerActions PlayerinputActions { get; private set; }

    // 用于追踪正在禁用的输入动作
    private Dictionary<InputAction, Coroutine> _disableCoroutines = new Dictionary<InputAction, Coroutine>();

    private void Awake()
    {
        PlayerInputMaps = new PlayerInputsActions();
        PlayerinputActions = PlayerInputMaps.Player;
    }

    private void OnEnable()
    {
        PlayerInputMaps.Enable();
    }

    private void OnDisable()
    {
        PlayerInputMaps.Disable();
        // 确保清理所有正在运行的协程
        StopAllDisableCoroutines();
    }

    public void DisableInputForSeconds(InputAction input, float time)
    {
        if (input == null)
            return;

        // 如果该输入动作已经在禁用状态，先停止之前的协程
        if (_disableCoroutines.TryGetValue(input, out Coroutine existingCoroutine))
        {
            if (existingCoroutine != null)
                StopCoroutine(existingCoroutine);
        }

        // 启动新的禁用协程
        var coroutine = StartCoroutine(DisableInputSeconds(input, time));
        _disableCoroutines[input] = coroutine;
    }

    private IEnumerator DisableInputSeconds(InputAction input, float time)
    {
        input.Disable();

        yield return new WaitForSeconds(time);

        input.Enable();
        _disableCoroutines.Remove(input);
    }

    public void DisableInputAction(InputAction input)
    {
        input.Disable();
    }

    public void RecoverInputAction(InputAction input)
    {
        input.Enable();
    }

    // 停止所有禁用协程的方法
    private void StopAllDisableCoroutines()
    {
        foreach (var pair in _disableCoroutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
                pair.Key.Enable(); // 确保重新启用所有被禁用的输入
            }
        }
        _disableCoroutines.Clear();
    }

    // 手动取消特定输入动作的禁用状态
    public void CancelDisableInput(InputAction input)
    {
        if (input == null || !_disableCoroutines.ContainsKey(input))
            return;

        if (_disableCoroutines[input] != null)
            StopCoroutine(_disableCoroutines[input]);

        input.Enable();
        _disableCoroutines.Remove(input);
    }
}