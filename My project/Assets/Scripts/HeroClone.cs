using System;
using UnityEngine;

public class HeroClone : MonoBehaviour
{
    private int attackDamage = 0;
    private float attackCoolDown = 0f;
    private int attackCount = 0;
    public Transform enemy;
    private SpriteRenderer sprite;
    private Animator anim;
    public Transform rangePoint;
    public LayerMask enemyLayers;
    private float attackRange = 0.77f;

    public AudioClip[] attackSounds;
    public AudioClip swingSound;
    public AudioSource audioSource;
    public AudioClip blockSound;


    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }


    public void attack()
    {
        if (attackCoolDown <= 0)
        {
            attackCount++;
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(rangePoint.position, attackRange, enemyLayers);

            switch (attackCount % 3)
            {
                case 0:
                    anim.SetTrigger("Attack1");
                    attackDamage = 15;
                    attackCoolDown = 0.2f;
                    break;
                case 1:
                    anim.SetTrigger("Attack2");
                    attackDamage = 20;
                    attackCoolDown = 0.2f;
                    break;
                case 2:
                    anim.SetTrigger("Attack3");
                    attackDamage = 25;
                    attackCoolDown = 0.6f;
                    break;
                default:
                    break;
            }
            if (attackCount == 3)
                attackCount = 0;

            audioSource.clip = swingSound;

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.GetComponent<Previous>() != null)
                {
                    if (attackCount % 3 == 0)
                        audioSource.clip = attackSounds[0];
                    else if (attackCount % 3 == 1)
                        audioSource.clip = attackSounds[1];
                    else if (attackCount % 3 == 2)
                        audioSource.clip = attackSounds[2];
                    enemy.GetComponent<Clone>().takeDamage(attackDamage);
                }
                if (enemy.GetComponent<Bringer>() != null)
                {
                    if (attackCount % 3 == 0)
                        audioSource.clip = attackSounds[0];
                    else if (attackCount % 3 == 1)
                        audioSource.clip = attackSounds[1];
                    else if (attackCount % 3 == 2)
                        audioSource.clip = attackSounds[2];
                    enemy.GetComponent<Bringer>().takeDamage(attackDamage);
                }
                if (enemy.GetComponent<Hero>() != null)
                {
                    if (attackCount % 3 == 0)
                        audioSource.clip = attackSounds[0];
                    else if (attackCount % 3 == 1)
                        audioSource.clip = attackSounds[1];
                    else if (attackCount % 3 == 2)
                        audioSource.clip = attackSounds[2];

                    if (!enemy.GetComponent<Hero>().blocked() && !enemy.GetComponent<Hero>().roll())
                        enemy.GetComponent<Hero>().takeDamage(attackDamage);
                    else if (enemy.GetComponent<Hero>().blocked())
                    {
                        enemy.GetComponent<Hero>().takeDamage(Convert.ToInt32(attackDamage * 0.7));
                        audioSource.clip = blockSound;
                    }
                        
                }
            }
            audioSource.Play();
        }
    }

    private void FixedUpdate()
    {
        if (enemy.position.x < transform.position.x && !sprite.flipX)
            rangePoint.position = Vector3.MoveTowards(rangePoint.position, new Vector3(transform.position.x - 1, rangePoint.position.y), 100000 * Time.deltaTime);
        else if (enemy.position.x > transform.position.x && sprite.flipX)
            rangePoint.position = Vector3.MoveTowards(rangePoint.position, new Vector3(transform.position.x + 1, rangePoint.position.y), 100000 * Time.deltaTime);

        if (attackCoolDown > 0)
            attackCoolDown -= Time.deltaTime;

        if (enemy.position.x < transform.position.x)
            sprite.flipX = true;
        else
            sprite.flipX = false;
    }
}
