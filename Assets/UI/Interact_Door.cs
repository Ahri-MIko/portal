using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interact_Door : MonoBehaviour
{
    [SerializeField] private GameObject door;//��
    [SerializeField] GameObject OpenDoorUI;//������ʾ
    private Rigidbody rb;
    Keyboard keyBoard;
    public bool isOpen = false;//���Ƿ��
    private Vector3 DoorClosedPos;   // �ų�ʼλ��
    public float doorOpenDistance = 3f; // �ſ���ʱ�ƶ��ľ���
    public float doorSpeed = 10f;        // ���ƶ����ٶ�
    public Vector3 doorDirection = Vector3.back;  // �ŵĿ��ط���Ĭ��������
    public PlayerInput playerInput;
    public bool PlayerIn = false;
    

    void Start()
    {
        //playerInput.Player.Interact.performed += ctx => Interact();
        DoorClosedPos = door.transform.position;
        rb= door.GetComponent<Rigidbody>();
        // ȷ�� Rigidbody û�б�����Ϊ isKinematic
        rb.isKinematic = false;

        // ȷ������û�б������ƶ�
        rb.constraints = RigidbodyConstraints.None; // û���κ��������

        // ���� Rigidbody Ϊ Kinematic����������Ӱ��
        rb.isKinematic = true;

        // ��ѡ���ر�����Ӱ��
        rb.useGravity = false;
    }

    void Update()
    {
        float doorLength = 0f;

        if (doorDirection == Vector3.right || doorDirection == Vector3.left)
        {
            doorLength = door.transform.localScale.x;  // ��ȡ���� X ���ϵĳ���
        }
        else if (doorDirection == Vector3.up || doorDirection == Vector3.down)
        {
            doorLength = door.transform.localScale.y;  // ��ȡ���� Y ���ϵĳ���
        }
        else if (doorDirection == Vector3.forward || doorDirection == Vector3.back)
        {
            doorLength = door.transform.localScale.z;  // ��ȡ���� Z ���ϵĳ���
        }
        if (isOpen)
        {
            // �����Ű��� `doorDirection` �ķ����ƶ�
            Vector3 DoorTargetPos = DoorClosedPos - doorDirection * doorOpenDistance;
            // ʹ�� MovePosition ��ƽ�����ƶ���
            MoveDoor(rb, DoorTargetPos, doorLength);
        }
        else if (!isOpen)
        {
            MoveDoor(rb, DoorClosedPos, doorLength);
        }

        if(Input.GetKeyDown(KeyCode.E) && PlayerIn)
        {
            //�ı��ŵ�״̬
            isOpen = !isOpen;

            OpenDoorUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)//����ҽ��봥������Χ��
    {
     
        //����������ʾ
        if (other.CompareTag("Player"))
        {
            PlayerIn = true;
            OpenDoorUI.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)//������뿪��������Χ
    {
       
        //�رս�����ʾ
        if (other.CompareTag("Player"))
        {
            PlayerIn = false;
            OpenDoorUI.SetActive(false);
        }
    }

    // ʹ�� Rigidbody MovePosition ��ƽ���ƶ���
    private void MoveDoor(Rigidbody doorRb, Vector3 targetPosition, float doorLength)
    {
        // �����ƶ�����
        Vector3 moveDirection = targetPosition - doorRb.position;

        // �����û�е���Ŀ��λ��
        if (moveDirection.magnitude > doorLength * 0.1f)
        {
            Vector3 newPosition = doorRb.position + moveDirection.normalized * doorSpeed * Time.deltaTime;
            doorRb.MovePosition(newPosition);
        }
        else
        {
            // ���Ѿ�����Ŀ��λ�ã�ֹͣ�ƶ�
            doorRb.MovePosition(targetPosition);
        }
    }
}


