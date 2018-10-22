﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region ShowInEditor
    [SerializeField] PlayerControlData data;
    [SerializeField] Transform grip;
    [SerializeField] Transform weaponTransform;
    [SerializeField] JetPack jetPack;
    //[SerializeField] Animator animator;
    #endregion
    #region HideInEditor
    Rigidbody2D self;
    SpriteRenderer sRenderer;
    Weapon weapon;
    
    bool isGrounded;
    int jumpCounter = 0;

    float currentMaxSpeed;
    float currentMoveForce;

    float teleportTimer;

    [HideInInspector] public bool weaponIsAuto;
    [HideInInspector] public bool canAim = true;
    [HideInInspector] public Vector2 target;
    #endregion

    #region UnityFunctions
    void Awake ()
    {
        self = GetComponent<Rigidbody2D>();
        sRenderer = GetComponent<SpriteRenderer>();
        self.sharedMaterial = data.material;
        weapon = weaponTransform.GetComponent<Weapon>();
    }
	void Update ()
    {
        Move();
        WeaponHandling();
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "DroppedWeapon")
        {
            PickUpWeapon(col.gameObject.GetComponent<DroppedWeapon>().data);
            Destroy(col.gameObject);
        }
    }
    #endregion
    #region Movement Functions
    void Move()
    {
        Grounded();
        Sprint();
        HorizontalMovement();
        Jump();
        JetPack();
        Teleport();
        //animator.SetFloat("speed", Mathf.Abs(self.velocity.x));
        //animator.SetBool("isGrounded", isGrounded);
    }

    void Grounded()
    {
        Vector2 pointA = new Vector2(grip.position.x - data.gripWidth, grip.position.y - data.gripWidth);
        Vector2 pointB = new Vector2(grip.position.x + data.gripWidth, grip.position.y + data.gripWidth);

        if (Physics2D.OverlapArea(pointA, pointB ,data.gripLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void HorizontalMovement()
    {
        if (isGrounded)
        {
            currentMoveForce = data.moveForce;
        }
        else
        {
            currentMoveForce = data.moveForce * data.airControlMultiplier;
        }

        if (Input.GetButton("Left"))
        {
            if (self.velocity.x > -currentMaxSpeed)
            {
                self.AddForce(Vector2.left * currentMoveForce);
            }
            else
            {
                self.velocity = new Vector2(-currentMaxSpeed, self.velocity.y);
            }
        }
        else if (Input.GetButton("Right"))
        {
            if (self.velocity.x < currentMaxSpeed)
            {
                self.AddForce(Vector2.right * currentMoveForce);
            }
            else
            {
                self.velocity = new Vector2(currentMaxSpeed, self.velocity.y);
            }
        }
        else if(isGrounded == true)
        {
            self.velocity = new Vector2( Mathf.Lerp(self.velocity.x, 0f, data.stoppingRate) , self.velocity.y);
        }
    }
    void Sprint()
    {
        if (Input.GetButton("Sprint") && isGrounded)
        {
            currentMaxSpeed = data.sprintMaxSpeed;
        }
        else
        {
            currentMaxSpeed = data.runMaxSpeed;
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                self.velocity = new Vector2(self.velocity.x, data.jumpForce);
                jumpCounter = 0;
            }
            else if (jumpCounter < data.multiJumps)
            {
                self.velocity = new Vector2 (self.velocity.x, data.jumpForce);
                jumpCounter++;
            }
        }
    }
    void JetPack()
    {
        if (Input.GetButton("JumpJet"))
        {
            jetPack.Use();
        }
    }
    void Teleport()
    {
        if (Input.GetButtonDown("Teleport"))
        {

            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(target, transform.position) < data.teleportRange && teleportTimer + data.teleportCoolDown < Time.time)
            {
                Vector2 colliderSize = GetComponent<Collider2D>().bounds.size;

                Vector2 pointA = new Vector2(-colliderSize.x / 2, -colliderSize.y / 2) + target;
                Vector2 pointB = new Vector2(colliderSize.x / 2, colliderSize.y / 2) + target;


                if (Physics2D.OverlapArea(pointA, pointB, data.teleportMask) == null)
                {
                    transform.position = target;
                    teleportTimer = Time.time;
                    if(data.loseMomentumOnTeleport)
                    {
                        self.velocity = Vector2.zero;
                    }
                }                
            }
        }
    }
    #endregion
    #region Weapon Functions
    void WeaponHandling()
    {
        AimWeapon();
        CheckDirection();
        Attack();
        Reload();
        ChangeWeapon();
        DropWeapon();
    }
    void AimWeapon()
    {
        if (canAim)
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            weaponTransform.right = target - (Vector2)weaponTransform.position;
        }
    }
    void Attack()
    {
        if (weapon.IsAuto && Input.GetButton("Attack"))
        {
            weapon.Attack();
        }
        else if(weapon.IsAuto == false && Input.GetButtonDown("Attack"))
        {
            weapon.Attack();
        }
    }
    void Reload()
    {
        if (Input.GetButtonDown("Reload"))
        {
            weapon.Reload();
        }
    }

    void CheckDirection()
    {
        if(target.x > transform.position.x)
        {
            sRenderer.flipX = false;
            weapon.SetApearence(true);
        }
        else
        {
            sRenderer.flipX = true;
            weapon.SetApearence(false);
        }
    }

    void ChangeWeapon()
    {
        if (Input.GetButtonDown("NextWeapon"))
        {
            weapon.ChangeWeapon(true);
        }
        if (Input.GetButtonDown("PreviousWeapon"))
        {
            weapon.ChangeWeapon(false);
        }
    }
    void DropWeapon()
    {
        if (Input.GetButtonDown("DropWeapon"))
        {
            weapon.DropWeapon();
        }
    }
    void PickUpWeapon(WeaponData weaponData)
    {
        weapon.PickUpWeapon(weaponData);
    }
    #endregion
}
