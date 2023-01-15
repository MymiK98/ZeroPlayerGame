using System.Collections;
using UnityEngine;

/// <summary>
/// 싱글톤 클래스 이다. 매니저클래스에서망ㄴ 사용하길 권장한다.
/// </summary>
/// <typeparam name="T">싱글톤으로써 인스턴스화할 클래스를 입력 ex) WebServerNetworkManager : MonoSingleton[WebServerNetworkManager] </typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                {
                    Debug.LogFormat("Create Singleton-instance. - Begin - Type: {0}", typeof(T).FullName);

                    var obj = new GameObject(typeof(T).ToString());
                    _instance = obj.AddComponent<T>(); // 이 때 Awake() 호출됨

                    Debug.LogFormat("Create Singleton-instance. - End - Type: {0}, InstanceID: {1}", typeof(T).FullName,
                        _instance.GetInstanceID());

                    // Problem during the creation, this should not happen
                    if (_instance == null)
                    {
                        Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                    }
                }
                else
                {
                    Debug.LogFormat("Find Singleton-instance. Type: {0}, InstanceID: {1}", typeof(T).FullName,
                        _instance.GetInstanceID());
                    _instance._Init();
                }
            }

            return _instance;
        }
    }

    public static bool isAlive
    {
        get { return (_instance != null); }
    }


    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (_instance == null)
        {
            _instance = this as T;
            Debug.LogFormat("Awake Singleton-instance. - OK - Type: {0}, InstanceID: {1}", typeof(T).FullName,
                _instance.GetInstanceID());

            _instance._Init();
        }
        else if (_instance != this)
        {
            Debug.LogFormat("Awake Singleton-instance. - Duplicate - Type: {0}, InstanceID: {1}, This: {2}",
                typeof(T).FullName, _instance.GetInstanceID(), this.GetInstanceID());
            Destroy(gameObject);
        }
    }

    private void _Init()
    {
        Init();
    }

    private void _Release()
    {
        Release();
    }

    // This function is called when the instance is used the first time
    // Put all the initializations you need here, as you would do in Awake
    protected virtual void Init()
    {
        /* BLANK */
    }

    protected virtual void Release()
    {
        /* BLANK */
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            Debug.LogFormat("Destroy : {0}, InstanceID: {1}", typeof(T).FullName, _instance.GetInstanceID());

            _instance._Release();
            _instance = null;
        }
    }
}