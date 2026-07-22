# 열쇠돌 범위 파괴 작업

## 작업 목적

- 토끼가 열쇠돌 하나를 해제하면 주변 10블럭 이내의 다른 열쇠돌도 함께 사라지게 한다.
- 로컬 씬 테스트와 Host/Client 네트워크 플레이에 같은 규칙을 적용한다.

## 변경한 파일

- `Assets/Scripts/BunnyKeyInteractionUtility.cs`
- `Assets/Scripts/BunnyKeyStoneLock.cs`
- `Assets/Scripts/BunnyKeyInventory.cs`
- `Assets/Prefabs/KeyStone.prefab`
- `Assets/Scenes/InGame_1.unity`

## 핵심 변경 내용

- `BunnyKeyStoneLock`에 `nearbyBreakRadius` 설정을 추가하고 기본값을 `10`으로 지정했다.
- 타일 1칸을 Unity 월드 1유닛으로 간주해 원형 거리 10유닛 이하를 범위로 판정한다.
- Transform 원점 대신 Collider2D 또는 Renderer의 실제 월드 Bounds 중심을 범위 중심으로 사용한다.
- 접촉한 열쇠돌과 같은 씬에 있으며 범위 안에 등록된 모든 `BunnyKeyStoneLock`을 함께 비활성화한다.
- 범위 안의 열쇠돌 개수와 관계없이 최초 해제에 열쇠 1개만 소비한다.
- 네트워크에서는 서버가 범위 중심과 반경을 결정하고 ClientRpc로 모든 클라이언트에 같은 범위 파괴를 적용한다.

## 확인 결과

- `dotnet build Assembly-CSharp.csproj --no-restore` 결과 오류 0개를 확인했다.
- 기존 `System.IO.Compression`, `System.Net.Http` 참조 버전 충돌 경고 2개만 존재한다.
- 플레이 모드 테스트는 팀에서 진행하기로 했으므로 실행하지 않았다.

## 테스트 절차

1. `KeyStone.prefab`을 서로 10유닛 이내와 10유닛 밖에 각각 배치한다.
2. 열쇠 하나를 획득한 토끼로 열쇠돌 하나에 접근한다.
3. 접촉한 열쇠돌과 10유닛 이내 열쇠돌은 함께 사라지고, 범위 밖 열쇠돌은 남는지 확인한다.
4. 네트워크에서는 Host와 Client 양쪽 결과가 같은지 확인한다.
