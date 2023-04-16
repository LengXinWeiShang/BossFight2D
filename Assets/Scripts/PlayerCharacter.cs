using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    private PlayerController controller;

    private Rigidbody2D rigid;
    private Animator anim;

    private Transform checkGround;

    public float speed = 3;
    public float jumpSpeed = 8;
    public int maxHp = 5;

    private int hp;

    private bool jump = false;
    private bool isGround = true;
    private bool faceLeft = false;

    // 受击时的硬直时间（物理帧）
    private int outControlTime = 0;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        hp = maxHp;

        checkGround = transform.Find("CheckGround");
    }

    private void Update()
    {
        if (controller.jump)
        {
            // 缓存跳跃按键（避免时序问题）
            jump = true;
        }

        // 更新动画状态
        anim.SetBool("IsGround", isGround);
        anim.SetFloat("Speed", Mathf.Abs(controller.h));
        if (controller.attack)
        {
            anim.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 在动画状态机中检测玩家是否在攻击状态（是否正在播放攻击动画）
    /// </summary>
    /// <returns>是否在攻击中</returns>
    private bool IsAttacking()
    {
        AnimatorStateInfo asi = anim.GetCurrentAnimatorStateInfo(0);
        // 判断当前动画名称
        return asi.IsName("Attack1") || asi.IsName("Attack2") || asi.IsName("Attack3");
    }

    private void FixedUpdate()
    {
        // 检测是否在地面
        CheckGround();
        if (!IsAttacking() && outControlTime <= 0)
        {
            // 不在攻击和硬直中才能移动
            Move(controller.h);
        }

        // 执行fixedUpdate后还原跳跃按键
        jump = false;
        outControlTime--;
    }

    /// <summary>
    /// 控制角色移动
    /// </summary>
    /// <param name="h">水平输入</param>
    private void Move(float h)
    {
        Flip(h);
        float vy = rigid.velocity.y;
        if (jump && isGround)
        {
            // 更新动画状态
            anim.SetTrigger("Jump");
            vy = jumpSpeed;
        }
        rigid.velocity = new Vector2(h * speed, vy);
    }

    /// <summary>
    /// 检测角色是否在地面
    /// </summary>
    private void CheckGround()
    {
        // 打出一个球形射线进行碰撞检测
        isGround = Physics2D.OverlapCircle(checkGround.position, 0.1f, ~LayerMask.GetMask("Player"));
    }

    /// <summary>
    /// 控制角色转身
    /// </summary>
    /// <param name="h">水平输入</param>
    private void Flip(float h)
    {
        Vector3 scaleLeft = new Vector3(1, 1, 1);
        Vector3 scaleRight = new Vector3(-1, 1, 1);

        if (h > 0.1f)
        {
            // 向右
            faceLeft = false;
            transform.localScale = scaleRight;
        }
        else if (h < -0.1f)
        {
            // 向左
            faceLeft = true;
            transform.localScale = scaleLeft;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            if (isGround)
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawSphere(checkGround.position, 0.1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Boss") || collision.transform.CompareTag("BossAttack"))
        {
            GetHit(1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Boss") || collision.transform.CompareTag("BossAttack"))
        {
            GetHit(1);
        }
    }

    private void GetHit(int damage)
    {
        hp = Mathf.Clamp(hp - damage, 0, maxHp);

        // 更新血条UI
        UIManager.Instance.SetPlayerHp(hp, maxHp);

        // 设置动画
        anim.SetTrigger("GetHit");

        // 受伤时向反方向弹飞
        Vector2 force = new Vector2(faceLeft ? 3 : -3, 3);
        rigid.velocity = force;

        // 角色进入硬直
        outControlTime = 30;
    }
}