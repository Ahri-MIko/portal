using UnityEngine;

//��Ҫ����������Ĳ�ͬ˳�����Ⱦ,�ﵽ�����ŵ�Ч��
public class MainCamera : MonoBehaviour
{

    Portal[] portals;

    void Awake()
    {
        portals = FindObjectsOfType<Portal>();
    }

    void OnPreCull()
    {

        //����Ⱦ������Ƭ�������
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PrePortalRender();
        }

        //����Ⱦ������(����û��),ȷ������һ��֮��֤��Ⱦ��ȷ
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].Render();
        }
        //��Ⱦ��õ���ȷ���ŵĺ��,Ҳ��������һ���ĺ����Ա�֤���ճ��ֵ�Ч����ȷ
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PostPortalRender();
        }

    }

    private void Update()
    {

    }

}