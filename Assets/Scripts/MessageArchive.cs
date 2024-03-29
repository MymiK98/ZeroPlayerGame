using UnityEngine;

//화면을 클릭해서 셀을 배치함 배치한 오브젝트와 위치를 전달
public class Msg_GroundBatchCell : Message
{
    public float cellX;
    public float cellY;
    public GameObject cellObject;
    public eGroundTouchState touchState;
    
    public enum eGroundTouchState
    {
        Add,Delete
    } 
    
    public Msg_GroundBatchCell(float cellX,float cellY,GameObject cellObject,eGroundTouchState touchState)
    {
        this.cellX = cellX;
        this.cellY = cellY;
        this.cellObject = cellObject;
        this.touchState = touchState;
    }
}