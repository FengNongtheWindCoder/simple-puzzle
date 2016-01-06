/*********************************************************************************
 *Copyright(C) 2015 by FengNongtheWindCoder
 *All rights reserved.
 *FileName:     GameManager.cs
 *Author:       FengNongtheWindCoder
 *Version:      1.0
 *UnityVersion：5.3.0f4
 *Date:         2015-12-27
 *Description:  此脚本负责：
                保存并传递具体游戏拼图名称
                游戏几个状态间的转换
                显示界面的ui控制
                显示开始结束画面控制
                此脚本通过摄像机上的loader保持整个游戏场景间唯一。
 *History:  
**********************************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    //保持此脚本场景中唯一，也是保持脚本所在的GameManager对象场景唯一的手段
    public static GameManager instance;
    //获取游戏具体逻辑控制
    public BoardManager boardManager;
    //此场景需要进行的拼图名称
    public string puzzlename;
    void Awake()
    {
        //预防重复检查
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        //保持场景切换不删除
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        InitGame();
#endif
    }

    void OnLevelWasLoaded(int level)
    {
        InitGame();
    }
   
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (boardManager.puzzleFinished())
        {
            Debug.Log("game finish!");
        }
        
    }
    /// <summary>
    /// Initializes the game.
    /// </summary>
    void InitGame()
    {
        boardManager.SetupBoard(puzzlename);
    }
    /// <summary>
    /// Reloads the level.
    /// </summary>
    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
