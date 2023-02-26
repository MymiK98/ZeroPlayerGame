using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//무한 그리드가 있음 각 그리드는 살거나 죽음
//룰 1 : 각 셀은 0,1의 상태를 가진다.
//룰 2 : 각 셀은 주위의 8개의 셀로 상태가 변경된다.
//룰 3 : 주위 셀이 2~3개의 살아있는 셀이 있을떄만 다음턴까지 살아가고 아니면 죽는다.
//룰 4 : 3개의 살아있는 셀이 주변에 있으면 죽었던 셀이 살아난다.
//룰 5 : 한턴 한턴 지나면서 갱신이 되며 한번의 동시의 갱신을 해야함
public struct CellInfo
{
    public float dicKey;
    public int listIdx;
    public int ulongBit;

    public CellInfo(float dicKey, int listIdx, int ulongBit)
    {
        this.dicKey = dicKey;
        this.listIdx = listIdx;
        this.ulongBit = ulongBit;
    }
}

public class LifeCycle : MonoBehaviour
{
    [SerializeField, Tooltip("카메라 컨트롤을 껐다 킬수 있어야함")] private CameraControl cameraControl;
    [SerializeField, Tooltip("LifeCell의 부모 오브젝트")] private Transform lifeCellParent;
    [SerializeField, Tooltip("AroundCell의 부모 오브젝트")] private Transform aroundCellParent;
    [SerializeField, Tooltip("몇 프레임당 셀을 업데이트 할지")] private int frameUpdateCount = 5;
    
    //key는 y축 존재하는 cell
    //list<ulong>는 x축 존재하는곳 bit로 표현 짝수 홀수 번갈아서 양수 음수 x축
    //y는 셀이 등록되는 위치 기준인 0.5, -0.5단위로 저장 x축은 0.5를 뺀 0,1,2단위
    private Dictionary<float, List<ulong>> currentCellDic = new Dictionary<float, List<ulong>>();

    private Dictionary<float, List<ulong>> inspectionCellDic = new Dictionary<float, List<ulong>>();

    [HideInInspector] public bool isLifeCycleUpdateStart = false;
    
    //ulong 비트 개수 64인데 0부터라서 63
    const int uLongBit = 63;
    
    private void Awake()
    {
        isLifeCycleUpdateStart = false;
        cameraControl.IsControl = true;
        
        Message.AddListener<Msg_GroundBatchCell>(msg_GroundBatchCell);
    }

    private void OnDestroy()
    {
        Message.RemoveListener<Msg_GroundBatchCell>(msg_GroundBatchCell);
    }

    void msg_GroundBatchCell(Msg_GroundBatchCell data)
    {
        if (data.touchState == Msg_GroundBatchCell.eGroundTouchState.Add)
        {
            data.cellObject.transform.SetParent(lifeCellParent);
            AddCellDic(data.cellX, data.cellY);
        }
        else if (data.touchState == Msg_GroundBatchCell.eGroundTouchState.Delete)
            RemoveCellDic(data.cellX, data.cellY);
    }

    void AddCellDic(float x,float y)
    {
        if (currentCellDic == null)
            currentCellDic = new Dictionary<float, List<ulong>>();

        bool isMinus = 0 > x;
        int tempX = Mathf.FloorToInt(Mathf.Abs(x)); 
        
        if (currentCellDic.ContainsKey(y))
        {
            if (currentCellDic[y] == null)
                currentCellDic[y] = new List<ulong>();

            listAddProcess();
        }
        else
        {
            currentCellDic.Add(y,new List<ulong>());

            listAddProcess();
        }

        void listAddProcess()
        {
            if (currentCellDic[y].Count < (tempX/uLongBit)+(isMinus?2:1))
            {
                int lessCount = (tempX / uLongBit) + (isMinus ? 2 : 1) - currentCellDic[y].Count;
                for (int i = 0; i < lessCount; i++)
                {
                    currentCellDic[y].Add(0);
                }
            }

            currentCellDic[y][(tempX / uLongBit) + (isMinus ? 1 : 0)] += (ulong)1 << (tempX % uLongBit);
        }
    }

