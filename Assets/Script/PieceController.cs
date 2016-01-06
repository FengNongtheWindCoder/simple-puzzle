/*********************************************************************************
 *Copyright(C) 2015 by FengNongtheWindCoder
 *All rights reserved.
 *FileName:     PieceController.cs
 *Author:       FengNongtheWindCoder
 *Version:      1.0
 *UnityVersion：5.3.0f4
 *Date:         2015-12-29
 *Description:  碎片移动的具体控制
                包括碎片跟随鼠标移动
                鼠标松开时检测是否临近展示板的某个固定位置，如果临近则自动入库 
                记录完成块数，随时跟踪修改boardManager里的记录
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PieceController : MonoBehaviour
{
    public enum PieceDisplayOrder { onBoard = 0, free = 1, followMouse = 2 };//控制piece显示的orderinlayer,入库onboard，正常free，拖拽followmouse
    public BoardManager boardManager;//在BoardManager中初始化
    public Vector3 rightPosition;//此块碎片的正确位置
    public GameObject leftNeighbour;//上下左右邻居,不存在的为null，已经组成一组的为null
    public GameObject rightNeighbour;
    public GameObject upNeighbour;
    public GameObject downNeighbour;
    public Sprite currentSprite;//当前使用的sprite
    public float width;//当前的尺寸，世界坐标大小
    public bool isInGroup = false;//是否处于某个group里
    public GroupControler groupController;//group的控制脚本
    public Vector3 mouseOffset;//鼠标与碎片中心的位置差异
    public bool finished = false;//这片碎片是否已经完成，回到自己的正确位置

    void Update()
    {
        if (!isInGroup)
        {
            return;
        }
        switch (groupController.moveState)
        {
            case GroupControler.GroupMoveState.idle:
                break;
            case GroupControler.GroupMoveState.start:
                StartMove();
                FollowMouse();
                break;
            case GroupControler.GroupMoveState.moving:
                FollowMouse();
                break;
            case GroupControler.GroupMoveState.finish:
                FinishMove();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Called when [mouse down].
    /// </summary>
    void OnMouseDown()
    {
        if (isInGroup)
        {
            groupController.StartMove();
            return;
        }
        StartMove();
    }

    /// <summary>
    /// Called when [mouse up].清除偏移量，将碎片放回原来的层
    /// </summary>
    void OnMouseUp()
    {
        if (isInGroup)
        {
            groupController.FinishMove();
            return;
        }
        FinishMove();
    }
    /// <summary>
    /// Called when [mouse drag].获取这一帧的鼠标位置，调用follow函数,只在不处于group时处理移动
    /// </summary>
    void OnMouseDrag()
    {
        if (!isInGroup)
        {
            FollowMouse();
        }
    }
    /// <summary>
    /// Starts the move.记录移动开始时的一些状态,获取鼠标位置计算偏移量，将碎片上移一层防止被其他遮挡到
    /// </summary>
    void StartMove()
    {
        mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = (int)PieceDisplayOrder.followMouse;
    }
    /// <summary>
    /// Finishes the move.移动结束时进行的处理.检查入库，检查是否临近邻居，检查完成状态并修改manager的计数
    /// </summary>
    void FinishMove()
    {
        mouseOffset = Vector3.zero;
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = (int)PieceDisplayOrder.free;
        //进行入库检查,自动进入临近的某个正确点位
        if (!CheckNearAnyRightPos())
        {
            CheckNearAnyNeighbour();
        }
        //判断是否进入自己的正确位置，如果是，对完成统计加1
        if (!finished && transform.position.Equals(rightPosition))
        {
            boardManager.finishedPieceNum++;
            finished = true;
        }
        //判断是否移出自己的正确位置，取消完成状态
        else if (finished && !transform.position.Equals(rightPosition))
        {
            boardManager.finishedPieceNum--;
            finished = false;
        }
    }
    /// <summary>
    /// Follows the mouse.piece 在鼠标拖拽时跟随移动
    /// </summary>
    /// <param name="enableOffset">if set to <c>true</c> [enable offset].</param>
    void FollowMouse(bool enableOffset = true)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePos;
        if (enableOffset)
        {
            transform.position = transform.position + mouseOffset;
        }

    }
    /// <summary>
    /// 检查周围是否有邻居，有邻居直接贴近并形成一个group,并将相应的邻居设置为null，以后不会检测了
    /// </summary>
    void CheckNearAnyNeighbour()
    {
        if (CheckNearNeighbour(rightNeighbour, new Vector2(-width, 0)))
        {
            rightNeighbour = null;
        }
        if (CheckNearNeighbour(leftNeighbour, new Vector2(width, 0)))
        {
            leftNeighbour = null;
        }
        if (CheckNearNeighbour(upNeighbour, new Vector2(0, -width)))
        {
            upNeighbour = null;
        }
        if (CheckNearNeighbour(downNeighbour, new Vector2(0, width)))
        {
            downNeighbour = null;
        }
    }
    /// <summary>
    /// 按照给定的偏移量计算是否这个邻居是临近的状态，如果是就组在一起
    /// </summary>
    /// <param name="neighbour">The neighbour.</param>
    /// <param name="neighbourOffset">The neighbour offset.</param>
    /// <returns>临近返回true，否则false</returns>
    bool CheckNearNeighbour(GameObject neighbour, Vector2 neighbourOffset)
    {
        if (neighbour == null)
        {
            return false;
        }
        PieceController neighbourController = neighbour.GetComponent<PieceController>();
        //处在同一个组内部，不需要再组了，也不需要移动位置，所以直接返回true
        if (isInGroup && neighbourController.groupController == groupController)
        {
            return true;
        }
        //其他情况
        Vector2 pos = transform.position;
        //计算临近状态代表的一个rect，中心就是邻居的旁边，rect的大小是允许的误差值，在这个rect内就算是临近
        Rect automoveRange = new Rect(0, 0, 2 * boardManager.maxAutoMoveDistance, 2 * boardManager.maxAutoMoveDistance);
        Vector2 neighbourPos = neighbour.transform.position;
        automoveRange.center = neighbourPos + neighbourOffset;
        if (automoveRange.Contains(pos))
        {
            if (isInGroup == false )
            {
                //两个都不在组里
                if (neighbourController.isInGroup == false)
                {
                    transform.position = automoveRange.center;
                    //把两个组成一个新的group
                    GroupControler.CreatePieceGroup(this, neighbour.GetComponent<PieceController>());
                }
                //对方在组里
                else
                {
                    transform.position = automoveRange.center;
                    //把自己加入邻居的group
                    neighbourController.groupController.AddChild(this);
                }
                
            }
            else  
            {
                Vector3 offset = new Vector3(automoveRange.center.x, automoveRange.center.y) - transform.position;
                //两个都在组里但是不在同一个组,将自己的组内成员全部转移到对方组内，同时位移，然后删除原来的组
                if (neighbourController.isInGroup == true)
                {
                    groupController.ChangeGroupThenSelfdestroy(neighbourController.groupController, offset);
                }
                //对方不在组里,将其加入自己的组内
                else
                {
                    groupController.MoveAllChild(offset);
                    groupController.AddChild(neighbourController);
                }
                
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// 检查当前位置是否贴近任何正确位置，如果贴近就自动入库。将显示层下移一层
    /// </summary>
    /// <returns>成功入位返回true，否则false</returns>
    bool CheckNearAnyRightPos()
    {
        Vector3 currentPos = transform.position;//v3->v2
        
        //循环所有的位置判断临近
        foreach (var pos in boardManager.allRightPos)
        {
            //已经入位了，不需要进行判断了
            if (currentPos.Equals(pos))
            {
                SetupOrderinlayer((int)PieceDisplayOrder.onBoard);
                return true;
            }
            //靠近某个正确位置，进行自动入库，用的v2的，没准能快点，不考虑z
            if (Vector2.Distance(currentPos, pos) < boardManager.maxAutoMoveDistance)
            {
                if (isInGroup)
                {
                    //在group内时移动所有同组的对象
                    Vector3 offset = pos - currentPos;
                    groupController.MoveAllChild(offset);
                }
                else
                {
                    transform.position = pos;
                }
                SetupOrderinlayer((int)PieceDisplayOrder.onBoard);
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Setups the orderinlayer.
    /// </summary>
    /// <param name="orderinlayer">The orderinlayer.</param>
    void SetupOrderinlayer(int orderinlayer)
    {
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = orderinlayer;
    }
}


