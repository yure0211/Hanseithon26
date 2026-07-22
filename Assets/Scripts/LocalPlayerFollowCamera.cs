using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineCamera))]
public sealed class LocalPlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform offlineFallbackTarget;
    [SerializeField, Min(0.02f)] private float targetRefreshInterval = 0.25f;

    private CinemachineCamera cinemachineCamera;
    private Transform currentTarget;
    private float nextTargetRefreshTime;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();

        if (offlineFallbackTarget == null)
        {
            offlineFallbackTarget = cinemachineCamera.Follow;
        }
    }

    private void OnEnable()
    {
        nextTargetRefreshTime = 0f;
        RefreshFollowTarget();
    }

    private void LateUpdate()
    {
        if (currentTarget != null && Time.unscaledTime < nextTargetRefreshTime)
        {
            return;
        }

        nextTargetRefreshTime = Time.unscaledTime + targetRefreshInterval;
        RefreshFollowTarget();
    }

    private void RefreshFollowTarget()
    {
        Transform target = ResolveFollowTarget();
        if (target == null || target == currentTarget)
        {
            return;
        }

        currentTarget = target;
        cinemachineCamera.Follow = target;
        cinemachineCamera.LookAt = target;
    }

    private Transform ResolveFollowTarget()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager != null && networkManager.IsListening)
        {
            NetworkClient localClient = networkManager.LocalClient;
            NetworkObject localPlayer = localClient != null ? localClient.PlayerObject : null;
            return localPlayer != null ? localPlayer.transform : null;
        }

        return offlineFallbackTarget;
    }
}
