# 듀얼플레이 실행 가이드

## 개요

Unity Netcode for GameObjects와 Unity Transport를 사용해 두 실행 인스턴스가 Host와 Client로 접속하고, 캐릭터와 레벨을 고른 뒤 실제 `InGame`으로 함께 이동하는 2인 네트워크 흐름이다.

- 메인 화면: `Assets/Scenes/MainMenuScene.unity`
- 연결 로비: `Assets/Scenes/DualPlayConnectionTestScene.unity`
- 캐릭터 선택: `Assets/Scenes/CharacterSelectScene.unity`
- 레벨 선택: `Assets/Scenes/Level.unity`
- 게임 씬: `Assets/Scenes/InGame.unity`
- 네트워크 샘플: `Assets/Scenes/DualPlaySampleScene.unity`
- 공용 네트워크 프리팹: `Assets/Prefabs/DualPlayNetworkBootstrap.prefab`
- 플레이어 프리팹: `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- 공용 연결 설정: `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- 기본 주소: `127.0.0.1`
- 기본 UDP 포트: `7777`
- 최대 접속 인원: 2명
- 캐릭터 역할: 접속 후 각자 선택하며 중복 선택 불가

## 전체 실행 흐름

1. 실행하면 `MainMenuScene`이 열린다.
2. `Enter Connection Lobby`를 눌러 연결 로비로 이동한다.
3. 한 실행 인스턴스에서 `Start Host`를 누른다.
4. 다른 실행 인스턴스에서 Host 주소를 입력하고 `Start Client`를 누른다.
5. 두 플레이어가 접속되면 Host가 약 1초 뒤 `CharacterSelectScene`을 네트워크 씬으로 불러온다.
6. 두 플레이어가 서로 다른 토끼 또는 거북이를 선택한다.
7. 두 선택이 완료되면 Host가 `Level`을 불러온다.
8. Host가 플레이할 레벨을 선택하면 두 플레이어가 해당 게임 씬으로 함께 이동한다.

## 포함된 기능

- Host 시작 및 Client 접속
- 세 번째 플레이어 접속 거부
- 마지막 Host 주소와 선택한 Host/Client 접속 모드 저장
- 연결 상태를 유지한 네트워크 씬 전환
- 두 플레이어 접속 완료 시 캐릭터 선택 화면 자동 전환
- 서버 판정으로 토끼와 거북이 중복 선택 방지
- 두 플레이어의 캐릭터 선택 완료 시 `Level` 자동 전환
- 네트워크 플레이 중에는 Host만 레벨 선택 가능
- 소유자에게만 해당 캐릭터 입력과 2D 물리 활성화
- Owner 권한 `NetworkTransform` 위치 동기화
- 현재 역할과 접속 인원을 표시하는 게임 HUD
- 연결 종료 후 메인 화면 복귀

## 조작

- 거북이: `WASD` 또는 방향키
- 토끼 이동: `A/D` 또는 좌우 방향키
- 토끼 점프: `Space`

## 같은 PC에서 테스트하기

1. `MainMenuScene`이 첫 씬으로 포함된 Standalone 빌드를 만든다.
2. Unity 에디터와 Standalone 빌드를 각각 실행한다.
3. 두 창에서 `Enter Connection Lobby`를 누른다.
4. Unity 에디터에서 `Start Host`를 누른다.
5. Standalone 빌드에서 주소를 `127.0.0.1`로 둔 채 `Start Client`를 누른다.
6. 두 창이 자동으로 캐릭터 선택 화면으로 전환되는지 확인한다.
7. 한쪽이 고른 캐릭터가 다른 쪽에서 선택 불가 상태가 되는지 확인한다.
8. 서로 다른 캐릭터를 고르면 두 창이 `Level`로 이동하는지 확인한다.
9. Host에서 레벨을 선택한 뒤 각 창에서 자신이 선택한 캐릭터만 조작되는지 확인한다.

## 다른 PC와 LAN에서 테스트하기

1. 두 PC를 같은 로컬 네트워크에 연결한다.
2. Host PC에서 `Start Host`를 누른다.
3. Client PC에 Host PC의 사설 IPv4 주소를 입력한다.
4. Client PC에서 `Start Client`를 누른다.
5. 연결되지 않으면 Host PC 방화벽에서 UDP `7777` 포트 또는 실행 파일을 허용한다.

이 구현은 Unity Relay를 사용하지 않는 직접 UDP 연결 방식이다. 인터넷 접속에는 NAT, 포트 포워딩 또는 Relay 대응이 별도로 필요하다.

## 주요 구성

### DualPlayConnectionSettings

- 주소, 포트, 최대 인원과 플레이어 프리팹을 저장한다.
- 메인 화면, 연결 로비, 캐릭터 선택, 레벨 선택, 게임 씬 이름과 자동 전환 대기시간을 저장한다.
- 마지막 입력 주소와 선택한 접속 모드를 `PlayerPrefs`에 저장한다.

### DualPlayNetworkBootstrap

- `NetworkManager`, `UnityTransport`, `DualPlayNetworkLauncher`를 하나의 프리팹으로 묶는다.
- `DontDestroyOnLoad`로 유지되어 연결 로비부터 캐릭터 선택, 레벨 선택, 게임 씬까지 이동해도 접속이 유지된다.
- 다음 씬에 같은 프리팹이 있으면 중복 인스턴스를 제거한다.

### DualPlayNetworkLauncher

- 연결 로비와 캐릭터 선택 UI, 게임 HUD를 표시한다.
- 2명이 모두 접속하면 Host가 NGO 씬 관리자로 `CharacterSelectScene`을 로드한다.
- 서로 다른 캐릭터 선택이 완료되면 Host가 `Level`을 로드한다.
- 최대 인원을 2명으로 제한하고 연결 종료 및 메인 화면 복귀를 처리한다.

### DualPlayNetworkPlayer

- 각 플레이어의 선택 요청을 서버가 검사하고, 중복되지 않은 토끼 또는 거북이 역할을 동기화한다.
- `IsOwner`인 인스턴스에서만 해당 역할의 컨트롤러와 `Rigidbody2D` 물리를 활성화한다.
- 원격 캐릭터에는 Animator 상태와 함께 `IsRun`, `IsGround`, `YVelocity` 파라미터를 동기화한다.
- 선택한 거북이 또는 토끼의 실제 스프라이트와 Animator Controller를 사용한다.

## 자동 실행 인자

Standalone 자동 연결 검증이 필요하면 다음 인자를 사용할 수 있다.

- Host 자동 시작: `-dualPlayHost`
- Client 자동 시작: `-dualPlayClient`
- Host 주소 지정: `-dualPlayAddress=127.0.0.1`

## 확장 시 주의 사항

- 네트워크 플레이어는 선택한 토끼·거북이 스프라이트와 Animator Controller를 사용하며, `InGame`에 배치된 로컬 테스트용 캐릭터는 네트워크 플레이 중 자동으로 비활성화된다.
- 현재 물리와 위치 동기화는 Owner 권한 방식이다. 치트 방지나 서버 판정이 필요하면 서버 권한 구조를 검토한다.
- 멀티플레이 씬 전환에는 일반 `SceneManager.LoadScene`이 아니라 `NetworkManager.SceneManager.LoadScene`을 사용해야 한다.
