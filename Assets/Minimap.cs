using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Camera minimapCamera;
    private int secondFloorLayer;

    void Start()
    {
        minimapCamera = GetComponent<Camera>();

        secondFloorLayer = LayerMask.NameToLayer("SecondFloor");
    }

    void Update()
    {
        if (player.transform.position.y > 10)
        {
            minimapCamera.cullingMask |= (1 << secondFloorLayer);
        }
        else
        {
            minimapCamera.cullingMask &= ~(1 << secondFloorLayer);
        }
    }
}
