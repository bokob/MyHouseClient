using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ٰŸ�, ���Ÿ� ������� ��ӹ޴� �Ϲ�ȭ�� ���� Ŭ����
/// </summary>
public class Weapon : MonoBehaviourPunCallbacks
{
    public Define.Type Type { get; set; } // ���� Ÿ��

    public Transform Master { get; set; } // ����

    public int Attack { get; set; }       // ���ݷ�
    public float Rate { get; set; } = 0.5f;      // ���ݼӵ�
    public float Range { get; set; } = 5f;


    void Awake()
    {
        RecordMaster();
        // TODO
        /*
         ���Ⱑ �پ����� �� ���� �̸��̳� Ÿ�Կ� ����
         �������� ���ݼӵ��� �����ϴ� �۾��� ����� ��
         */
    }

    /// <summary>
    /// Use() �����ϸ鼭 �� ���⿡ �´� ���� ȿ�� �ڷ�ƾ�� ���� ����ȴ�.
    /// </summary>
    public virtual void Use()
    {
        // TODO
        // ���⿡ �´� ���� ���
    }

    /// <summary>
    /// �ֻ��� �θ� �������� ����ϴ� �޼���
    /// </summary>
    public void RecordMaster()
    {
        // �ֻ��� �θ� Master�� ����
        Master = transform.root.GetChild(2);
        Debug.Log("���� ����: " + Master.name);
    }
}