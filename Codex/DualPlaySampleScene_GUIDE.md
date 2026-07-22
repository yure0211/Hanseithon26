# DualPlaySampleScene 실행 가이드

## 개요

`DualPlaySampleScene`은 Unity Netcode for GameObjects와 Unity Transport를 사용해 두 실행 인스턴스가 Host와 Client로 접속하는 최소 2인 네트워크 예제다. 연결 기능만 빠르게 확인할 수 있도록 동일한 구성을 복제한 `DualPlayConnectionTestScene`도 제공한다.

- 씬: `Assets/Scenes/DualPlaySampleScene.unity`
- 연결 테스트 씬: `Assets/Scenes/DualPlayConnectionTestScene.unity`
- 공용 네트워크 프리팹: `Assets/Prefabs/DualPlayNetworkBootstrap.prefab`
- 플레이어 프리팹: `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- 공용 연결 설정: `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- 기본 주소: `127.0.0.1`
- 기본 UDP 포트: `7777`
- 최대 접속 인원: 2명
- 이동: `WASD` 또는 방향키

## 포함된 기능

- Host 시작
- Client 접속
- 연결 종료
- Host 주소 직접 입력
- 세 번째 플레이어 접속 거부
- 접속한 플레이어 자동 생성
- 각 클라이언트가 자신이 소유한 플레이어만 조작
- `NetworkTransform`의 Owner 권한을 이용한 위치 동기화
- Host와 Client를 서로 다른 색으로 표시
- 마지막으로 입력한 Host 주소 저장
- 연결 상태를 유지한 채 다른 네트워크 씬으로 전환

## 같은 PC에서 테스트하기

1. Unity에서 `Assets/Scenes/DualPlayConnectionTestScene.unity`을 연다.
2. Standalone 빌드를 하나 만든다. 이 씬은 Build Settings에 이미 추가되어 있다.
3. Unity 에디터에서 Play Mode를 시작하고 `Start Host`를 누른다.
4. 빌드한 실행 파일을 열고 주소를 `127.0.0.1`로 둔 채 `Start Client`를 누른다.
5. 두 창에서 각각 `WASD` 또는 방향키를 눌러 자신의 사각형만 움직이는지 확인한다.

에디터와 빌드의 역할은 바꿔도 된다. 한쪽이 Host이고 다른 한쪽이 Client이면 된다.

## 다른 PC와 LAN에서 테스트하기

1. 두 PC가 같은 로컬 네트워크에 연결되어 있어야 한다.
2. Host PC에서 실행 파일을 열고 `Start Host`를 누른다.
3. Client PC의 주소 입력란에 Host PC의 사설 IPv4 주소를 입력한다.
4. Client PC에서 `Start Client`를 누른다.
5. 연결되지 않으면 Host PC 방화벽에서 UDP `7777` 포트 또는 실행 파일을 허용한다.

이 예제는 Unity Relay를 사용하지 않는 직접 UDP 연결 예제다. 인터넷을 통한 접속은 공유기 NAT, 포트 포워딩과 방화벽 설정이 별도로 필요하다.

## 주요 구성

### DualPlayConnectionSettings

- 기본 Host 주소와 UDP 포트를 저장한다.
- 최대 접속 인원과 네트워크 플레이어 프리팹을 저장한다.
- 씬 전환 시 네트워크 부트스트랩을 유지할지 설정한다.
- `rememberLastAddress`가 활성화되어 있으면 마지막 입력 주소를 `PlayerPrefs`에 저장한다.

### DualPlayNetworkBootstrap

- `NetworkManager`, `UnityTransport`, `DualPlayNetworkLauncher`를 하나의 프리팹으로 묶는다.
- `persistAcrossScenes`가 활성화된 경우 `DontDestroyOnLoad`로 유지된다.
- 다음 씬에도 같은 프리팹이 있으면 중복 인스턴스를 제거하고 기존 네트워크 연결을 유지한다.

### DualPlayNetworkLauncher

- Host와 Client 시작 UI를 표시한다.
- Unity Transport의 주소와 포트를 설정한다.
- 연결 승인 과정에서 최대 인원을 2명으로 제한한다.
- 접속 상태와 로컬 Client ID를 표시한다.

### DualPlayNetworkPlayer

- `IsOwner`인 인스턴스에서만 키보드 입력을 처리한다.
- 플레이 영역 안에서 이동하도록 위치를 제한한다.
- Host와 Client의 색을 구분한다.

### DualPlayDemoEnvironment

- 별도 이미지 에셋 없이 런타임 스프라이트로 테스트 공간을 만든다.
- Host 영역, Client 영역, 중앙선과 공동 목표 지점을 시각적으로 표시한다.

## 다른 씬에서 재사용하기

1. `Assets/Prefabs/DualPlayNetworkBootstrap.prefab`을 네트워크 연결을 시작할 씬에 배치한다.
2. 주소, 포트, 최대 인원 또는 플레이어 프리팹을 바꾸려면 `Assets/Settings/Networking/DualPlayConnectionSettings.asset`을 수정한다.
3. Host가 접속을 유지한 채 씬을 바꿀 때는 `NetworkManager.SceneManager.LoadScene`을 사용한다.
4. 전환 대상 씬에도 부트스트랩 프리팹을 배치해도 되며, 실행 중 중복 인스턴스는 자동으로 제거된다.

연결 UI가 첫 씬에서만 필요하다면 부트스트랩 프리팹을 첫 씬에만 배치해도 된다. `persistAcrossScenes`가 활성화되어 있으면 이후 씬에서도 기존 `NetworkManager`가 유지된다.

## 확장 시 주의 사항

- 현재 예시는 이동 동기화 확인을 위한 최소 샘플이며 게임의 토끼·거북이 능력은 아직 포함하지 않는다.
- 실제 게임에서는 플레이어 프리팹을 토끼와 거북이 프리팹으로 교체하고 역할 배정 규칙을 추가해야 한다.
- 공용 인터넷 멀티플레이가 필요하면 Unity Relay 또는 별도의 서버·NAT 대응 방식을 설계해야 한다.
- 네트워크 입력 검증과 치트 방지가 필요하면 Owner 권한 이동 대신 서버 권한 이동 구조를 검토한다.
- 연결을 유지해야 하는 멀티플레이 씬 전환은 일반 `SceneManager.LoadScene`보다 NGO의 `NetworkManager.SceneManager.LoadScene`을 사용한다.
