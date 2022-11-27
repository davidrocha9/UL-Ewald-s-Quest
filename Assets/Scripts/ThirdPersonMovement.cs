using System.Collections;


using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float speed = 6;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    Vector3 velocity;
    Vector2 movementRcvd;
    bool isGrounded;

    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    // sender
    public delegate void PlayerWalking(bool isWalking);
    public static event PlayerWalking PlayerActionInfo;
    private Animator anim;

    void Start()
    {
        // get animator from child
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= 1.6 && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void FixedUpdate(){
        bool isWalking = MovePlayer();
        if (PlayerActionInfo != null)
        {
            PlayerActionInfo(isWalking);
        }
    }

    bool MovePlayer(){
        bool isWalking = false;
        Vector3 direction = new Vector3(movementRcvd.x, 0f, movementRcvd.y).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);

            
            isWalking = true;

        }

        return isWalking;
    }

    void OnMove(InputValue input){
        movementRcvd = input.Get<Vector2>();
    }

    void OnJump(InputValue input){
        if (transform.position.y <= 1.6f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);

            // set is_jumping to true on animator
            anim.SetBool("is_jumping", true);
        }
    }

}