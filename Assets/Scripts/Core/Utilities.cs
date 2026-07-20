using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilties : MonoBehaviour
{
    public static void FollowMouse(Transform _transform, float rotateSpeed, float offsetAngle)
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = mousePos - _transform.position;

        // 鼠标方向角度
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 剑默认朝右上，修正45度
        targetAngle += offsetAngle;

        // 当前角度平滑过渡到目标角度
        float currentAngle = _transform.eulerAngles.z;

        float newAngle = Mathf.LerpAngle(
            currentAngle,
            targetAngle,
            rotateSpeed * Time.deltaTime
        );

        _transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
}
