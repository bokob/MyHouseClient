using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] GameObject _mainCamera;                     // 메인 카메라
    [SerializeField] GameObject _quaterFollowCamera;
    [SerializeField] GameObject _thirdFollowCamera;
    [SerializeField] GameObject _aimCamera;
    public GameObject _cinemachineCameraTarget; // 카메라가 바라볼 목표물
    public float _topClamp = 70.0f;             // 카메라 위 제한 각도
    public float _bottomClamp = -30.0f;         // 카메라 아래 제한 각도
    public float _cameraAngleOverride = 0.0f;   // 카메라 회전 각도 미세 조정에 사용
    public bool _lockCameraPosition = false;    // 카메라 잠금
    float _cinemachineTargetYaw;                // 카메라 Y축 회전 제어 사용
    float _cinemachineTargetPitch;

    public Define.View View = Define.View.None; // 카메라 시점

    // player
#if ENABLE_INPUT_SYSTEM
    PlayerInput _playerInput;
#endif

    public PlayerInputs _input;

    const float _threshold = 0.01f;

    public float _sensitivity = 1f;

    // Start is called before the first frame update

    void Start()
    {
    }

    void LateUpdate()
    {
        if (View != Define.View.Third)
        {
            CameraRotation();
        }

        if (_quaterFollowCamera.activeSelf)
        {
            //TestRay();
            //TestRay2();
            TestRay4();
        }
    }

    public void SetRobberView() // 강도 시점 설정
    {
        // 강도에 맞는 카메라 설정
        _quaterFollowCamera.SetActive(true);
        _thirdFollowCamera.SetActive(false);
        _aimCamera.SetActive(false);
    }

    public void SetHouseownerView() // 집주인 시점 설정
    {
        // 집주인에 맞는 카메라 설정
        _quaterFollowCamera.SetActive(false);
        _thirdFollowCamera.SetActive(true);
        _aimCamera.SetActive(true);
    }

    /// <summary>
    /// 카메라 각도 제한
    /// </summary>
    /// <param name="lfAngle"></param>
    /// <param name="lfMin"></param>
    /// <param name="lfMax"></param>
    /// <returns></returns>
    static public float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    /// <summary>
    /// 카메라 회전
    /// </summary>
    void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = 1.0f;

            // 정조준 할 때 천천히 돌아가야 하니까 Sensitivity를 넣어준다.
            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * _sensitivity;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * _sensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        // 시네마신 카메라가 목표를 따라감
        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        _sensitivity = newSensitivity;
    }


    #region 테스트
    public Camera mainCamera; // 메인 카메라
    public Transform player; // 플레이어의 Transform
    public float rayDistance = 100f; // 레이의 최대 거리
    public int raysPerAxis = 10; // X, Y축 당 쏠 레이의 수 (총 레이 수는 raysPerAxis^2)
    public float maxRenderDistance = 1f; // 이 거리 이하일 때 렌더링하지 않음

    private Dictionary<GameObject, Renderer> hiddenRenderers = new Dictionary<GameObject, Renderer>();


    void TestRay()
    {
        // 기존에 비활성화된 모든 렌더러를 다시 활성화
        List<GameObject> toEnable = new List<GameObject>();

        foreach (var item in hiddenRenderers)
        {
            if (item.Value != null)
            {
                item.Value.enabled = true;
                Debug.Log(item.Key.name + "활성화");
                toEnable.Add(item.Key);
            }
        }

        foreach (var obj in toEnable)
        {
            hiddenRenderers.Remove(obj);
        }

        // 플레이어 위치에서 카메라 위치를 향해 레이 쏘기
        Vector3 direction = (mainCamera.transform.position - player.position).normalized;
        Ray ray = new Ray(player.position, direction);
        RaycastHit[] hits;

        // 레이캐스트로 충돌된 모든 오브젝트 탐지
        hits = Physics.RaycastAll(ray, rayDistance);

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                Debug.Log(hit.transform.name + "비활성화");
                rend.enabled = false;
                if (!hiddenRenderers.ContainsKey(hit.collider.gameObject))
                {
                    hiddenRenderers.Add(hit.collider.gameObject, rend);
                }
            }
        }
    }

    void TestRay2()
    {
        // 기존에 비활성화된 모든 렌더러를 다시 활성화
        List<GameObject> toEnable = new List<GameObject>();

        foreach (var item in hiddenRenderers)
        {
            if (item.Value != null)
            {
                item.Value.enabled = true;
                Debug.Log(item.Key.name + " 활성화");
                toEnable.Add(item.Key);
            }
        }

        foreach (var obj in toEnable)
        {
            hiddenRenderers.Remove(obj);
        }

        // 화면을 그리드로 나누고 각 그리드의 중앙에서 레이 쏘기
        float stepX = 1f / raysPerAxis;
        float stepY = 1f / raysPerAxis;

        for (int i = 0; i < raysPerAxis; i++)
        {
            for (int j = 0; j < raysPerAxis; j++)
            {
                Vector3 viewportPoint = new Vector3(stepX * i + stepX / 2, stepY * j + stepY / 2, 0);
                Ray ray = mainCamera.ViewportPointToRay(viewportPoint);
                RaycastHit[] hits;

                // 레이캐스트로 충돌된 모든 오브젝트 탐지
                hits = Physics.RaycastAll(ray, rayDistance);

                foreach (RaycastHit hit in hits)
                {
                    Renderer rend = hit.collider.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        Debug.Log(hit.transform.name + " 비활성화");
                        rend.enabled = false;
                        if (!hiddenRenderers.ContainsKey(hit.collider.gameObject))
                        {
                            hiddenRenderers.Add(hit.collider.gameObject, rend);
                        }
                    }
                }
            }
        }
    }

    void TestRay3()
    {
        // 기존에 비활성화된 모든 렌더러를 다시 활성화
        List<GameObject> toEnable = new List<GameObject>();

        foreach (var item in hiddenRenderers)
        {
            if (item.Value != null)
            {
                item.Value.enabled = true;
                Debug.Log(item.Key.name + " 활성화");
                toEnable.Add(item.Key);
            }
        }

        foreach (var obj in toEnable)
        {
            hiddenRenderers.Remove(obj);
        }

        // 화면을 그리드로 나누고 각 그리드의 중앙에서 레이 쏘기
        float stepX = 1f / raysPerAxis;
        float stepY = 1f / raysPerAxis;

        for (int i = 0; i < raysPerAxis; i++)
        {
            for (int j = 0; j < raysPerAxis; j++)
            {
                Vector3 viewportPoint = new Vector3(stepX * i + stepX / 2, stepY * j + stepY / 2, 0);
                Ray ray = mainCamera.ViewportPointToRay(viewportPoint);
                RaycastHit[] hits;

                // 레이캐스트로 충돌된 모든 오브젝트 탐지
                hits = Physics.RaycastAll(ray, rayDistance);

                foreach (RaycastHit hit in hits)
                {
                    //// "Obstacle" 태그가 있는 오브젝트는 무시
                    //if (hit.collider.CompareTag("Obstacle"))
                    //{
                    //    continue;
                    //}

                    // 카메라와의 거리 계산
                    float distanceToCamera = Vector3.Distance(mainCamera.transform.position, hit.transform.position);

                    // 만약 거리가 maxRenderDistance 이하라면, 렌더링 비활성화
                    if (distanceToCamera <= maxRenderDistance)
                    {
                        Renderer rend = hit.collider.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            Debug.Log(hit.transform.name + " 비활성화");
                            rend.enabled = false;
                            if (!hiddenRenderers.ContainsKey(hit.collider.gameObject))
                            {
                                hiddenRenderers.Add(hit.collider.gameObject, rend);
                            }
                        }
                    }
                }
            }
        }
    }

    void TestRay4()
    {
        // 기존에 비활성화된 모든 렌더러를 다시 활성화
        List<GameObject> toEnable = new List<GameObject>();

        foreach (var item in hiddenRenderers)
        {
            if (item.Value != null)
            {
                item.Value.enabled = true;
                Debug.Log(item.Key.name + "활성화");
                toEnable.Add(item.Key);
            }
        }

        foreach (var obj in toEnable)
        {
            hiddenRenderers.Remove(obj);
        }

        // 플레이어 위치에서 카메라 위치를 향해 레이 쏘기
        Vector3 direction = (mainCamera.transform.position - player.position).normalized;
        Ray ray = new Ray(player.position, direction);
        RaycastHit[] hits;

        // 레이캐스트로 충돌된 모든 오브젝트 탐지
        hits = Physics.RaycastAll(ray, rayDistance);

        foreach (RaycastHit hit in hits)
        {
            //// "Obstacle" 태그가 있는 오브젝트는 무시
            //if (hit.collider.CompareTag("Obstacle"))
            //{
            //    continue;
            //}

            // 카메라와의 거리 계산
            float distanceToCamera = Vector3.Distance(mainCamera.transform.position, hit.transform.position);

            distanceToCamera = Mathf.Abs(distanceToCamera);

            // 만약 거리가 maxRenderDistance 이하라면, 렌더링 비활성화
            if (distanceToCamera <= maxRenderDistance)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null)
                {
                    Debug.Log(hit.transform.name + " 비활성화");
                    rend.enabled = false;
                    if (!hiddenRenderers.ContainsKey(hit.collider.gameObject))
                    {
                        hiddenRenderers.Add(hit.collider.gameObject, rend);
                    }
                }
            }
        }
    }
    #endregion
}
