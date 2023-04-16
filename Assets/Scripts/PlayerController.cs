using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerCharacter player;

    [HideInInspector]
    public float h, v;

    public bool jump;

    public bool attack;

    private void Start()
    {
        player = GetComponent<PlayerCharacter>();
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        jump = Input.GetButtonDown("Jump");
        attack = Input.GetKeyDown(KeyCode.J);
    }
}