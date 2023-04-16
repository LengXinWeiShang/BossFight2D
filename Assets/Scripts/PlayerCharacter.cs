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

    // �ܻ�ʱ��Ӳֱʱ�䣨����֡��
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
            // ������Ծ����������ʱ�����⣩
            jump = true;
        }

        // ���¶���״̬
        anim.SetBool("IsGround", isGround);
        anim.SetFloat("Speed", Mathf.Abs(controller.h));
        if (controller.attack)
        {
            anim.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// �ڶ���״̬���м������Ƿ��ڹ���״̬���Ƿ����ڲ��Ź���������
    /// </summary>
    /// <returns>�Ƿ��ڹ�����</returns>
    private bool IsAttacking()
    {
        AnimatorStateInfo asi = anim.GetCurrentAnimatorStateInfo(0);
        // �жϵ�ǰ��������
        return asi.IsName("Attack1") || asi.IsName("Attack2") || asi.IsName("Attack3");
    }

    private void FixedUpdate()
    {
        // ����Ƿ��ڵ���
        CheckGround();
        if (!IsAttacking() && outControlTime <= 0)
        {
            // ���ڹ�����Ӳֱ�в����ƶ�
            Move(controller.h);
        }

        // ִ��fixedUpdate��ԭ��Ծ����
        jump = false;
        outControlTime--;
    }

    /// <summary>
    /// ���ƽ�ɫ�ƶ�
    /// </summary>
    /// <param name="h">ˮƽ����</param>
    private void Move(float h)
    {
        Flip(h);
        float vy = rigid.velocity.y;
        if (jump && isGround)
        {
            // ���¶���״̬
            anim.SetTrigger("Jump");
            vy = jumpSpeed;
        }
        rigid.velocity = new Vector2(h * speed, vy);
    }

    /// <summary>
    /// ����ɫ�Ƿ��ڵ���
    /// </summary>
    private void CheckGround()
    {
        // ���һ���������߽�����ײ���
        isGround = Physics2D.OverlapCircle(checkGround.position, 0.1f, ~LayerMask.GetMask("Player"));
    }

    /// <summary>
    /// ���ƽ�ɫת��
    /// </summary>
    /// <param name="h">ˮƽ����</param>
    private void Flip(float h)
    {
        Vector3 scaleLeft = new Vector3(1, 1, 1);
        Vector3 scaleRight = new Vector3(-1, 1, 1);

        if (h > 0.1f)
        {
            // ����
            faceLeft = false;
            transform.localScale = scaleRight;
        }
        else if (h < -0.1f)
        {
            // ����
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

        // ����Ѫ��UI
        UIManager.Instance.SetPlayerHp(hp, maxHp);

        // ���ö���
        anim.SetTrigger("GetHit");

        // ����ʱ�򷴷��򵯷�
        Vector2 force = new Vector2(faceLeft ? 3 : -3, 3);
        rigid.velocity = force;

        // ��ɫ����Ӳֱ
        outControlTime = 30;
    }
}