using System;
using UnityEngine;

public class Bringer : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private int MaxHealthPoints = 1500;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private int spellDamage = 35;
    [SerializeField] private float agresDistance;

    private int currentHealthPoints;
    private Animator anim;
    private Rigidbody2D rb;
    private new BoxCollider2D collider;
    public Transform rangePoint;
    public Transform leftBorder;
    public Transform rightBorder;

    public AudioClip attackSound;
    public AudioClip blockSound;
    public AudioClip swingSound;
    public AudioClip spellSound;
    public AudioSource audioSource;

    public Transform patrolPoint;
    public float attackRange = 0.5f;
    public LayerMask heroLayer;
    public Transform player;
    private float attackCoolDown = 0;
    private float hurtCoolDown = 0;
    private float spellCoolDown = 0;
    private float dead = 0;
    private SpriteRenderer sprite;
    public GameObject spell;
    public GameObject bringer;

    bool isDead = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
        currentHealthPoints = MaxHealthPoints;

        Physics2D.IgnoreLayerCollision(3, 8);
    }

    void Update()
    {
        if (!isDead)
        {
            if (spellCoolDown <= 0 && attackCoolDown <= 0 && hurtCoolDown <= 0)
            {
                if (transform.position.x - player.position.x <= 2 && transform.position.x - player.position.x >= -2)
                {
                    attackCoolDown = 0.8f;
                    attack();
                }
            }

            if (spellCoolDown <= 0 && attackCoolDown <= 0 && hurtCoolDown <= 0)
            {
                spellCoolDown = 2.4f;
                spellCast();
            }
            
            if (Vector2.Distance(transform.position, player.position) > agresDistance && transform.position.x == patrolPoint.position.x)
                idle();
            else if (Vector2.Distance(transform.position, player.position) < agresDistance && spellCoolDown < 1)
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
        dead = 1f;
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

        if (attackCoolDown <= 0 && hurtCoolDown <= 0 && transform.position.x > leftBorder.position.x && transform.position.x < rightBorder.position.x)
        {
            anim.SetBool("Walk", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else
            anim.SetBool("Walk", false);
    }

    private void goBack()
    {
        if (patrolPoint.position.x < transform.position.x)
            sprite.flipX = false;
        else
            sprite.flipX = true;

        anim.SetBool("Walk", true);

        transform.position = Vector2.MoveTowards(transform.position, patrolPoint.position, speed * Time.deltaTime);
    }

    private void idle()
    {
        anim.SetBool("Walk", false);
    }


    private void FixedUpdate()
    {
        if (isDead && dead > 0)
            dead -= Time.deltaTime;
        else if(isDead && dead <= 0)
            bringer.SetActive(false);

        if (attackCoolDown > 0)
            attackCoolDown -= Time.deltaTime;
        if (hurtCoolDown > 0)
            hurtCoolDown -= Time.deltaTime;
        if (spellCoolDown > 0)
        {
            spellCoolDown -= Time.deltaTime;
            if (spellCoolDown < 1.4 && spellCoolDown > 1.4 - Time.deltaTime)
            {
                spell.transform.position = new Vector3(player.position.x, player.position.y + 3, player.position.z);
                spell.GetComponent<SpellAnim>().cast();

                audioSource.clip = spellSound;
                audioSource.Play();
            }
            if (spellCoolDown < 1.4f && spellCoolDown > 0.4)
            {
                spell.transform.position = Vector3.MoveTowards(new Vector3(player.position.x, player.position.y + 3, player.position.z), spell.transform.position, (speed - 3) * Time.deltaTime);
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
            if(spellCoolDown < 0.2f && spellCoolDown > 0.2 - Time.deltaTime)
                spell.SetActive(false);
        }
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
            else if (hero.GetComponent<Hero>().blocked())
            {
                hero.GetComponent<Hero>().takeDamage(Convert.ToInt32(attackDamage * 0.7));
                audioSource.clip = blockSound;
            }
        }
        audioSource.Play();
    }

    private void spellCast()
    {
        spell.SetActive(true);

        anim.SetTrigger("Cast");
    }

    public bool getState()
    {
        return isDead;
    }
}