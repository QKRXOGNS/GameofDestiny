using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ProjectBootstrap
{
    /// <summary>
    /// GitHub UPM 패키지를 설치하는 인스톨러
    /// LLMUnity: https://github.com/undreamai/LLMUnity
    /// </summary>
    public static class PackageInstaller
    {
        // GitHub UPM 패키지 목록
        static readonly string[] PackagesToAdd =
        {
            "git+https://github.com/undreamai/LLMUnity.git"
        };

        const double TimeoutSeconds = 600;

        static AddRequest _request;
        static double _deadline;

        // Invoke: -executeMethod ProjectBootstrap.PackageInstaller.Install
        public static void Install()
        {
            if (PackagesToAdd.Length == 0)
            {
                Debug.Log("[PackageInstaller] Nothing to do.");
                EditorApplication.Exit(0);
                return;
            }

            Debug.Log($"[PackageInstaller] Adding: {string.Join(", ", PackagesToAdd)}");
            _request = Client.Add(PackagesToAdd[0]);
            _deadline = EditorApplication.timeSinceStartup + TimeoutSeconds;
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (_request == null) return;

            if (!_request.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup > _deadline)
                {
                    EditorApplication.update -= Poll;
                    Debug.LogError("[PackageInstaller] Timed out waiting for UPM.");
                    EditorApplication.Exit(2);
                }
                return;
            }

            EditorApplication.update -= Poll;

            if (_request.Status == StatusCode.Success)
            {
                var info = _request.Result;
                Debug.Log($"[PackageInstaller] Installed: {info.name}@{info.version}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[PackageInstaller] Failed: {_request.Error?.message}");
                EditorApplication.Exit(1);
            }
        }
    }
}
