# 컴포넌트 기반 다중 열쇠·열쇠돌 작업

## 작업 목적

- 오브젝트 이름에 의존하던 열쇠 상호작용을 제거한다.
- 여러 개의 열쇠와 열쇠돌을 씬에 쉽게 배치할 수 있게 한다.
- 토끼가 실제로 접근한 오브젝트 한 개만 처리하고 네트워크 양쪽에 같은 결과를 반영한다.

## 변경한 파일

- `Assets/Scripts/BunnyKeyInteractionUtility.cs`
- `Assets/Scripts/BunnyKeyInventory.cs`
- `Assets/Scripts/BunnyKeyPickup.cs`
- `Assets/Scripts/BunnyKeyStoneLock.cs`
- `Assets/Prefabs/Key.prefab`
- `Assets/Prefabs/KeyStone.prefab`
- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Assets/Scenes/InGame_1.unity`
- 각 신규 에셋의 `.meta` 파일
- 기존 이름 기반 `BunnyKeyStoneInteraction.cs`와 `.meta`는 제거

## 핵심 변경 내용

- `GameObject.Find`와 `Key`, `KeyStone`, `Bunny` 이름 판정을 모두 제거했다.
- `BunnyKeyPickup` 컴포넌트가 붙은 오브젝트에 토끼가 접근하면 열쇠 수가 1 증가하고 해당 오브젝트만 사라진다.
- `BunnyKeyStoneLock` 컴포넌트가 붙은 오브젝트에 열쇠를 가진 토끼가 접근하면 열쇠 수가 1 감소하고 해당 오브젝트만 사라진다.
- 각 오브젝트는 씬 경로와 Transform 계층 순서로 만든 식별자를 사용하므로 이름을 변경해도 동작한다.
- 열쇠 획득과 열쇠돌 해제 요청은 토끼 소유 클라이언트가 보내고 서버가 역할, 소유권, 거리, 열쇠 수를 검증한 뒤 ClientRpc로 모든 클라이언트에 반영한다.
- 현재 `InGame_1` 씬의 기존 열쇠와 열쇠돌에도 각각 새 컴포넌트를 연결했다.
- `Key.prefab`과 `KeyStone.prefab`을 추가해 Project 창에서 원하는 만큼 씬에 끌어다 놓을 수 있게 했다.
- 열쇠 수는 씬이 바뀔 때 서버에서 0으로 초기화된다.

## 사용 방법

- `Assets/Prefabs/Key.prefab`을 필요한 위치에 원하는 개수만큼 배치한다.
- `Assets/Prefabs/KeyStone.prefab`을 필요한 위치에 원하는 개수만큼 배치한다.
- 오브젝트 이름은 자유롭게 변경해도 된다.
- 열쇠 하나는 토끼가 접촉한 열쇠돌 하나를 해제할 때 소비된다.

## 확인 결과

- `Assembly-CSharp.csproj` 컴파일 결과 오류 0개를 확인했다.
- 기존 프로젝트 의존성의 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개만 남아 있다.
- 신규 스크립트에서 `GameObject.Find`를 사용하지 않음을 확인했다.
- 씬과 프리팹의 신규 MonoBehaviour 참조가 각각 한 번 선언되고 한 번 연결되어 있음을 확인했다.

## 남은 작업 및 주의 사항

- Unity MCP 서버가 현재 Codex에 연결되지 않아 Unity 플레이 모드 검증은 실행하지 않았다.
- Unity 에디터가 외부 씬 변경 감지 창을 표시하면 `Reload`를 선택해야 한다.
- 실제 Host/Client 환경에서 열쇠 획득, 한 개 소비, 접촉한 열쇠돌만 사라지는지 팀에서 확인한다.
