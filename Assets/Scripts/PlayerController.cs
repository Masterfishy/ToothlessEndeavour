using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    public float speed;

    private new Rigidbody2D rigidbody;

    private float horizontal;
    private float vertical;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Vector2 vel;
        vel.x = horizontal;
        vel.y = vertical;
        vel = vel.normalized * speed;

        rigidbody.velocity = vel;
    }
}
