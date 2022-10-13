using System;                           // for Action
using System.Runtime.InteropServices;   // for DllImport
using System.Threading;                 // for SynchronizationContext
using UnityEngine;                      // for Application

namespace MyUtility
{
    public class ATTUtili
    {
#if UNITY_IOS
    private const string DLL_NAME = "__Internal";

    /**
     * ATT承認状態を取得(同期)
     **/
    [DllImport(DLL_NAME)]
    private static extern int Sge_Att_getTrackingAuthorizationStatus();
    public static int GetTrackingAuthorizationStatus()
    {
        if (Application.isEditor)
        {
            return -1;
        }
        return Sge_Att_getTrackingAuthorizationStatus();
    }


    /**
     * ATT承認を要求(非同期)
     **/
    [DllImport(DLL_NAME)]
    private static extern void Sge_Att_requestTrackingAuthorization(OnCompleteCallback callback);

    private delegate void OnCompleteCallback(int status);
    private static SynchronizationContext _context;
    private static Action<int> _onComplete;

    public static void RequestTrackingAuthorization(Action<int> onComplete)
    {
        if (Application.isEditor)
        {
            // 呼出元のActionの引数を0にして実行する
            onComplete?.Invoke(0);
            return;
        }
        _context = SynchronizationContext.Current;
        _onComplete = onComplete;
        Sge_Att_requestTrackingAuthorization(OnRequestComplete);
    }

    [AOT.MonoPInvokeCallback(typeof(OnCompleteCallback))]
    private static void OnRequestComplete(int status)
    {
        if (_onComplete != null)
        {
            _context.Post(_ => {
                // if (_onComplete!=null) { _onComplete(status); } を省略した書き方
                _onComplete?.Invoke(status);
                _onComplete = null;
            }, null);
        }
    }
#endif
    }
}