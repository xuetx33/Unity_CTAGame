using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject TalkPanelGo;
    public Image characterImage;
    public Sprite[] characterSprtes;
    public Text nameText;
    public Text contentText;
    public Text taskName;
    public Text taskText;

    public GameObject MapTPPanelGo;
    public GameObject MapBagPanelGo;
    public GameObject MapTaskPanelGo;

    public GameObject slotGrid;
    public Text itemInfromation;
    public string interactNpc;




    ////加载过场UI元素
    //[Header("过场UI元素")]
    //public Image blackScreen; // 黑屏遮罩
    //public Text loadingText; // 加载文字
    //public Slider progressBar; // 进度条

    //[Header("过渡参数")]
    //public float fadeInDuration = 0.5f; // 黑屏淡入时间
    //public float fadeOutDuration = 0.8f; // 黑屏淡出时间
    //public float stayTime = 0.5f; // 黑屏淡入时间


    void Awake()
    { // 单例逻辑：确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 手动标记持久化（或通过PersistentObjectManager）
        }
        else
        {
            Destroy(gameObject); // 销毁重复实例
            return;
        }

        //// 过场动画初始化状态（隐藏黑屏）
        //Color blackColor = blackScreen.color;
        //blackColor.a = 0;
        //blackScreen.color = blackColor;
        //loadingText.gameObject.SetActive(false);
        //if (progressBar != null) progressBar.gameObject.SetActive(false);
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
       
    }
    private void Update()
    {
       
    
    }

   
    public void ShowOrHideTPPanel(bool show)
    {
        MapTPPanelGo.SetActive(show);
    }
    public bool IsTPPanelOpen()
    {
        return MapTPPanelGo.activeSelf;
    }
    public void ShowOrHideBagPanel(bool show)
    {
        MapBagPanelGo.SetActive(show);
    }
    public bool IsBagPanelOpen()
    {
        return MapBagPanelGo.activeSelf;
    }
    public void ShowOrHideTaskPanel(bool show)
    {
        MapTaskPanelGo.SetActive(show);
    }
    public bool IsTaskPanelOpen()
    {
        return MapTaskPanelGo.activeSelf;
    }
   
    public void ShowDialog(string content = null, string name = null)
    {
       
            TalkPanelGo.SetActive(true);
            if (name != null)
            {
                if (name == "Luna")
                {
                    characterImage.sprite = characterSprtes[0];
                }
                else
                {
                    characterImage.sprite = characterSprtes[1];
                }
                characterImage.SetNativeSize();
            }
            contentText.text = content;
            nameText.text = name;
        
    }
    public void HideDialog(string content = null, string name = null)
    {
        TalkPanelGo.SetActive(false);
    }
    public void RefreshItem()
    {
        for(int i=0;i<Instance.slotGrid.transform.childCount;i++)
        {
            if(Instance.slotGrid.transform.childCount==0)
            {
                break;
            }
            Destroy(Instance.slotGrid.transform.GetChild(i).gameObject);
        }
    }
    // 渐隐 Image 组件方法  参数为：渐隐image组件，渐隐持续时间，渐隐完成后的回调函数
    //public Coroutine FadeOut(Image image, float duration = 1f, System.Action onComplete = null)
    //{
    //    return StartCoroutine(FadeOutCoroutine(image, duration, onComplete));
    //}

    //// 渐隐 Image 的协程
    //private IEnumerator FadeOutCoroutine(Image image, float duration, System.Action onComplete)
    //{
    //    if (image == null) yield break;

    //    float startTime = Time.time;
    //    Color originalColor = image.color;

    //    while (Time.time < startTime + duration)
    //    {
    //        float progress = (Time.time - startTime) / duration;
    //        image.color = new Color(
    //            originalColor.r,
    //            originalColor.g,
    //            originalColor.b,
    //            Mathf.Lerp(1f, 0f, progress)
    //        );
    //        yield return null;
    //    }

    //    image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    //    onComplete?.Invoke();
    //}
    ///// <summary>
    ///// 开始读取存档过场
    ///// </summary>
    ///// <param name="onLoadComplete">读取完成后的回调（如加载场景、应用数据）</param>
    //public void StartLoadTransition( System.Action onLoadComplete)
    //{
    //    StartCoroutine(LoadTransitionCoroutine(onLoadComplete,0.5f,"读取中..."));
    //}

    //播放黑屏过场动画
    //public IEnumerator LoadTransitionCoroutine( System.Action onLoadComplete,float stayTime,string text)
    //{
    //    loadingText.text = text;
    //    loadingText.gameObject.SetActive(true);
    //    // 1. 黑屏淡入
    //    yield return StartCoroutine(FadeBlackScreen(0, 1, fadeInDuration));
        
    //    if (progressBar != null) progressBar.gameObject.SetActive(true);

  

    //    // 停留指定时间
    //    yield return new WaitForSeconds(stayTime);

    //    // 3. 调用读取完成后的逻辑（如切换场景、刷新UI）
    //    onLoadComplete?.Invoke();

    //    // 4. 黑屏淡出
    //    loadingText.gameObject.SetActive(false);
    //    if (progressBar != null) progressBar.gameObject.SetActive(false);
    //    yield return StartCoroutine(FadeBlackScreen(1, 0, fadeOutDuration));
    //}

    // 读取存档数据（模拟进度）
    //private IEnumerator LoadSaveData(string savePath)
    //{
    //    float progress = 0;
    //    while (progress < 1)
    //    {
    //        // 实际项目中，这里替换为真实的读取逻辑（如读取文件、解析数据）
    //        progress += Time.deltaTime * 0.5f; // 模拟加载进度
    //        progress = Mathf.Clamp01(progress);

    //        // 更新进度条
    //        if (progressBar != null)
    //            progressBar.value = progress;

    //        // 更新提示文字（可选）
    //        loadingText.text = $"读取存档中... {Mathf.RoundToInt(progress * 100)}%";

    //        yield return null;
    //    }
    //}

    // 黑屏淡入淡出效果
    //private IEnumerator FadeBlackScreen(float startAlpha, float targetAlpha, float duration)
    //{
    //    float startTime = Time.time;
    //    Color color = blackScreen.color;

    //    while (Time.time < startTime + duration)
    //    {
    //        float t = (Time.time - startTime) / duration;
    //        color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
    //        blackScreen.color = color;
    //        yield return null;
    //    }

    //    // 确保最终状态正确
    //    color.a = targetAlpha;
    //    blackScreen.color = color;
    //}
    ////关闭游戏按钮的回调函数
    public void OnCloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 编辑器模式下停止播放
#else
        Application.Quit(); // 独立应用关闭
#endif
    }
    //按名称加载场景
    public void ReplaceScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // 按名称加载

    }
    // 显示/隐藏任务面板
 
   
}
 