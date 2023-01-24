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
public class LifeCycle : MonoBehaviour
{
    private void Awake()
    {
        Message.AddListener<Msg_GroundBatchCell>(msg_GroundBatchCell);
    }

    private void OnDestroy()
    {
        Message.RemoveListener<Msg_GroundBatchCell>(msg_GroundBatchCell);
    }

    void msg_GroundBatchCell(Msg_GroundBatchCell data)
    {
    }
}
