using UnityEngine;
using UnityEngine.SceneManagement;

public class Hero : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private int MaxHealthPoints = 100;
     private int currentHealthPoints;
    [SerializeField] private int spellDamage = 35;
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private Vector3 rollDistance;
    private bool isGrounded = false;
    public bool isDead = false;
    private bool isImmortal = false;
    private float rollCoolDown = 0;
    private float immortalTime = 0;
    private int attackCount = 0;
    private int attackDamage = 0;
    private float attackCoolDown = 0f;
    private float timeBeforeDeath = 1.2f;
    private float hurtCoolDown = 0;
    private float spellCoolDown = 0;
    private float cloneCoolDown = 0;
    private bool isBlocked = false;
    private bool pauseGame;

    public AudioClip[] attackSounds;
    public AudioClip swingSound;
    public AudioClip spellSound;
    public AudioSource audioSource;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    public Transform rangePoint;
    private float attackRange = 0.77f; 
    public GameObject pauseMenu;
    public LayerMask enemyLayers;
    public Transform enemy;
    public GameObject Enemy;
    public GameObject spell;
    public GameObject heroClone;
    public Transform endOfLevel;

    private int damageMult = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHealthPoints = MaxHealthPoints;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12) && damageMult == 1)
            damageMult = 20;
        else if(Input.GetKeyDown(KeyCode.F12) && damageMult == 20)
            damageMult = 1;

        if (!isDead)
        {
            if (transform.position.y < -5)
                NoBloodDeath();

            if (Input.GetKeyDown(KeyCode.C) && isGrounded && !isImmortal && !isBlocked)
            {
                Roll();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Z) && !isBlocked)
                {
                    if (attackCoolDown <= 0 && hurtCoolDown <= 0 && spellCoolDown <= 0)
                    {
                        Attack();
                        attackCount++;
                    }
                }
                else if(Input.GetKeyDown(KeyCode.V) && !isBlocked)
                {
                    if (attackCoolDown <= 0 && hurtCoolDown <= 0 && spellCoolDown <= 0 && SceneManager.GetActiveScene().buildIndex == 3)
                    {
                        if (transform.position.x - enemy.position.x <= 10 && transform.position.x - enemy.position.x >= -10)
                        {
                            spellCast();
                            spellCoolDown = 2f;
                        }
                    }
                    else if(attackCoolDown <= 0 && hurtCoolDown <= 0 && spellCoolDown <= 0 && SceneManager.GetActiveScene().buildIndex == 2)
                    {
                        clone();
                        cloneCoolDown = 2f;
                    }
                }
                if (Input.GetKey(KeyCode.X))
                    Block();
                else if (Input.GetKeyUp(KeyCode.X))
                {
                    isBlocked = false;
                    anim.SetBool("IdleBlock", false);
                }

                if (Input.GetButton("Horizontal") && !isBlocked)
                {
                    if (isGrounded)
                        anim.SetInteger("AnimState", 1);
                    Run();
                }
                else
                    anim.SetInteger("AnimState", 0);
                if (isGrounded && Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                    anim.SetBool("Jump", true);
                }
                else
                    anim.SetBool("Jump", false);
            }
        }
    }


    private void Run()
    {
        Vector3 dir = transform.right * Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) && !sprite.flipX)
            rangePoint.position = Vector3.MoveTowards(rangePoint.position, new Vector3(transform.position.x - 1, rangePoint.position.y), 100000 * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) && sprite.flipX)
            rangePoint.position = Vector3.MoveTowards(rangePoint.position, new Vector3(transform.position.x + 1, rangePoint.position.y), 100000 * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);

        sprite.flipX = dir.x < 0.0f;
    }


    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        anim.SetFloat("AirSpeedY", 1);
    }


    private void CheckGround()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        isGrounded = collider.Length > 1;
        if (isGrounded)
            anim.SetBool("Grounded", true);
        else if(!anim.GetBool("Jump"))
        {
            anim.SetBool("Grounded", false);
            anim.SetFloat("AirSpeedY", -1);
        }
    }


    private void NoBloodDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.SetTrigger("Death");
            anim.SetBool("noBlood", true);
        }
    }


    private void Death()
    {
        if (!isDead)
        {
            isDead = true;
            anim.SetTrigger("Death");
            anim.SetBool("noBlood", false);
        }
    }


    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(rangePoint.position, attackRange, enemyLayers);

        switch (attackCount % 3)
        {
            case 0:
                anim.SetTrigger("Attack1");
                attackDamage = 15 * damageMult;
                attackCoolDown = 0.2f;
                break;
            case 1:
                anim.SetTrigger("Attack2");
                attackDamage = 20 * damageMult;
                attackCoolDown = 0.2f;
                break;
            case 2:
                anim.SetTrigger("Attack3");
                attackDamage = 25 * damageMult;
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
            if (enemy.GetComponent<Clone>() != null)
            {
                if (attackCount % 3 == 0)
                    audioSource.clip = attackSounds[0];
                else if(attackCount % 3 == 1)
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
            if (enemy.GetComponent<Previous>() != null)
            {
                if (attackCount % 3 == 0)
                    audioSource.clip = attackSounds[0];
                else if (attackCount % 3 == 1)
                    audioSource.clip = attackSounds[1];
                else if (attackCount % 3 == 2)
                    audioSource.clip = attackSounds[2];
                enemy.GetComponent<Previous>().takeDamage(attackDamage);
            }
            
        }
        audioSource.Play();
    }


    private void Block()
    {
        isBlocked = true;
        anim.SetTrigger("Block");
        anim.SetBool("IdleBlock", true);
    }

    
    private void Roll()
    {
        if (rollCoolDown <= 0)
        {
            isImmortal = true;
            rollCoolDown = 1;
            immortalTime = 0.6f;
            anim.SetTrigger("Roll");
            if (sprite.flipX)
                transform.position = Vector3.Lerp(transform.position, transform.position - rollDistance, 0.005f);
            else
                transform.position = Vector3.Lerp(transform.position, transform.position + rollDistance, 0.005f);
        }
    }


    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (MaxHealthPoints >= 250 && transform.position.x > endOfLevel.position.x)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (MaxHealthPoints >= 400 && transform.position.x > endOfLevel.position.x)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            if (Enemy.GetComponent<Previous>().getState())
            {
                if (transform.position.x <= endOfLevel.position.x)
                    SceneManager.LoadScene(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseGame)
                Resume();
            else
                Pause();
        }

        
        if (isDead)
        {
            timeBeforeDeath -= Time.deltaTime;
            if (timeBeforeDeath <= 0)
            {
                timeBeforeDeath = 1.2f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        if (isImmortal)
        {
            immortalTime -= Time.deltaTime;

            Physics2D.IgnoreLayerCollision(6, 3);

            if (immortalTime <= 0)
            {
                isImmortal = false;
            }
        }
        else
            Physics2D.IgnoreLayerCollision(6, 3, false);

        if (rollCoolDown > 0)
        {
            rollCoolDown -= Time.deltaTime;
        }

        CheckGround();
        if (attackCoolDown >= 0)
            attackCoolDown -= Time.deltaTime;
        if (hurtCoolDown > 0)
            hurtCoolDown -= Time.deltaTime;

        if (spellCoolDown > 0)
        {
            spellCoolDown -= Time.deltaTime;
            if (spellCoolDown < 1.4 && spellCoolDown > 1.4 - Time.deltaTime)
            {
                spell.transform.position = new Vector3(enemy.position.x, enemy.position.y + 1.5f, enemy.position.z);
                spell.GetComponent<SpellAnim>().cast();

                audioSource.clip = spellSound;
                audioSource.Play();
            }
            if (spellCoolDown < 1.4f && spellCoolDown > 0.4)
            {
                spell.transform.position = Vector3.MoveTowards(new Vector3(enemy.position.x, enemy.position.y + 1.5f, enemy.position.z), spell.transform.position, (speed - 3) * Time.deltaTime);
            }
            if (spellCoolDown < 0.4f && spellCoolDown > 0.4 - Time.deltaTime)
            {

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(spell.transform.position, attackRange, enemyLayers);

                foreach (Collider2D enemy in hitEnemies)
                {
                    if (SceneManager.GetActiveScene().buildIndex == 2)
                        enemy.GetComponent<Bringer>().takeDamage(spellDamage);
                    else if(SceneManager.GetActiveScene().buildIndex == 3)
                        enemy.GetComponent<Previous>().takeDamage(spellDamage);
                }
            }
            if (spellCoolDown < 0.2f && spellCoolDown > 0.2 - Time.deltaTime)
                spell.SetActive(false);
        }

        if (cloneCoolDown > 0)
        {
            cloneCoolDown -= Time.deltaTime;
            if (cloneCoolDown < 1.8 && cloneCoolDown > 1.8 - Time.deltaTime)
                heroClone.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            if (cloneCoolDown <= 1.8f && cloneCoolDown >= 0.2)
                heroClone.GetComponent<HeroClone>().attack();

            if (cloneCoolDown < 0.2f && cloneCoolDown > 0.2 - Time.deltaTime)
                heroClone.SetActive(false);
        }
    }


    public void takeDamage(int damage)
    {
        if (!isDead)
        {
            anim.SetTrigger("Hurt");
            currentHealthPoints -= damage;
            hurtCoolDown = 0.6f;

            if (currentHealthPoints <= 0)
            {
                Death();
            }
        }
    }

    public int getMaxHelth()
    {
        return MaxHealthPoints;
    }

    public int getCurrentHealth()
    {
        return currentHealthPoints;
    }

    public bool blocked()
    {
        return isBlocked;
    }

    public bool roll()
    {
        return isImmortal;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        pauseGame = false;
    }

    private void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        pauseGame = true;
    }

    public void loadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void spellCast()
    {
        spell.SetActive(true);
    }

    private void clone()
    {
        heroClone.SetActive(true);
    }

    public void getPotion(int healthAdd)
    {
        MaxHealthPoints += healthAdd;
        currentHealthPoints = MaxHealthPoints;
    }
}