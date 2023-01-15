using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoadObjectInfoStruct
{
    public string objectResourcePath;
    public int objectLoadCount = 1;
}

public class ObjectFoolItem
{
    public GameObject itemObject;
    public Transform foldObject;
    public List<GameObject> poolList = new List<GameObject>();
    public List<GameObject> pushList;
}

public class ObjectPoolManage : MonoSingleton<ObjectPoolManage>
{
    [SerializeField] private List<LoadObjectInfoStruct> objectList;
    private Dictionary<string, ObjectFoolItem> objectItemList;

    protected override void Init()
    {
        if (objectList != null && objectList.Count > 0)
        {
            objectItemList = new Dictionary<string, ObjectFoolItem>();
            for (int i = 0; i < objectList.Count; i++)
            {
                GameObject itempObject =Resources.Load<GameObject>(objectList[i].objectResourcePath);
                if (itempObject != null)
                {
                    Transform foldObject = new GameObject(objectList[i].objectResourcePath).transform;
                    foldObject.SetParent(this.gameObject.transform);
                    foldObject.position = Vector3.zero;
                    foldObject.rotation = Quaternion.identity;
                    foldObject.localScale = Vector3.one;

                    objectItemList.Add(objectList[i].objectResourcePath, new ObjectFoolItem());

                    objectItemList[objectList[i].objectResourcePath].itemObject = itempObject;
                    objectItemList[objectList[i].objectResourcePath].foldObject = foldObject;
                    for (int j = 0; j < objectList[i].objectLoadCount; j++)
                    {
                        GameObject temp = Instantiate(objectItemList[objectList[i].objectResourcePath].itemObject,
                            Vector3.zero, Quaternion.identity, foldObject);
                        temp.name = objectList[i].objectResourcePath + "_" + (j + 1);
                        temp.SetActive(false);

                        objectItemList[objectList[i].objectResourcePath].poolList.Add(temp);
                    }
                }
                else
                    Debug.LogError(objectList[i].objectResourcePath+"가 Resources.Load힐수 없습니다.");
            }
        }
    }

    protected override void Release()
    {
    }

    /// <summary>
    /// object path가 pool list에 있는지 확인
    /// </summary>
    /// <param name="path"></param>
    /// <returns>path가 list에 있음</returns>
    public bool CheckObjectPath(string path)
    {
        return objectItemList.ContainsKey(path);
    }

    /// <summary>
    /// path를 넣어서 Object를 받아옴
    /// </summary>
    /// <param name="popObjectPath">오브젝트를 받을 resources path</param>
    /// <param name="popObjecParent">오브젝트의 부모 trasform</param>
    /// <returns></returns>
    public GameObject PopObject(string popObjectPath,Transform popObjecParent)
    {
        if (objectItemList.ContainsKey(popObjectPath))
        {
            if (objectItemList[popObjectPath].poolList != null && objectItemList[popObjectPath].poolList.Count > 0)
            {
                if (objectItemList[popObjectPath].pushList == null)
                    objectItemList[popObjectPath].pushList = new List<GameObject>();

                GameObject temp = null;
                if (objectItemList[popObjectPath].poolList.Count <= 0)
                {
                    temp = Instantiate(objectItemList[popObjectPath].itemObject,Vector3.zero, Quaternion.identity, objectItemList[popObjectPath].foldObject);
                    temp.name = popObjectPath + "_" + (objectItemList[popObjectPath].pushList.Count+1);
                    
                    objectItemList[popObjectPath].pushList.Add(temp);
                }
                else
                {
                    temp = objectItemList[popObjectPath].poolList[0];
                    
                    objectItemList[popObjectPath].poolList.RemoveAt(0);
                    objectItemList[popObjectPath].pushList.Add(temp);
                }
                
                temp.SetActive(true);
                temp.transform.SetParent(popObjecParent);

                return temp;
            }
        }

        return null;
    }

    /// <summary>
    /// 사용한 object를 반환
    /// </summary>
    /// <param name="pushObjectPath">어떤 path의 오브젝트 였는지</param>
    /// <param name="pushObject">반환할 object</param>
    public void PushObject(string pushObjectPath,GameObject pushObject)
    {
        string path = pushObjectPath;
        if (!objectItemList.ContainsKey(path))
        {
            path = pushObject.name.Split('_', StringSplitOptions.RemoveEmptyEntries)[0];
            if (objectItemList.ContainsKey(path))
            {
                Debug.LogError(pushObjectPath+"가 오브젝트풀에 없는 아이템입니다.");
                return;   
            }
        }

        if (objectItemList[path].pushList.Contains(pushObject))
        {
            pushObject.SetActive(false);
            pushObject.transform.SetParent(objectItemList[path].foldObject);
            
            objectItemList[path].poolList.Add(pushObject);
            objectItemList[path].pushList.Remove(pushObject);
        }
        else
            Destroy(pushObject);
    }
}
