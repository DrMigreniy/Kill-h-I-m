using System;
using UnityEngine;

public class Previous : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private int MaxHealthPoints = 1000;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private int spellDamage = 35;
    [SerializeField] private float agresDistance;
    private int attackCount = 0;

    private int currentHealthPoints;
    private Animator anim;
    private Rigidbody2D rb;
    private new BoxCollider2D collider;
    public Transform rangePoint;

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

    public GameObject spell;
    public GameObject previousClone;

    private float spellCoolDown = 0;
    private float cloneCoolDown = 0;

    public AudioClip[] attackSounds;
    public AudioClip swingSound;
    public AudioClip spellSound;
    public AudioClip blockSound;
    public AudioSource audioSource;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
        currentHealthPoints = MaxHealthPoints;

        Physics2D.IgnoreLayerCollision(3, 3);

        Physics2D.IgnoreLayerCollision(3, 8);
    }


    void Update()
    {
        if (transform.position.y < -5)
        {
            transform.position = patrolPoint.position;
        }

        if (!isDead)
        {
            if (hurtCoolDown <= 0 && spellCoolDown <= 0 && isGrounded && attackCount == 0)
            {
                spellCast();
                spellCoolDown = 3f;
            }
            else if (hurtCoolDown <= 0 && cloneCoolDown <= 0 && isGrounded && attackCount == 0)
            {
                clone();
                cloneCoolDown = 3f;
            }
            else if(attackCoolDown <= 0 && hurtCoolDown <= 0)
            {
                if (transform.position.x - player.position.x <= 3 && transform.position.x - player.position.x >= -3)
                {
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
            if (currentHealthPoints > 400)
                hurtCoolDown = 0.8f;
            else
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
            sprite.flipX = true;
        else
            sprite.flipX = false;

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
        if (isGrounded && jumpCoolDown <= 0 && !isDead)
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


        if (spellCoolDown > 0)
        {
            spellCoolDown -= Time.deltaTime;
            if (spellCoolDown < 1.4 && spellCoolDown > 1.4 - Time.deltaTime)
            {
                spell.transform.position = new Vector3(player.position.x, player.position.y + 2, player.position.z);
                spell.GetComponent<SpellAnim>().cast();

                audioSource.clip = spellSound;
                audioSource.Play();
            }
            if (spellCoolDown < 1.4f && spellCoolDown > 0.4)
            {
                spell.transform.position = Vector3.MoveTowards(new Vector3(player.position.x, player.position.y + 2, player.position.z), spell.transform.position, (speed - 3) * Time.deltaTime);
            }
            if (spellCoolDown < 0.4f && spellCoolDown > 0.4 - Time.deltaTime)
            {

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(spell.transform.position, attackRange, heroLayer);

                foreach (Collider2D hero in hitEnemies)
                {
                    if (!hero.GetComponent<Hero>().roll())
                        hero.GetComponent<Hero>().takeDamage(spellDamage);
                }
            }
            if (spellCoolDown < 0.2f && spellCoolDown > 0.2 - Time.deltaTime)
                spell.SetActive(false);
        }


        if (cloneCoolDown > 0)
        {
            cloneCoolDown -= Time.deltaTime;
            if (cloneCoolDown < 1.8 && cloneCoolDown > 1.8 - Time.deltaTime)
                previousClone.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            if (cloneCoolDown <= 1.8f && cloneCoolDown >= 0.2)
                previousClone.GetComponent<HeroClone>().attack();

            if (cloneCoolDown < 0.2f && cloneCoolDown > 0.2 - Time.deltaTime)
                previousClone.SetActive(false);
        }
    }


    private void attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(rangePoint.position, attackRange, heroLayer);

        switch (attackCount % 3)
        {
            case 0:
                anim.SetTrigger("Attack1");
                attackDamage = 15;
                attackCoolDown = 0.4f;
                break;
            case 1:
                anim.SetTrigger("Attack2");
                attackDamage = 20;
                attackCoolDown = 0.4f;
                break;
            case 2:
                anim.SetTrigger("Attack3");
                attackDamage = 25;
                attackCoolDown = 0.8f;
                break;
            default:
                break;
        }
        if (attackCount == 3)
            attackCount = 0;

        audioSource.clip = swingSound;

        foreach (Collider2D hero in hitEnemies)
        {
            if (hero.GetComponent<Hero>() != null)
            {
                if (attackCount % 3 == 0)
                    audioSource.clip = attackSounds[0];
                else if (attackCount % 3 == 1)
                    audioSource.clip = attackSounds[1];
                else if (attackCount % 3 == 2)
                    audioSource.clip = attackSounds[2];

                if (!hero.GetComponent<Hero>().blocked() && !hero.GetComponent<Hero>().roll())
                    hero.GetComponent<Hero>().takeDamage(attackDamage);
                else if (hero.GetComponent<Hero>().blocked())
                {
                    hero.GetComponent<Hero>().takeDamage(Convert.ToInt32(attackDamage * 0.7));
                    audioSource.clip = blockSound;
                }
            }
        }
        audioSource.Play();
    }


    private void spellCast()
    {
        spell.SetActive(true);
    }


    private void clone()
    {
        previousClone.SetActive(true);
    }


    public bool getState()
    {
        return isDead;
    }
}
