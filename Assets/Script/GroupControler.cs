/*********************************************************************************
 *Copyright(C) 2015 by FengNongtheWindCoder
 *All rights reserved.
 *FileName:     GroupControler.cs
 *Author:       FengNongtheWindCoder
 *Version:      1.0
 *UnityVersion：5.3.0f4
 *Date:         2016-01-04
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections.Generic;
public class GroupControler : MonoBehaviour {

    public enum GroupMoveState { idle, start, moving, finish };//表示group的运动状态，空闲，开始，移动中，移动结束
    public GroupMoveState moveState = GroupMoveState.idle;//group当前的运动状态
    //记录所有child的控制
    List<PieceController> child = new List<PieceController>();
    
	
	// Update is called once per frame
	void LateUpdate () {
        switch (moveState)
        {
            case GroupMoveState.start:
                moveState = GroupMoveState.moving;
                break;
            case GroupMoveState.finish:
                moveState = GroupMoveState.idle;
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Creates the piece group.
    /// </summary>
    /// <param name="original">The original.</param>
    /// <param name="neighbour">The neighbour.</param>
    public static void CreatePieceGroup(PieceController original , PieceController neighbour)
    {
        //创建一个group对象作为父节点，其坐标为0
        GameObject groupPiece = new GameObject();
        groupPiece.transform.SetParent(original.transform.parent);
        groupPiece.transform.position = Vector3.zero;
        groupPiece.AddComponent<GroupControler>();
        GroupControler controller = groupPiece.GetComponent<GroupControler>();
        //将这两个对象的父节点都修改为这个
        original.transform.SetParent(groupPiece.transform);
        neighbour.transform.SetParent(groupPiece.transform);
        //修改这两个对象的标志位
        original.isInGroup = true;
        neighbour.isInGroup = true;
        original.groupController = controller;
        neighbour.groupController = controller;
        //记录这两个对象
        controller.child.Add(original);
        controller.child.Add(neighbour);
    }
    /// <summary>
    /// Adds the child.
    /// </summary>
    /// <param name="newChild">The new child.</param>
    public void AddChild(PieceController newChild)
    {
        newChild.isInGroup = true;
        newChild.groupController = this;
        newChild.transform.SetParent(this.transform);
        child.Add(newChild);
    }
    /// <summary>
    /// Changes the group then selfdestroy.将自己的所有child转移到另一个group，进行位移，然后删除自己
    /// </summary>
    /// <param name="changeTo">The change to.</param>
    /// <param name="moveOffset">The move offset.</param>
    public void ChangeGroupThenSelfdestroy(GroupControler changeTo,Vector3 moveOffset) {
        for (int i = 0; i < child.Count; i++)
        {
            child[i].transform.position = child[i].transform.position + moveOffset;
            changeTo.AddChild(child[i]);
        }
        Destroy(gameObject);
    }
    /// <summary>
    /// Moves all child.所有的child都进行移动，距离是offset
    /// </summary>
    /// <param name="offset">The offset.</param>
    public void MoveAllChild(Vector3 offset) {
        for (int i = 0; i < child.Count; i++)
        {
            child[i].transform.position = child[i].transform.position + offset;
        }
    }
    public void StartMove() {
        moveState = GroupMoveState.start;
    }
    public void FinishMove()
    {
        moveState = GroupMoveState.finish;
    }
}
