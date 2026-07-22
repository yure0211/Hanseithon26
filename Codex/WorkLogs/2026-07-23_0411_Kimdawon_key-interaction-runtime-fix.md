# 열쇠 상호작용 미작동 수정

## 작업 목적

- 토끼가 열쇠와 열쇠돌에 접근해도 아무 동작이 발생하지 않는 문제를 해결한다.
- 현재 씬 직접 실행과 정상적인 2인 네트워크 흐름을 모두 지원한다.

## 원인

- 열쇠 컴포넌트가 `DualPlayNetworkPlayer.LocalPlayer`만 상호작용 대상으로 허용했다.
- `InGame_1` 씬을 직접 실행하면 움직이는 토끼는 일반 `BunnyController` 오브젝트이므로 `LocalPlayer`가 존재하지 않아 즉시 반환됐다.
- 네트워크 플레이어 프리팹과 연결 설정은 플레이 씬 이름을 `InGame`으로 가지고 있었지만 실제 Build Settings의 플레이 씬은 `InGame_1`이었다.
- `Level` 씬의 첫 스테이지 버튼도 존재하지 않는 `InGame`을 요청하고 있었다.
- Unity 로그에서는 신규 열쇠 스크립트의 C# 컴파일 또는 NGO RPC 후처리 오류가 발견되지 않았다.

## 변경한 파일

- `Assets/Scripts/BunnyLocalKeyInventory.cs`
- `Assets/Scripts/BunnyKeyInteractionUtility.cs`
- `Assets/Scripts/BunnyKeyPickup.cs`
- `Assets/Scripts/BunnyKeyStoneLock.cs`
- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Assets/Settings/Networking/DualPlayConnectionSettings.asset`
- `Assets/Scenes/Level.unity`

## 핵심 변경 내용

- 네트워크가 실행 중이지 않을 때 활성화된 일반 `BunnyController`를 컴포넌트로 찾아 로컬 열쇠 보관함을 자동 추가한다.
- 씬 직접 실행에서도 토끼가 열쇠에 닿으면 열쇠 수가 증가하고 열쇠 오브젝트가 사라진다.
- 로컬 열쇠를 가진 상태에서 열쇠돌에 닿으면 열쇠 1개를 소비하고 접촉한 열쇠돌만 사라진다.
- 네트워크 세션 중에는 기존 서버 검증과 ClientRpc 동기화 경로만 사용한다.
- 네트워크 플레이 씬 설정과 Level 첫 버튼 대상을 `InGame_1`로 통일했다.

## 확인 결과

- `dotnet build Assembly-CSharp.csproj --no-restore` 결과 오류 0개를 확인했다.
- 기존 `System.IO.Compression`, `System.Net.Http` 참조 버전 충돌 경고 2개만 존재한다.
- Unity Editor 로그에서 열쇠 스크립트 관련 컴파일 오류, Missing Script, RPC 후처리 오류가 발견되지 않았다.
- 플레이 모드 동작 검증은 팀에서 진행하기로 했으므로 실행하지 않았다.

## 테스트 절차

1. 실행 중이라면 Play Mode를 종료하고 Unity의 스크립트 컴파일이 끝날 때까지 기다린다.
2. `InGame_1`을 직접 실행해 씬의 일반 토끼로 열쇠와 열쇠돌을 차례대로 접촉한다.
3. 네트워크 테스트에서는 Main Menu부터 Host와 Client를 연결하고 Level의 첫 스테이지로 진입한다.
4. 토끼가 열쇠를 먹었을 때 양쪽 화면에서 같은 열쇠만 사라지는지 확인한다.
5. 열쇠돌 접촉 시 열쇠 1개가 소비되고 양쪽 화면에서 접촉한 열쇠돌만 사라지는지 확인한다.
