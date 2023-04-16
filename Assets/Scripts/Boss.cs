using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Idle,               // 静止
    Run,                // 冲刺
    Skill_FireBall,     // 火球
    Skill_FireRain,     // 火雨
}

public class Boss : MonoBehaviour
{
    public int maxHp = 100;
    public float speed = 8;
    public FireBall prefabFireBall;
    public FireRain prefabFireRain;

    private int hp;
    private BossState state;
    private float lastChangeStateTime;
    private bool faceRight = false;
    private bool isRunning = false;
    private Transform firePoint;

    private Animator anim;
    private Rigidbody2D rigid;

    private void Start()
    {
        hp = maxHp;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        firePoint = transform.Find("FirePoint");

        state = BossState.Idle;
        lastChangeStateTime = Time.time;
    }

    private void Update()
    {
        switch (state)
        {
            case BossState.Idle:
                {
                    // 静止3秒后切换状态
                    if (Time.time - lastChangeStateTime > 3)
                    {
                        // 随机切换不同的状态
                        int r = Random.Range(1, 4);
                        switch (r)
                        {
                            case 1:
                                state = BossState.Run;
                                break;

                            case 2:
                                state = BossState.Skill_FireBall;
                                // 开启吐火球协程
                                StartCoroutine(CoFireBallState());
                                break;

                            case 3:
                                state = BossState.Skill_FireRain;
                                // 开启火雨协程
                                StartCoroutine(CoFireRainState());
                                break;

                            default:
                                break;
                        }

                        lastChangeStateTime = Time.time;
                    }
                    // 速度归零
                    rigid.velocity = new Vector2(0, rigid.velocity.y);
                    // 更新动画状态
                    isRunning = false;
                    anim.SetBool("IsRunning", isRunning);
                }
                break;

            case BossState.Run:
                {
                    // 冲刺到头，转身，转为Idle状态
                    if (faceRight && transform.position.x >= 8
                        || !faceRight && transform.position.x <= -8)
                    {
                        Flip();
                        state = BossState.Idle;
                        lastChangeStateTime = Time.time;
                        break;
                    }

                    // 冲刺逻辑
                    Vector2 move = new Vector2(speed, rigid.velocity.y);
                    rigid.velocity = faceRight ? move : move * -1;
                    // 更新动画状态机
                    isRunning = true;
                    anim.SetBool("IsRunning", isRunning);
                }
                break;

            case BossState.Skill_FireBall:

                break;

            case BossState.Skill_FireRain:
                break;

            default:
                break;
        }
    }

    private IEnumerator CoFireBallState()
    {
        for (int i = 0; i < 3; ++i)
        {
            // 吐火球
            anim.SetTrigger("Attack");

            yield return new WaitForSeconds(1.5f);
        }
        state = BossState.Idle;
        lastChangeStateTime = Time.time;
    }

    private IEnumerator CoFireRainState()
    {
        for (int i = 0; i < 2; ++i)
        {
            // 下火雨
            anim.SetTrigger("Attack");

            yield return new WaitForSeconds(1.5f);
        }
        state = BossState.Idle;
        lastChangeStateTime = Time.time;
    }

    public void Attack()
    {
        if (state == BossState.Skill_FireBall)
        {
            FireBall ball = Instantiate(prefabFireBall, firePoint.position, Quaternion.identity);
            if (faceRight)
            {
                ball.transform.right = Vector3.left;
            }
        }

        if (state == BossState.Skill_FireRain)
        {
            for (int j = 0; j < 5; ++j)
            {
                float r = Random.Range(-7f, 7f);
                FireRain fireRain = Instantiate(prefabFireRain, new Vector3(r, 4, 0), Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// BOSS受伤逻辑
    /// </summary>
    public void GetHit(int damage)
    {
        hp = Mathf.Clamp(hp - damage, 0, maxHp);
        // 更新血条UI
        UIManager.Instance.SetBossHp(hp, maxHp);

        anim.SetTrigger("GetHit");

        if (hp == 0)
        {
            anim.SetTrigger("Die");
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var c in colliders)
            {
                c.enabled = false;
            }
            rigid.isKinematic = true;
        }
    }

    private void Flip()
    {
        faceRight = !faceRight;

        Vector3 scaleRight = new Vector3(1, 1, 1);
        Vector3 scaleLeft = new Vector3(-1, 1, 1);

        transform.localScale = faceRight ? scaleRight : scaleLeft;
    }
}