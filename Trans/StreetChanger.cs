//传送触发器

using MFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//玩家传送触发器及传送点
public class StreetChanger : MonoBehaviour
{

    public string sceneName;//传送的场景的名字
    public float transTime;//传送等待的时间，比如在此时间内让场景屏幕变黑，有过渡感
    private bool locked = true;//自加锁，保证同一个场景中的多个传送器只有一个运行
    public int pathPoint;//表示和场景中的哪个点相对应，所对应的点在目标场景的SceneControl中
    private bool isInTeleportArea = false;
    // Update is called once per frame
    void Update()
    {
      //  Debug.Log(SceneControl.state);
        if (isInTeleportArea && Input.GetKeyDown(KeyCode.I) && SceneControl.state == 0)
        {
           
            Debug.Log("触发传送");
            SceneControl.state = 1;
            SceneControl.pathPoint = pathPoint;
            locked = false;
            // 原来在 Update 中可能有的传送倒计时等逻辑
            if (SceneControl.state == 1 && !locked)
            {
                transTime -= Time.deltaTime;
                if(UIManager.Instance==null)
                {                  
                    Debug.Log("UIManager实例为空"); 
                }
                else if (transTime <= 0)
                {
     
                    UITransition vt = MUIManager.Instance.ActiveUI("UITransition") as UITransition;
                    if (vt != null)
                    {
                        vt.FadeIn(() => {
                            Trans(); // 黑透了之后执行跳转

                            // 跳转完成后，再获取一次并淡出
                            UITransition vtOut = MUIManager.Instance.GetUI("UITransition") as UITransition;
                            if (vtOut != null)
                            {
                                vtOut.FadeOut(() => {
                                    MUIManager.Instance.DeActiveUI("UITransition");
                                });
                            }
                        });
                    }

                }
            }
        }
      

    }
    private void Trans()
    {
        SceneManager.LoadScene(sceneName);
        SceneControl.state = 0;
        locked = true;
    }
    //如果进入触发区域的对象为玩家，并且按下I键，场景处于空闲状态，那么进行传送过程
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.tag.Equals("ShenYan"))
       // {
            isInTeleportArea = true;
            Debug.Log("进入传送区域");
        //}
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Luna"))
        {
            isInTeleportArea = false;
        }
    }
}