    void RemoveCellDic(float x,float y)
    {
        if (!GetCellDic(x,y))
            return;
        
        if (currentCellDic == null)
            return;

        if (!currentCellDic.ContainsKey(y))
            return;
        
        if (currentCellDic[y] == null)
            return;
        
        bool isMinus = 0 > x;
        int tempX = Mathf.FloorToInt(Mathf.Abs(x));

        if (currentCellDic[y].Count < (tempX / uLongBit) + (isMinus ? 2 : 1))
            return;

        currentCellDic[y][(tempX / uLongBit) + (isMinus ? 1 : 0)] -= (ulong)1 << (tempX % uLongBit);
    }
    
    bool GetCellDic(float x,float y)
    {
        if (currentCellDic == null)
            return false;

        if (!currentCellDic.ContainsKey(y))
            return false;
        
        if (currentCellDic[y] == null)
            return false;
        
        bool isMinus = 0 > x;
        int tempX = Mathf.FloorToInt(Mathf.Abs(x));

        if (currentCellDic[y].Count < (tempX / uLongBit) + (isMinus ? 2 : 1))
            return false;

        return EqualBit(currentCellDic[y][(tempX / uLongBit) + (isMinus ? 1 : 0)],(ulong) 1 << (tempX % uLongBit));
    }

