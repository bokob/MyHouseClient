using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Monster))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        Monster fov = (Monster)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov._radius);

        // 시야각의 가운데 직선 기준으로 왼쪽, 오른쪽 부분
        Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov._angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, fov._angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov._radius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov._radius);

        if (fov._canSeePlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(fov.transform.position, fov._target.position);
        }
    }

    Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
