/*********************************************************************************
 *Copyright(C) 2015 by FengNongtheWindCoder
 *All rights reserved.
 *FileName:     BoardManager.cs
 *Author:       FengNongtheWindCoder
 *Version:      1.0
 *UnityVersion：5.3.0f4
 *Date:         2015-12-27
 *Description:  此脚本负责：
                生成图片，打乱piece位置
                统计归位piece数量

 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public GameManager gameManager;//gamemanager脚本
    public GameObject puzzleImgObject;//当前场景中的图片父节点对象
    public Vector3[] allRightPos  ;//记录所有正确位置，作为piece临近时的自动入库，入库并不会提示是否是正确位置，临近任何一个位置都会入库
    public float maxAutoMoveDistance = 0.2f; //自动移动入库的最大距离，当距离某个正确位置小于这个距离自动入库。
    public int finishedPieceNum;//归位的piece数量

    private Transform spawnPoint;//场景内的拼图生成点
    private float sceneMaxX;//当前场景坐标范围
    private float sceneMaxY;
    private string pieceSortinglayer = "pieces";//图片显示层

    /// <summary>
    /// Setups the board.
    /// </summary>
    /// <param name="puzzlename">The puzzlename.</param>
    public void SetupBoard(string puzzlename)
    {
        //找到场景中的生成点
        spawnPoint = GameObject.FindGameObjectWithTag("ImgBoard").transform;
        Camera currentCamera = Camera.main;
        sceneMaxX = currentCamera.orthographicSize * currentCamera.aspect;
        sceneMaxY = currentCamera.orthographicSize;
        //生成此prefab
        GameObject prefab = Resources.Load(puzzlename) as GameObject;
        puzzleImgObject = Instantiate(prefab, spawnPoint.position, Quaternion.identity) as GameObject;
        allRightPos = new Vector3[puzzleImgObject.transform.childCount];
        //随机位置摆放碎片
        int maxChild = puzzleImgObject.transform.childCount;
        int size = (int) Mathf.Sqrt((float)maxChild);//只考虑正方形长宽上的碎片数相等。
        for (int i = 0; i < maxChild; i++)
        {
            GameObject piece = puzzleImgObject.transform.GetChild(i).gameObject;
            //设置显示层
            SpriteRenderer render = piece.GetComponent<SpriteRenderer>();
            render.sortingLayerName = pieceSortinglayer;
            //记录原位置 
            allRightPos[i] = piece.transform.position;

            //添加collider,pieceController脚本
            piece.AddComponent<BoxCollider2D>();
            PieceController pieceController =  piece.AddComponent<PieceController>();
            pieceController.boardManager = this;
            pieceController.rightPosition = piece.transform.position;
            pieceController.currentSprite = render.sprite;
            pieceController.width = render.sprite.rect.width / render.sprite.pixelsPerUnit;
            //记录邻居碎片
            SetPieceNeighbour(pieceController, i, maxChild, size);
            //设置摆放位置
            SetRandomPiecePosition(piece);
        }
    }
    /// <summary>
    /// Sets the piece neighbour.
    /// </summary>
    /// <param name="pieceController">The piece controller.</param>
    /// <param name="index">The index.</param>
    /// <param name="maxChild">The maximum child.</param>
    /// <param name="size">The size.</param>
    void SetPieceNeighbour(PieceController pieceController,int index, int maxChild,int size)
    {
        //邻居各个编号
        int left = index - 1;
        int right = index + 1;
        int up = index + size;
        int down = index - size;
        //检查范围，而且左右必须在同一行
        if (0 <= left && left < maxChild 
            && index / size == left / size )
        {
            pieceController.leftNeighbour = puzzleImgObject.transform.GetChild(left).gameObject;
        }
        if (0 <= right && right < maxChild
            && index / size == right / size)
        {
            pieceController.rightNeighbour = puzzleImgObject.transform.GetChild(right).gameObject;
        }
        if (0 <= up && up < maxChild)
        {
            pieceController.upNeighbour = puzzleImgObject.transform.GetChild(up).gameObject;
        }
        if (0 <= down && down < maxChild)
        {
            pieceController.downNeighbour = puzzleImgObject.transform.GetChild(down).gameObject;
        }
    }
    /// <summary>
    /// Sets the random piece position.
    /// </summary>
    /// <param name="piece">The piece.</param>
    private void SetRandomPiecePosition(GameObject piece)
    {
        Vector2 spriteSize = piece.GetComponent<SpriteRenderer>().sprite.rect.size;//sprite大小，pixel
        Vector2 worldSpriteSize = spriteSize / piece.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        float pieceMaxX = sceneMaxX  - worldSpriteSize.x/2;//横向不能超出边界，中心坐标距离边界要大于半个size
        float pieceMaxY = sceneMaxY - 1 - worldSpriteSize.y / 2;//竖向除了边界问题，还有上方预留的ui。下方正常。
        float pieceMinY = -1 * (sceneMaxY - worldSpriteSize.y / 2);
        float randomX = Random.Range(-pieceMaxX, pieceMaxX);
        float randomY = Random.Range(pieceMinY, pieceMaxY);
        piece.transform.position = new Vector2(randomX, randomY);
    }
    public bool puzzleFinished()
    {
        return finishedPieceNum == allRightPos.Length;
    }
}
