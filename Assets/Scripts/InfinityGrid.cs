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

    private void Start()
    {
        gridTransform = this.transform;
        gridMaterial = gridTransform.GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (Camera.main == null || gridMaterial == null)
            return;
        
        gridTransform.localScale = new Vector2(Camera.main.aspect*Camera.main.orthographicSize * 2+2,Camera.main.orthographicSize * 2+2);
        gridMaterial.mainTextureScale = new Vector2(Camera.main.aspect*Camera.main.orthographicSize * 2+2,Camera.main.orthographicSize * 2+2);
    }

    private void LateUpdate()
    {
        if (Camera.main == null || gridMaterial == null)
            return;
        
        float temp = Camera.main.orthographicSize % (float)1;
        temp = temp >= 0.5f ? temp - 0.5f : Mathf.Abs(0.5f - temp);
        gridTransform.position = new Vector2(Camera.main.aspect*temp,temp);
    }
}
