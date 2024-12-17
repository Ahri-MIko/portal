using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/*
 * ʹ�÷���
DisableInputForSeconds(PlayerinputActions.Jump, 2f);
// �����Ҫ��ǰȡ������
CancelDisableInput(PlayerinputActions.Jump);
 */
public class MoveInputs : MonoBehaviour
{
    public PlayerInputsActions PlayerInputMaps { get; private set; }
    public PlayerInputsActions.PlayerActions PlayerinputActions { get; private set; }

    // ����׷�����ڽ��õ����붯��
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
        // ȷ�����������������е�Э��
        StopAllDisableCoroutines();
    }

    public void DisableInputForSeconds(InputAction input, float time)
    {
        if (input == null)
            return;

        // ��������붯���Ѿ��ڽ���״̬����ֹ֮ͣǰ��Э��
        if (_disableCoroutines.TryGetValue(input, out Coroutine existingCoroutine))
        {
            if (existingCoroutine != null)
                StopCoroutine(existingCoroutine);
        }

        // �����µĽ���Э��
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

    // ֹͣ���н���Э�̵ķ���
    private void StopAllDisableCoroutines()
    {
        foreach (var pair in _disableCoroutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
                pair.Key.Enable(); // ȷ�������������б����õ�����
            }
        }
        _disableCoroutines.Clear();
    }

    // �ֶ�ȡ���ض����붯���Ľ���״̬
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