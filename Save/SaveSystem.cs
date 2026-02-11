using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;
    //数据实例化
    SaveData data = new();
    //获取“我的文档”对应的路径目录
    private string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    //存放存档文件的那个文件夹的名字
    private const string SaveFolder = "LunaGame";
    //存档文件名称（这里要全称）
    private const string SaveFileName = "LunaGameTest.txt";

    //声明一个目录信息用来保存路径
    private DirectoryInfo directoryInfo;

    //保存按钮的特效
    [SerializeField] Image SaveEffect;


    GameObject lunaGameObject;
    Transform lunaTransform;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        //这里的Path.Combine()用来链接路径，实际上用 SavePath + "/" + SaveFloder 表示也行
        directoryInfo = new DirectoryInfo(Path.Combine(SavePath, SaveFolder));

    }
    private void Start()
    {
        lunaGameObject = GameObject.FindGameObjectWithTag("ShenYan");
        if (lunaGameObject != null)
        {

            lunaTransform = lunaGameObject.transform;
            Vector3 position = lunaTransform.position;
            Debug.Log($"Luna的位置是: {position}");
        }
        else
        {
            Debug.Log("未找到带有Luna标签的游戏物体");
        }
    }
    //用来增加金币
    public void AddCoin()
    {
        data.coin++;
    }
    //保存数据
    public void Save()
    {
        //如果该路径不存在就先将其创建出来
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        data.LunaPosition = lunaTransform.position; // 保存Luna的位置  

        //保存特效显示
        SaveEffect.gameObject.SetActive(true);
        //UIManager.Instance.FadeOut(SaveEffect,1.5f, () =>
        //{
        //    SaveEffect.gameObject.SetActive(false);
        //});

        //保存数据直接转换json序列，暂未加密
        string EncryptedData = JsonUtility.ToJson(data);

        File.WriteAllText(Path.Combine(directoryInfo.FullName, SaveFileName), EncryptedData);

    }
    //加载数据
    public void Load()
    {
        //与上述Save为逆过程        
        string DecryptedData
        = File.ReadAllText(Path.Combine(directoryInfo.FullName, SaveFileName));
        data = JsonUtility.FromJson<SaveData>(DecryptedData);


        lunaTransform.position = data.LunaPosition; // 恢复Luna的位置

    }

    private void Update()
    {
        //GameObject.Find("number").GetComponent<Text>().text = data.coin.ToString();
    }
    public void ClickSaveButton()
    {
        Save(); // 调用保存方法



        Debug.Log("保存成功！");
    }
    public void ClickLoadButton()
    {
        //// 调用过场管理器，传入存档路径和加载完成后的逻辑
        //UIManager.Instance.StartLoadTransition(
            
        //    OnLoadComplete // 加载完成后执行的回调
        //);
       

        Debug.Log("读取成功！");

    }
    // 加载完成后执行（如应用存档数据到游戏）
    private void OnLoadComplete()
    {
        Load(); // 调用加载方法
        // 例如：更新玩家位置
        Debug.Log("存档读取完成，应用游戏数据...");
    }

}