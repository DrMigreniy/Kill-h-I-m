using UnityEngine;
using UnityEngine.UI;
using System;

public class HealthSystem : MonoBehaviour
{
	public static HealthSystem Instance;

	public Image currentHealthGlobe;
	private float hitPoint;
	private float maxHitPoint;
	public Text healthCounter;


	void Awake()
	{
		Instance = this;
		maxHitPoint = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>().getMaxHelth();
	}


  	void Start()
	{
		UpdateHealthGlobe();
	}

	
	void Update ()
	{
		hitPoint = GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>().getCurrentHealth();
		UpdateHealthGlobe();
		healthCounter.text = Convert.ToString(GameObject.FindGameObjectWithTag("Player").GetComponent<Hero>().getCurrentHealth());
	}


	private void UpdateHealthGlobe()
	{
		float ratio = hitPoint / maxHitPoint;
		currentHealthGlobe.rectTransform.localPosition = new Vector3(0, currentHealthGlobe.rectTransform.rect.height * ratio - currentHealthGlobe.rectTransform.rect.height, 0);
	}


	public void newHealth(int MaxHealthAdd)
    {
		maxHitPoint += MaxHealthAdd;
    }
}
