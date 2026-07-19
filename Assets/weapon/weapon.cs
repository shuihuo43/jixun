using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10f; // 缓动速度
    [SerializeField] private Animator animator;

    #region 生命周期函数
    void Update()
    {
        AttackCheck();
        if (!GetAttackState())
            Utilties.FollowMouse(this.transform, rotateSpeed, -45);
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