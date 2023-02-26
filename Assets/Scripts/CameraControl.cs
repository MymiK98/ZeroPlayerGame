using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private Camera mainCamera;
    
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

        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            defaultPosition = mainCamera.transform.position;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = defaultOrthographicSize;
            mainCamera.aspect = 1;
        }
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        if (!isControl)
            return;

        #region Camera Control

        if (Input.GetKey(KeyCode.W))
            mainCamera.transform.position += Vector3.up * Time.deltaTime * fourDirectionSpeed;
        else if (Input.GetKey(KeyCode.A))
            mainCamera.transform.position += Vector3.left * Time.deltaTime * fourDirectionSpeed;
        else if (Input.GetKey(KeyCode.S))
            mainCamera.transform.position += Vector3.down * Time.deltaTime * fourDirectionSpeed;
        else if (Input.GetKey(KeyCode.D))
            mainCamera.transform.position += Vector3.right * Time.deltaTime * fourDirectionSpeed;

        if (Input.GetKey(KeyCode.Q) && mainCamera.orthographicSize > defaultOrthographicSize)
            mainCamera.orthographicSize -= Time.deltaTime * zDirectionSpeed;
        else if (Input.GetKey(KeyCode.E)) mainCamera.orthographicSize += Time.deltaTime * zDirectionSpeed;
        else if (mainCamera.orthographicSize < defaultOrthographicSize)
            mainCamera.orthographicSize = defaultOrthographicSize;

        if (Input.GetKey(KeyCode.Space))
        {
            mainCamera.transform.position = defaultPosition;
            mainCamera.orthographicSize = defaultOrthographicSize;
        }

        #endregion

        if (Input.GetMouseButton(0))
        {
#if UNITY_ANDROID
                if (!EventSystem.current.IsPointerOverGameObject(0))
#else
            if (!EventSystem.current.IsPointerOverGameObject())
#endif
            {
                RaycastHit rayHit = new RaycastHit();
                if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out rayHit,
                        Mathf.Abs(mainCamera.transform.position.z)))
                {
                    Vector3 cameraPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    GameObject selectCell = ObjectPoolManage.Instance.PopObject("LifeCell", null);
                    selectCell.transform.position = new Vector3(Mathf.FloorToInt(cameraPoint.x) + .5f,
                        Mathf.FloorToInt(cameraPoint.y) + .5f, 0);
                    Message.Send(new Msg_GroundBatchCell(selectCell.transform.position.x,
                        selectCell.transform.position.y,
                        selectCell, Msg_GroundBatchCell.eGroundTouchState.Add));
                }
                else
                {
                    Message.Send(new Msg_GroundBatchCell(rayHit.collider.gameObject.transform.position.x,
                        rayHit.collider.gameObject.transform.position.y, null,
                        Msg_GroundBatchCell.eGroundTouchState.Delete));
                    ObjectPoolManage.Instance.PushObject("LifeCell", rayHit.collider.gameObject);
                }
            }
        }
    }

    public struct SelectRect
    {
        public float up;
        public float down;
        public float right;
        public float left;
    }
    
    /// <summary>
    /// ㅌㅡㄱ정 사이즈로 카메라 위치 및 확재 변경
    /// </summary>
    public void SelectRectCamera(SelectRect selectRect)
    {
        float xDis =  Mathf.Abs(selectRect.right - selectRect.left) * 0.5f;
        float yDis = Mathf.Abs(selectRect.up - selectRect.down) * 0.5f;

        Vector3 tempPos = new Vector3();
        tempPos.z = mainCamera.transform.position.z;
        tempPos.x = selectRect.right >= selectRect.left ? selectRect.right - xDis : selectRect.left - xDis;
        tempPos.y = selectRect.up >= selectRect.down ? selectRect.up - yDis : selectRect.down - yDis;
        
        mainCamera.transform.position = tempPos;

        mainCamera.orthographicSize = xDis > yDis? xDis: yDis;
    }

    private void OnGUI()
    {
        if (!isControl)
            return;
        
        if (mainCamera != null)
        {
            GUI.Label(new Rect(rect_pos_x, rect_pos_y, w, h), "WSAD = 상하좌우", style);
            GUI.Label(new Rect(rect_pos_x, rect_pos_y + h * 1, w, h), "QE = 앞뒤", style);
            GUI.Label(new Rect(rect_pos_x, rect_pos_y + h * 2, w, h), "space = 위치 리셋", style);
        }
        else
            GUI.Label(new Rect(rect_pos_x, rect_pos_y, w, h), "메인 카메라가 없습니다.", style);
    }
}