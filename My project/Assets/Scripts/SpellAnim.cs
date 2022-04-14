using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellAnim : MonoBehaviour
{
    private Animator anim; 
    private void Start()
    {
        anim = GetComponent<Animator>();
    }


    public void cast()
    {
        anim.SetTrigger("SpellCast");
    }

}