    bool ValidCheckCellAround(float dicKey, int listIdx, int ulongBit)
    {
        int aroundCellLive = 0;

        bool currentCell = false;
        if (currentCellDic.ContainsKey(dicKey))
            if (currentCellDic[dicKey] != null && currentCellDic[dicKey].Count > listIdx)
                currentCell = EqualBit(currentCellDic[dicKey][listIdx], (ulong) 1 << ulongBit);

        InspectionProcess(dicKey - 1.0f);
        InspectionProcess(dicKey);
        InspectionProcess(dicKey + 1.0f);

        aroundCellLive = currentCell ? aroundCellLive--: aroundCellLive++;

        if (currentCell)
        {
            aroundCellLive--;
            return aroundCellLive == 2 || aroundCellLive == 3;
        }
        else
            return aroundCellLive == 3; 

         void InspectionProcess(float tempDic)
        {
            if (currentCellDic.ContainsKey(tempDic))
            {
                if (currentCellDic[tempDic] != null && currentCellDic[tempDic].Count > listIdx)
                {
                    if (ulongBit == 0)
                    {
                        if (listIdx == 0)
                        {
                            if (currentCellDic[tempDic].Count >= 2)
                                EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][1], 1));
                        }
                        else if (listIdx == 1)
                            EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][0], 1));
                        else
                            EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx - 2], 1UL));

                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit));
                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit + 1));
                    }
                    else if (ulongBit == uLongBit)
                    {
                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit - 1));
                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit));

                        if (currentCellDic[tempDic].Count > listIdx + 2)
                            EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx + 2], (ulong) 1 << 1));
                    }
                    else
                    {
                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit - 1));
                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit));
                        EqualAroundCellAdd(EqualBit(currentCellDic[tempDic][listIdx], (ulong) 1 << ulongBit + 1));
                    }
                }
            }
        }
        
        void EqualAroundCellAdd(bool equealBit)
        {
            if (equealBit)
                aroundCellLive++;
        }
    }

    void AroundCellInspection(float dicKey, int listIdx, int ulongBit)
    {
        if (inspectionCellDic == null)
            inspectionCellDic = new Dictionary<float, List<ulong>>();

        InspectionProcess(dicKey - 1.0f);
        InspectionProcess(dicKey);
        InspectionProcess(dicKey + 1.0f);
        
        void InspectionProcess(float tempDic)
        {
            if (inspectionCellDic.ContainsKey(tempDic))
            {
                if (inspectionCellDic[tempDic] == null)
                    inspectionCellDic[tempDic] = new List<ulong>();
            }
            else
                inspectionCellDic.Add(tempDic, new List<ulong>());

            if (inspectionCellDic[tempDic].Count <= listIdx)
            {
                int tempAdd = listIdx - inspectionCellDic[tempDic].Count + 1;
                for (int i = 0; i < tempAdd; i++)
                {
                    inspectionCellDic[tempDic].Add(0);
                }
            }

            if (ulongBit == 0)
            {
                if (listIdx == 0)
                {
                    if (inspectionCellDic[tempDic].Count < 2)
                        inspectionCellDic[tempDic].Add(0);

                    AddInspectionDic(tempDic, 1, 0);
                }
                else if (listIdx == 1)
                    AddInspectionDic(tempDic, 0, 0);
                else
                    AddInspectionDic(tempDic, listIdx - 2, uLongBit);

                AddInspectionDic(tempDic, listIdx, ulongBit);
                AddInspectionDic(tempDic, listIdx, ulongBit + 1);
            }
            else if (ulongBit == uLongBit)
            {
                AddInspectionDic(tempDic, listIdx, ulongBit - 1);
                AddInspectionDic(tempDic, listIdx, ulongBit);

                if (inspectionCellDic[tempDic].Count <= listIdx + 2)
                {
                    int tempAdd = listIdx - inspectionCellDic[tempDic].Count + 3;
                    for (int i = 0; i < tempAdd; i++)
                    {
                        inspectionCellDic[tempDic].Add(0);
                    }
                }

                AddInspectionDic(tempDic, listIdx + 2, 0);
            }
            else
            {
                AddInspectionDic(tempDic, listIdx, ulongBit - 1);
                AddInspectionDic(tempDic, listIdx, ulongBit);
                AddInspectionDic(tempDic, listIdx, ulongBit + 1);
            }
        }

        void AddInspectionDic(float tempKey, int tempIdx, int tempBit)
        {
            if (!EqualBit(inspectionCellDic[tempKey][tempIdx], (ulong) 1 << tempBit))
                inspectionCellDic[tempKey][tempIdx] += (ulong) 1 << tempBit;
        }
    }

    bool EqualBit(ulong a,ulong b)
    {
        return (a & b) != 0;
    }

    Vector3 InfoToPosition(float dicKey, int listIdx, int ulongBit, float zPos)
    {
        return new Vector3(((float) listIdx % 2 == 0 ? +1 : -1) * (((uLongBit + 1) * ((float) listIdx % 2 == 0 ? listIdx : listIdx - 1)) + ulongBit + .5f), dicKey, zPos);
    }

    public void LifeCellObjectDestroy(bool isDicClear = false)
    {
        if (isDicClear)
            currentCellDic.Clear();
        
        for (int i = lifeCellParent.childCount-1; i >= 0; i--)
        {
            ObjectPoolManage.Instance.PushObject("LifeCell", lifeCellParent.GetChild(i).gameObject);
        }
    }
    
    public void AroundCellObjectDestroy()
    {
        for (int i = aroundCellParent.childCount-1; i >= 0; i--)
        {
            ObjectPoolManage.Instance.PushObject("AroundCell", aroundCellParent.GetChild(0).gameObject);
        }
    }

    private void Update()
    {
        if (!isLifeCycleUpdateStart)
            return;

        updateCount++;

        if (updateCount >= frameUpdateCount)
        {
            updateCount = 0;
            FrameProgress();
        }
    }

    private int updateCount = 0;
    
    public void StartProgressFrame()
    {
        cameraControl.IsControl = false;
        isLifeCycleUpdateStart = true;

        updateCount = 0;
        
        FrameProgress();
    }
    
    public void EndProgressFrame()
    {
        cameraControl.IsControl = true;
        isLifeCycleUpdateStart = false;

        AroundCellObjectDestroy();
    }

    void FrameProgress()
    {
        inspectionCellDic.Clear();
        
        LifeCellObjectDestroy();
        AroundCellObjectDestroy();
        
        foreach (var dicPair in currentCellDic)
        {
            for (int i = 0; i < dicPair.Value.Count; i++)
            {
                for (int j = 0; j <= uLongBit; j++)
                {
                    if (EqualBit(dicPair.Value[i], (ulong) 1 << j))
                        AroundCellInspection(dicPair.Key,i,j);
                }
            }
        }

        List<CellInfo> tempNextCell = new List<CellInfo>();
        foreach (var dicPair in inspectionCellDic)
        {
            for (int i = 0; i < dicPair.Value.Count; i++)
            {
                for (int j = 0; j <= uLongBit; j++)
                {
                    if (EqualBit(dicPair.Value[i], (ulong) 1 << j))
                    {
                        if (ValidCheckCellAround(dicPair.Key,i,j))
                        {
                            tempNextCell.Add(new CellInfo(dicPair.Key,i,j));
                            GameObject selectCell = ObjectPoolManage.Instance.PopObject("LifeCell", lifeCellParent);
                            selectCell.transform.position = InfoToPosition(dicPair.Key, i, j, 0);
                            Debug.Log(selectCell.transform.position);
                        }
                    }
                }
            }
        }
        
        currentCellDic.Clear();
        for (int i = 0; i < tempNextCell.Count; i++)
        {
            AddCellDic(((float) tempNextCell[i].listIdx % 2 == 0 ? +1 : -1) * (((uLongBit + 1) * ((float) tempNextCell[i].listIdx % 2 == 0 ? tempNextCell[i].listIdx : tempNextCell[i].listIdx - 1)) + tempNextCell[i].ulongBit + .5f), tempNextCell[i].dicKey);
        }

        cameraControl.SelectRectCamera(SelectRectProcess());
    }

    CameraControl.SelectRect SelectRectProcess()
    {
        CameraControl.SelectRect dataRect = new CameraControl.SelectRect();
        foreach (var data in currentCellDic)
        {
            if (dataRect.up < data.Key) dataRect.up = data.Key;
            else if (dataRect.down > data.Key) dataRect.down = data.Key;

            if (data.Value.Count >= 2)
            {
                //짝수
                if ((data.Value.Count - 1) % 2 == 0)
                {
                    if (dataRect.right < RepeatBitProcess(data.Value.Count - 1)) dataRect.right = RepeatBitProcess(data.Value.Count - 1);
                    else if (dataRect.left > RepeatBitProcess(data.Value.Count - 2)) dataRect.left = RepeatBitProcess(data.Value.Count - 2);
                }
                //홀수
                else
                {
                    if (dataRect.right < RepeatBitProcess(data.Value.Count - 2)) dataRect.right = RepeatBitProcess(data.Value.Count - 2);
                    else if (dataRect.left > RepeatBitProcess(data.Value.Count - 1)) dataRect.left = RepeatBitProcess(data.Value.Count - 1);
                }
            }
            else if (data.Value.Count == 1)
            {
                if (dataRect.right < RepeatBitProcess(data.Value.Count - 1)) dataRect.right = RepeatBitProcess(data.Value.Count - 1);
                else if (dataRect.left > 0) dataRect.left = 0;
            }

            float RepeatBitProcess(int idx)
            {
                if (data.Value[idx] == 0)
                {
                    if (idx-2 > 0) return RepeatBitProcess(idx - 2);
                    else return 0;
                }
                else
                {
                    for (int i = uLongBit; i >= 0; i--)
                    {
                        if (EqualBit(data.Value[idx],(ulong) 1 << i))
                            return ((float) idx % 2 == 0 ? +1 : -1) * (((uLongBit + 1) * ((float) idx % 2 == 0 ? idx : idx - 1)) + i + .5f);
                    }
                    
                    if (idx-2 > 0) return RepeatBitProcess(idx - 2);
                    else return 0;
                }
            }
        }

        return dataRect;
    }

    public void AroundCellTestFrame()
    {
        inspectionCellDic.Clear();
        foreach (var dicPair in currentCellDic)
        {
            for (int i = 0; i < dicPair.Value.Count; i++)
            {
                for (int j = 0; j <= uLongBit; j++)
                {
                    if (EqualBit(dicPair.Value[i],(ulong)1<<j))
                        AroundCellInspection(dicPair.Key,i,j);
                }
            }
        }
        foreach (var dicPair in inspectionCellDic)
        {
            for (int i = 0; i < dicPair.Value.Count; i++)
            {
                for (int j = 0; j <= uLongBit; j++)
                {
                    if (EqualBit(dicPair.Value[i], (ulong) 1 << j))
                    {
                        GameObject selectCell = ObjectPoolManage.Instance.PopObject("AroundCell", aroundCellParent);
                        selectCell.transform.position = InfoToPosition(dicPair.Key, i, j, 0);
                    }
                }
            }
        }
    }
    
    public void CurrentCellTestFrame()
    {
        foreach (var dicPair in currentCellDic)
        {
            for (int i = 0; i < dicPair.Value.Count; i++)
            {
                for (int j = 0; j <= uLongBit; j++)
                {
                    if (EqualBit(dicPair.Value[i], (ulong) 1 << j))
                    {
                        GameObject selectCell = ObjectPoolManage.Instance.PopObject("AroundCell", aroundCellParent);
                        selectCell.transform.position = InfoToPosition(dicPair.Key, i, j, 0);
                    }
                }
            }
        }
    }
}
