using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f; // 缓动速度
    [SerializeField] private Animator animator;

    #region 生命周期函数
    void Update()
    {
        AttackCheck();
        FollowMouse();
    }

    #endregion


    #region 攻击

    private void AttackCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AttackLogic();
        }
    }

    private void AttackLogic()
    {
        if(!GetAttackState())
        {
            animator.SetTrigger("Attack");
            animator.SetBool("IsAttack", true);
        }
    }

    #endregion


    #region 方法函数
    private void FollowMouse()
    {
        if (GetAttackState())
            return;


        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = mousePos - transform.position;

        // 鼠标方向角度
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 剑默认朝右上，修正45度
        targetAngle -= 45f;

        // 当前角度平滑过渡到目标角度
        float currentAngle = transform.eulerAngles.z;

        float newAngle = Mathf.LerpAngle(
            currentAngle,
            targetAngle,
            rotateSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }


    private void AttackEnd()
    {
        animator.SetBool("IsAttack", false);
        print("attack_over");
    }


    private bool GetAttackState()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
    }

    #endregion
}