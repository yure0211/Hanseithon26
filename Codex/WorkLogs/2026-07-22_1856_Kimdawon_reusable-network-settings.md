# 재사용 가능한 네트워크 설정 및 연결 테스트 씬 작업 기록

## 작업 목적

기존 `DualPlaySampleScene`을 연결 테스트 전용 씬으로 복제하고, 네트워크 연결 설정과 `NetworkManager` 구성을 다른 씬에서도 재사용할 수 있도록 공용 에셋과 프리팹으로 분리한다.

## 변경한 파일

- `Assets/Scenes/DualPlaySampleScene.unity`
- `Assets/Scenes/DualPlayConnectionTestScene.unity`
- `Assets/Prefabs/DualPlayNetworkBootstrap.prefab`
- `Assets/Scripts/DualPlaySample/DualPlayConnectionSettings.cs`
- `Assets/Scripts/DualPlaySample/DualPlayNetworkLauncher.cs`
- `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- 위 신규 에셋에 대응하는 Unity `.meta` 파일
- `ProjectSettings/EditorBuildSettings.asset`
- `Codex/DualPlaySampleScene_GUIDE.md`
- `Codex/WorkLogs/2026-07-22_1856_Kimdawon_reusable-network-settings.md`

## 핵심 변경 내용

- 기존 샘플 씬을 `DualPlayConnectionTestScene`으로 복제했다.
- 주소, 포트, 최대 인원, 플레이어 프리팹과 씬 간 유지 여부를 담는 `DualPlayConnectionSettings` ScriptableObject를 추가했다.
- 마지막으로 입력한 Host 주소를 `PlayerPrefs`에 저장하고 다음 실행에서 불러오도록 구현했다.
- `NetworkManager`, `UnityTransport`, 런처를 `DualPlayNetworkBootstrap` 프리팹으로 분리했다.
- 네트워크 부트스트랩이 `DontDestroyOnLoad`로 씬 전환 후에도 유지되도록 구현했다.
- 다음 씬에 동일한 부트스트랩이 배치되어 있어도 중복 인스턴스를 제거하도록 구현했다.
- 원본 샘플 씬과 연결 테스트 씬이 동일한 설정 에셋과 부트스트랩 프리팹을 사용하도록 연결했다.
- 연결 테스트 씬을 Build Settings에 추가했다.

## 확인 및 테스트 결과

- 설정 스크립트 및 런처 스크립트 검증: 오류 0건, 경고 0건.
- 연결 테스트 씬 검증: 누락 스크립트와 깨진 프리팹 없이 문제 0건.
- 연결 테스트 씬에서 Host 시작 성공: Host 상태 `True`, 연결 인원 1명.
- 공용 설정 로드 확인: 주소 `127.0.0.1`, 포트 `7777`, 최대 인원 2명, 씬 간 유지 활성화.
- NGO 씬 관리자로 `DualPlaySampleScene` 전환 성공.
- 전환 후 `NetworkManager` 1개, 런처 1개만 남고 Host 상태와 연결 인원이 유지되는 것을 확인했다.
- 최종 Unity 콘솔 오류와 경고 0건.

## 남은 작업 및 주의 사항

- 실제 Standalone Client를 연결한 상태에서 두 프로세스의 동시 씬 전환은 별도 수동 테스트가 필요하다.
- 멀티플레이 씬 전환은 `NetworkManager.SceneManager.LoadScene`을 사용해야 모든 클라이언트가 함께 전환된다.
- 이번 작업은 별도 커밋 요청이 없어 커밋하지 않았다.
