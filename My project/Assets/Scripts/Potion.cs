using UnityEngine;
using UnityEngine.SceneManagement;

public class Potion : MonoBehaviour
{
    public GameObject bringer;
    public GameObject potion;
    public GameObject hero;
    public int healthAdd;

    void Update()   
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (bringer.GetComponent<Clone>().getState())
                potion.transform.position = bringer.transform.position;
        }
        else if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (bringer.GetComponent<Bringer>().getState())
            {
                potion.transform.position = bringer.transform.position;
            }
        }  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && potion.activeSelf)
        {
            hero.GetComponent<Hero>().getPotion(healthAdd);
            GameObject.FindGameObjectWithTag("HealthBar").GetComponent<HealthSystem>().newHealth(healthAdd);
            potion.SetActive(false);
        }
    }
}
