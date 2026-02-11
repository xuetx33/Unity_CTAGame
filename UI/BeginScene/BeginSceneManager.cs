using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //按名称加载场景
    public void ReplaceScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // 按名称加载

    }
    //开始游戏按钮的回调函数
    public void OnStartGame()
    {
        ReplaceScene("FirstScene");
    }
    //关闭游戏按钮的回调函数
    public void OnCloseGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 编辑器模式下停止播放
        #else
        Application.Quit(); // 独立应用关闭
        #endif
    }
    public void ContinueGame()
    {
        ReplaceScene("MainScene");
        SceneManager.sceneLoaded += OnMainSceneLoaded;
    }
    // 场景加载完成后执行的回调
    private void OnMainSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "FirstScene")
        {
            // 3. 取消监听（避免重复触发）
            SceneManager.sceneLoaded -= OnMainSceneLoaded;

            // 4. 执行读档逻辑（此时主场景已完全加载）
            SaveSystem.Instance.ClickLoadButton();
        }
    }


}
