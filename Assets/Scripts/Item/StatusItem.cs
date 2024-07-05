using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusItem : Item
{
    [SerializeField]
    float _pickupRange;
    void Start()
    {
        base.ItemInit();
        _pickupRange = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        base.Floating();
    }

    // ���� ������ ����
    void TakeStatusItem(Collider other)
    {
        Status status = other.GetComponent<Status>();
        if (base.itemType == Define.Item.Heart)
        {
            if ((int)status.Hp == (int)status.MaxHp)
                return;
            status.Heal();
        }
        else if (base.itemType == Define.Item.Energy)
        {
            if ((int)status.Sp == (int)status.MaxSp)
                return;
            status.SpUp();
        }
        Destroy(gameObject);
    }

    void PickUp(Collider other)
    {
        Debug.Log("������ ȹ��");
        TakeStatusItem(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("������ ����");
        PickUp(other);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _pickupRange);
    }
}
