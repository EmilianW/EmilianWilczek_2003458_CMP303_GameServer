// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float health;
    public float maxHealth = 100f;

    private bool[] _inputs;
    private float _yVelocity;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void FixedUpdate()
    {
        if (health <= 0f) return;

        var inputDirection = Vector2.zero;
        if (_inputs[0]) inputDirection.y += 1;
        if (_inputs[1]) inputDirection.y -= 1;
        if (_inputs[2]) inputDirection.x -= 1;
        if (_inputs[3]) inputDirection.x += 1;

        Move(inputDirection);
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        _inputs = new bool[5];
    }

    private void Move(Vector2 inputDirection)
    {
        var _transform = transform;
        var moveDirection = _transform.right * inputDirection.x + _transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            _yVelocity = 0f;
            if (_inputs[4]) _yVelocity = jumpSpeed;
        }

        _yVelocity += gravity;

        moveDirection.y = _yVelocity;
        controller.Move(moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] inputs, Quaternion rotation)
    {
        _inputs = inputs;
        transform.rotation = rotation;
    }

    public void Shoot(Vector3 viewDirection)
    {
        if (!Physics.Raycast(shootOrigin.position, viewDirection, out var out_hit, 25f)) return;
        if (out_hit.collider.CompareTag("Player")) out_hit.collider.GetComponent<Player>().TakeDamage(50f);
    }

    private void TakeDamage(float damage)
    {
        if (health <= 0f) return;

        health -= damage;
        if (health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }
}