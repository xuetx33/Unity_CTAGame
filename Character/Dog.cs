using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//创建人： Trigger 
//功能说明：
//***************************************** 
public class Dog : MonoBehaviour
{
    private Animator animator;
    public GameObject starEffect;
    public AudioClip petSound;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

  

    private void CanControlLuna()
    {
        GameManager.Instance.canControlShenYan = true;
    }
}
