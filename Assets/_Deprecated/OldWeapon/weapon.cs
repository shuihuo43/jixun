using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f; // �����ٶ�
    [SerializeField] private Animator animator;

    #region �������ں���
    void Update()
    {
        AttackCheck();
        //print(GetAttackState());
        if (!GetAttackState())
            Utilties.FollowMouse(this.transform, rotateSpeed, 0);
        else
            print("attacking");
    }

    #endregion


    #region ����

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
        }
    }

    #endregion


    #region ��������

    private void AttackStart()
    {
        animator.SetBool("IsAttack", true);
        SnapToMouseDirection();
        print("attack_start");
        //print(GetAttackState());
    }

    /// <summary>
    /// 立即将朝向定到鼠标方向（仅一次，不平滑过渡）
    /// </summary>
    private void SnapToMouseDirection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }

    private void AttackEnd()
    {
        animator.SetBool("IsAttack", false);
        //print("attack_over");
    }


    private bool GetAttackState()
    {
        return animator.GetBool("IsAttack");
    }

    #endregion
}