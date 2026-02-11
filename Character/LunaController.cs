using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunaController : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;
    public float moveSpeed;
    private Animator animator;
    private Vector2 lookDirection = new Vector2(1,0);
    private float moveScale;
    private Vector2 move;
    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        Debug.Log("LunaController Start() called, " );
    }
    void Update() 
    {
        if (!GameManager.Instance.canControlShenYan)
        {
            return;
        }
        float horizontal = Input.GetAxisRaw("Horizontal");        //获取玩家水平轴向输入值
        float vertical = Input.GetAxisRaw("Vertical");        //获取玩家垂直轴向输入值
        move = new Vector2(horizontal,vertical);
        //当前玩家输入的某个轴向不为0
        if (!Mathf.Approximately(move.x,0)|| !Mathf.Approximately(move.y, 0))
        {
            lookDirection.Set(move.x,move.y);
            //lookDirection = move;
            lookDirection.Normalize();
            //animator.SetFloat("MoveValue", 1);
        }
        //动画的控制
        animator.SetFloat("Look X",lookDirection.x);
        animator.SetFloat("Look Y",lookDirection.y);
        moveScale = move.magnitude;
        if (move.magnitude>0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveScale = 2;
                moveSpeed = 3.5f;
            }
            else
            {
                moveScale = 1;
                moveSpeed = 2;
            }
        }
        animator.SetFloat("MoveValue", moveScale);
    }

    private void FixedUpdate()
    {
        // 添加控制状态检查
        if (!GameManager.Instance.canControlShenYan)
        {
            // 当不能控制时，将移动向量清零
            move = Vector2.zero;
            animator.SetFloat("MoveValue", 0); // 停止移动动画
            return;
        }

        Vector2 position = transform.position;
        position = position + moveSpeed * move * Time.fixedDeltaTime;
        rigidbody2d.MovePosition(position);
    }
    public void StopMovement()
    {
        move = Vector2.zero;
        animator.SetFloat("MoveValue", 0);
    }

    public void Climb(bool start)
    {
        animator.SetBool("Climb",start);
    }

    public void Jump(bool start)
    {
        animator.SetBool("Jump",start);
        rigidbody2d.simulated = !start;
    }
    private void PetTheDog()
    {
        animator.CrossFade("PetTheDog", 0);
        transform.position = new Vector3(-1.19f, -7.83f, 0);
    }
}
