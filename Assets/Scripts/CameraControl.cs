using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//카메라를 이동시키기
//카메라를 이용 클릭한 부분이 어떤 그리드인지 체크
public class CameraControl : MonoBehaviour
{
    [Tooltip("카메라 상하좌우 이동속도")] public float fourDirectionSpeed = 10;
    [Tooltip("카메라 앞뒤 이동속도")] public float zDirectionSpeed = 10;
    [Tooltip("카메라 디폴트 오소크래픽 사이즈")] public float defaultOrthographicSize = 10;

    private Vector3 defaultPosition;

    private bool isControl = true;
    public bool IsControl { get { return isControl; } set { isControl = value; } }

    #region GUI Property

    GUIStyle style = new GUIStyle();

    float rect_pos_x = 5f;
    float rect_pos_y = 5f;
    float w = Screen.width;
    float h = 20f;

    #endregion

    private void Start()
    {
        #region GUI Init

        style.normal.textColor = Color.green;
        style.fontSize = 19;

        #endregion

        if (Camera.main != null)
        {
            defaultPosition = Camera.main.transform.position;
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = defaultOrthographicSize;
            Camera.main.aspect = 1;
        }
    }

    private void Update()
    {
        if (Camera.main == null)
            return;

        #region Camera Control

        if (isControl)
        {
            if (Input.GetKey(KeyCode.W))
                Camera.main.transform.position += Vector3.up * Time.deltaTime * fourDirectionSpeed;
            else if (Input.GetKey(KeyCode.A))
                Camera.main.transform.position += Vector3.left * Time.deltaTime * fourDirectionSpeed;
            else if (Input.GetKey(KeyCode.S))
                Camera.main.transform.position += Vector3.down * Time.deltaTime * fourDirectionSpeed;
            else if (Input.GetKey(KeyCode.D))
                Camera.main.transform.position += Vector3.right * Time.deltaTime * fourDirectionSpeed;

            if (Input.GetKey(KeyCode.Q) && Camera.main.orthographicSize > defaultOrthographicSize)
                Camera.main.orthographicSize -= Time.deltaTime * zDirectionSpeed;
            else if (Input.GetKey(KeyCode.E)) Camera.main.orthographicSize += Time.deltaTime * zDirectionSpeed;
            else if (Camera.main.orthographicSize < defaultOrthographicSize)
                Camera.main.orthographicSize = defaultOrthographicSize;

            if (Input.GetKey(KeyCode.Space))
            {
                Camera.main.transform.position = defaultPosition;
                Camera.main.orthographicSize = defaultOrthographicSize;
            }
        }

        #endregion

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit rayHit = new RaycastHit();
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit,
                    Mathf.Abs(Camera.main.transform.position.z)))
            {
                Vector3 cameraPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                GameObject selectCell = ObjectPoolManage.Instance.PopObject("LifeCell", null);
                selectCell.transform.position = new Vector3(Mathf.FloorToInt(cameraPoint.x) + .5f,
                    Mathf.FloorToInt(cameraPoint.y) + .5f, 0);
                Message.Send(new Msg_GroundBatchCell(Mathf.FloorToInt(cameraPoint.x), Mathf.FloorToInt(cameraPoint.y),
                    selectCell, Msg_GroundBatchCell.eGroundTouchState.Add));
            }
            else
            {
                Message.Send(new Msg_GroundBatchCell(
                    Mathf.FloorToInt(rayHit.collider.gameObject.transform.position.x - .5f),
                    Mathf.FloorToInt(rayHit.collider.gameObject.transform.position.y - .5f), null,
                    Msg_GroundBatchCell.eGroundTouchState.Delete));
                Destroy(rayHit.collider.gameObject);
            }
        }
    }

    private void OnGUI()
    {
        if (!isControl)
            return;
        
        if (Camera.main != null)
        {
            GUI.Label(new Rect(rect_pos_x, rect_pos_y, w, h), "WSAD = 상하좌우", style);
            GUI.Label(new Rect(rect_pos_x, rect_pos_y + h * 1, w, h), "QE = 앞뒤", style);
            GUI.Label(new Rect(rect_pos_x, rect_pos_y + h * 2, w, h), "space = 위치 리셋", style);
        }
        else
            GUI.Label(new Rect(rect_pos_x, rect_pos_y, w, h), "메인 카메라가 없습니다.", style);
    }
}