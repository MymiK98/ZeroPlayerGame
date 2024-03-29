using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//무한 그리드 표현하기
//카메라의 orthographicSize를 2배하여 1의 길이를 스케일 늘리고 마테리얼 타일링을 늘리면 1의 사이즈를 표현가능
//가로세로는 카메라의 aspect로 조절
[RequireComponent(typeof(Renderer),typeof(Transform))]
public class InfinityGrid : MonoBehaviour
{
    private Transform gridTransform;
    private Material gridMaterial;

    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        gridTransform = this.transform;
        gridMaterial = gridTransform.GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (mainCamera == null || gridMaterial == null)
            return;
        
        //카메라에 맞추어 그리드의 크기도 변경
        gridTransform.localScale = new Vector2( mainCamera.aspect*mainCamera.orthographicSize * 2+2,mainCamera.orthographicSize * 2+2);
        gridMaterial.mainTextureScale = new Vector2(mainCamera.aspect*mainCamera.orthographicSize * 2+2,mainCamera.orthographicSize * 2+2);
    }

    private void LateUpdate()
    {
        if (mainCamera == null || gridMaterial == null)
            return;
        
        //그리드에 셀이 생성됬는데 배율을 바꾸거나 움직이면그리드와 셀이 안맞으 그유격을 맞춤
        float temp = mainCamera.orthographicSize % (float)1;
        gridTransform.position = new Vector2( mainCamera.transform.position.x + mainCamera.aspect*(temp - 0.5f),mainCamera.transform.position.y+(temp - 0.5f));
        gridMaterial.mainTextureOffset = new Vector2(mainCamera.transform.position.x%(float)1,mainCamera.transform.position.y%(float)1);
    }
}
