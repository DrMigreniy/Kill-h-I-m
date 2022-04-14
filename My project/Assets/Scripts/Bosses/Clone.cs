using System;
using UnityEngine;

public class Clone : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private int MaxHealthPoints = 100;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float agresDistance;
    private int currentHealthPoints;
    private Animator anim;
    private Rigidbody2D rb;
    private new BoxCollider2D collider;
    public Transform rangePoint;

    public AudioClip attackSound;
    public AudioClip blockSound;
    public AudioClip swingSound;
    public AudioSource audioSource;

    public Transform patrolPoint;
    public float attackRange = 0.5f;
    public LayerMask heroLayer;
    public Transform player;
    private float jumpCoolDown = 1f;
    private float attackCoolDown = 0;
    private float hurtCoolDown = 0;
    private SpriteRenderer sprite;

    bool isDead = false;
    bool isGrounded = false;

   
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
        currentHealthPoints = MaxHealthPoints;

        Physics2D.IgnoreLayerCollision(3, 3);
    }

    
    void Update()
    {
        if (transform.position.y < -5)
        {
            transform.position = patrolPoint.position;
        }
        if (!isDead)
        {
            if (attackCoolDown <= 0 && hurtCoolDown <= 0)
            {
                if (transform.position.x - player.position.x <= 1 && transform.position.x - player.position.x >= -1)
                {
                    attackCoolDown = 0.6f;
                    attack();
                }
            }
            if (Vector2.Distance(transform.position, player.position) > agresDistance && transform.position.x == patrolPoint.position.x && isGrounded)
                idle();
            else if (Vector2.Distance(transform.position, player.position) < agresDistance)
                agressive();
            else if (Vector2.Distance(transform.position, player.position) > agresDistance)
                goBack();
        }
    }


    public void takeDamage(int damage)
    {
        if (!isDead)
        {
            hurtCoolDown = 0.6f;
            anim.SetTrigger("Hurt");
            currentHealthPoints -= damage;
        }

        if (currentHealthPoints <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        anim.SetTrigger("Death");
        isDead = true;
        collider.enabled = false;
        rb.gravityScale = 0;
    }


    private void agressive()
    {
        if (player.position.x < transform.position.x)
            sprite.flipX = false;
        else
            sprite.flipX = true;

        if (player.position.x < transform.position.x && !sprite.flipX)
            rangePoint.position = Vector3.MoveTowards(rangePoint.position, new Vector3(transform.position.x - 1, rangePoint.position.y), 100000 * Time.deltaTime);
        if (player.position.x > transform.position.x && sprite.flipX)
            rangePoint.position = Vector3.MoveTowards(rangePoint.position, new Vector3(transform.position.x + 1, rangePoint.position.y), 100000 * Time.deltaTime);


        anim.SetInteger("AnimState", 2);

        if (attackCoolDown <= 0 && hurtCoolDown <= 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            if (player.position.y > transform.position.y)
            {
                Jump();
            }
        }
    }

    private void goBack()
    {
        if (patrolPoint.position.x < transform.position.x)
            sprite.flipX = false;
        else
            sprite.flipX = true;

        anim.SetInteger("AnimState", 2);
        
        transform.position = Vector2.MoveTowards(transform.position, patrolPoint.position, speed * Time.deltaTime);
    }

    private void idle()
    {
        anim.SetInteger("AnimState", 0);
    }

    private void Jump()
    {
        if (isGrounded && jumpCoolDown <= 0)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
            anim.SetBool("Grounded", false);
            jumpCoolDown = 1;
        }
    }


    private void CheckGround()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        isGrounded = collider.Length > 1;
        if (isGrounded)
            anim.SetBool("Grounded", true);
        else 
            anim.SetBool("Grounded", false);
    }


    private void FixedUpdate()
    {
        CheckGround();
        if (jumpCoolDown > 0)
            jumpCoolDown -= Time.deltaTime;
        if (attackCoolDown > 0)
            attackCoolDown -= Time.deltaTime;
        if (hurtCoolDown > 0)
            hurtCoolDown -= Time.deltaTime;
    }


    private void attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(rangePoint.position, attackRange, heroLayer);
        
        anim.SetTrigger("Attack");

        audioSource.clip = swingSound;

        foreach (Collider2D hero in hitEnemies)
        {
            if (!hero.GetComponent<Hero>().blocked() && !hero.GetComponent<Hero>().roll())
            {
                hero.GetComponent<Hero>().takeDamage(attackDamage);
                audioSource.clip = attackSound;

            }
            else if(hero.GetComponent<Hero>().blocked())
            {
                hero.GetComponent<Hero>().takeDamage(Convert.ToInt32(attackDamage * 0.7));
                audioSource.clip = blockSound;
            }
        }
        audioSource.Play();
    }

    public bool getState()
    {
        return isDead;
    }
}
