using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Android;

    [Serializable]
    public struct PermissionStruct
    {
        public PermissionUtil.ePermissionKind permission;
        [Range(7, 13)] public int minAndroidVersion;
        
        //Android OS 13 / API-33 (TP1A.220624.014/G981NKSU2HVL3)
        public bool CheckMinAndroidOs()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string os = SystemInfo.operatingSystem.Split(new String[] { "/" },
                StringSplitOptions.RemoveEmptyEntries)[0];
            string tempCurrentOs = String.Empty;
            for (int i = 0; i < os.Length; i++)
            {
                if (char.IsNumber(os[i]))
                    tempCurrentOs += os[i];
            }

            int currentOsVersion = 0;
            if (int.TryParse(tempCurrentOs, out currentOsVersion))
                return currentOsVersion >= minAndroidVersion;
            else
            {
                Debug.LogError("현재 버전을 찾을수 없습니다. " + tempCurrentOs);
                return false;
            }
#elif UNITY_ANDROID && UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    /// <summary>
    /// 어플리케이션에서 퍼미션이 필요하다면 사용
    /// </summary>
    public class PermissionUtil : MonoBehaviour
    {
        public PermissionStruct[] WantedPermissions;

        public enum ePermissionKind
        {
            Camera = 0,
            Microphone,
            CoarseLocation,
            FineLocation,
            ExternalStorageRead,
            ExternalStorageWrite,
            Bluetooth,
            Notification
        }

        private string[] androidPermissionList =
        {
            Permission.Camera, Permission.Microphone, Permission.CoarseLocation, Permission.FineLocation,
            Permission.ExternalStorageRead, Permission.ExternalStorageWrite,"android.permission.BLUETOOTH_CONNECT","android.permission.POST_NOTIFICATIONS"
        };

        private UserAuthorization[] iosPermissionList =
        {
            UserAuthorization.WebCam, UserAuthorization.Microphone
        };
        

        /// <summary>
        /// 권한요청이 필요한지ㅏ를체크함 필요하면 true
        /// </summary>
        /// <returns>권하ㅣㄴ요청이 필요할시 true반환 필요없을시 false반환</returns>
        public bool CheckNecessityPermissions()
        {
            // 한개라도 permission이 제대로 안되있다면 true를 반환함
            for (int i = 0; i < WantedPermissions.Length; i++)
            {
                // 안드로이드가 아니면 ㅍㅊ true를 리턴시킨다.
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (WantedPermissions[i].CheckMinAndroidOs())
                        if (!AndroidPermissionsManager.IsPermissionGranted(androidPermissionList[(int)(WantedPermissions[i].permission)]))
                            return true;
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (iosPermissionList.Length > (int)(WantedPermissions[i].permission))
                        if (!Application.HasUserAuthorization(iosPermissionList[(int)(WantedPermissions[i].permission)]))
                            return true;
                }
                else
                    Debug.LogError($"no supported platform => {Application.platform}");
            }

            return false;
        }

        private int grantedCount = 0;
        private int deniedCount = 0;
        private int deniedDontAskCount = 0;

        /// <summary>
        /// 퍼미션 권한요청이 가도록함 퍼미션이 여러개일경우 콜백은 모든 퍼미션의 요청창이 사라진후 순차적으로 콜백이 드러옴
        /// </summary>
        public void OnGrantButtonPress(UnityAction grantedCallback, UnityAction deniedCallback,
            UnityAction deniedAndDontAskAgainCallback)
        {
            grantedCount = 0;
            deniedCount = 0;
            deniedDontAskCount = 0;

            StartCoroutine(PermissionCallbackProcess(grantedCallback, deniedCallback, deniedAndDontAskAgainCallback));

            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < WantedPermissions.Length; i++)
                {
                    if (!WantedPermissions[i].CheckMinAndroidOs())
                        grantedCount++;
                    else
                    {
                        if (!AndroidPermissionsManager.IsPermissionGranted(
                                androidPermissionList[(int)(WantedPermissions[i].permission)]))
                        {
                            AndroidPermissionsManager.RequestPermission(
                                new[] { androidPermissionList[(int)(WantedPermissions[i].permission)] },
                                new AndroidPermissionCallback(
                                    grantedPermission =>
                                    {
                                        grantedCount++;
                                        // 권한이 승인 되었다.
                                    },
                                    deniedPermission =>
                                    {
                                        deniedCount++;
                                        // 권한이 거절되었다.
                                    },
                                    deniedPermissionAndDontAskAgain =>
                                    {
                                        deniedDontAskCount++;
                                        // 권한이 거절된데다가 다시 묻지마시오를 눌러버렸다.
                                        // 안드로이드 설정창 권한에서 직접 변경 할 수 있다는 팝업을 띄우는 방식을 취해야함. 
                                    }));
                        }
                        else
                            grantedCount++; // 권한이 승인 되었다.
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                for (int i = 0; i < WantedPermissions.Length; i++)
                {
                    if (iosPermissionList.Length > (int)(WantedPermissions[i].permission))
                    {
                        if (!Application.HasUserAuthorization(iosPermissionList[(int)(WantedPermissions[i].permission)]))
                        {
                            UserAuthorization temp = iosPermissionList[(int)(WantedPermissions[i].permission)];
                            var asyncPermission = Application.RequestUserAuthorization(temp);
                            asyncPermission.completed += (operation) =>
                            {
                                if (Application.HasUserAuthorization(temp))
                                    grantedCount++; // 권한이 승인 되었다.
                                else
                                    deniedCount++;
                            };
                        }
                        else
                            grantedCount++; // 권한이 승인 되었다.
                    }
                    else
                        grantedCount++; // 권한이 승인 되었다.
                }
            }
            else
                Debug.LogError($"no supported platform => {Application.platform}");
        }

        IEnumerator PermissionCallbackProcess(UnityAction grantedCallback, UnityAction deniedCallback,
            UnityAction deniedAndDontAskAgainCallback)
        {
            yield return new WaitUntil(() => grantedCount + deniedCount + deniedDontAskCount >= WantedPermissions.Length);

            if (deniedDontAskCount > 0)
                deniedAndDontAskAgainCallback?.Invoke();
            else if (deniedCount > 0)
                deniedCallback?.Invoke();
            else
                grantedCallback?.Invoke();
        }
    }