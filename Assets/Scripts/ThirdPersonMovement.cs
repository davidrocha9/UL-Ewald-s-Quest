using System;
using System.Collections;


using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float speed = 6;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    private bool isGrounded = false;
    Vector3 velocity;
    Vector2 movementRcvd;

    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    // Player run & stamina variables
    private float runSpeed = 10;
    private float walkSpeed = 6;
    [SerializeField] private UnityEngine.UI.Slider staminaSlider;
    private bool isStaminaOut = false;
    Color staminaNoAvailableColor = new Color(0f/255f, 1704f/255f, 209f/255f);
    Color staminaAvailableColor = new Color(0f / 255f, 194f / 255f, 209f / 255f);

    // Animator variables
    private Animator anim;
    private string JUMP_ANIMATION = "is_jumping";

    // Object tags
    private string MESH_TAG = "Mesh";

    void Start()
    {
        // get animator from child
        anim = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        MovePlayer();

        // Movement Control
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) && isGrounded)
        {
            if (Input.GetKey(KeyCode.LeftShift) && GameManager.gameManager._playerStamina.Stamina > 0 && !isStaminaOut)
            {
                Run();
                UseStamina();
            } 
            else
            {
                Walk();
                if (isStaminaOut)
                {
                    FullWaitReloadStamina();
                }
                else
                {
                    ReloadStamina();
                }
            }
            
        }
        else
        {
            Idle();
            if (isStaminaOut)
            {
                FullWaitReloadStamina();
            }
            else
            {
                ReloadStamina();
            }
        }

        // if animation is not jumping, then set anim param to false
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            // check if animation is finished
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                anim.SetBool(JUMP_ANIMATION, false);
            }
        }
    }

    void MovePlayer(){

        Vector3 direction = new Vector3(movementRcvd.x, 0f, movementRcvd.y).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
            
        }

        
    }

    void OnMove(InputValue input){
        movementRcvd = input.Get<Vector2>();
    }

    IEnumerator OnJump(InputValue input){
        if (isGrounded)
        {
            // set is_jumping to true on animator
            anim.SetBool("is_jumping", true);

            yield return new WaitForSeconds(0.7f);

            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);

            isGrounded = false;
        }
    }

    // Movement Functions
    private void Idle()
    {
        anim.SetFloat("Speed", 0);
    }

    private void Walk()
    {
        speed = walkSpeed;
        anim.SetFloat("Speed", 0.5f);
    }

    private void Run()
    {
        speed = runSpeed;
        anim.SetFloat("Speed", 1);
    }

    // Stamina Functions
    private void ReloadStamina()
    {
        if (GameManager.gameManager._playerStamina.Stamina < GameManager.gameManager._playerStamina.MaxStamina)
        {
            GameManager.gameManager._playerStamina.Stamina += GameManager.gameManager._playerStamina.ReloadAmount * Time.deltaTime;
            staminaSlider.value = GameManager.gameManager._playerStamina.Stamina / 100.0f;
        }
    }

    private void FullWaitReloadStamina()
    {
        GameManager.gameManager._playerStamina.Stamina += GameManager.gameManager._playerStamina.ReloadAmount * Time.deltaTime;
        staminaSlider.value = GameManager.gameManager._playerStamina.Stamina / 100.0f;
        staminaSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = staminaNoAvailableColor;
        if (GameManager.gameManager._playerStamina.Stamina >= 25)
        {
            isStaminaOut = false;
            staminaSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<UnityEngine.UI.Image>().color = staminaAvailableColor;
        }
    }
    private void UseStamina()
    {
        GameManager.gameManager._playerStamina.Stamina -= GameManager.gameManager._playerStamina.UseAmount * Time.deltaTime;
        staminaSlider.value = GameManager.gameManager._playerStamina.Stamina / 100.0f;

        if (GameManager.gameManager._playerStamina.Stamina <= 0)
        {
            isStaminaOut = true;
        }
    }

    // Collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(MESH_TAG))
        {
            isGrounded = true;
        }
    }

}