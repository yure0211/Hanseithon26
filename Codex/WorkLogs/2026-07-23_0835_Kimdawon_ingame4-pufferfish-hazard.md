# InGame_4 복어 장애물 작업 기록

## 작업 목적

- `InGame_4`에 배치된 복어 오브젝트를 재사용 가능한 프리팹으로 만든다.
- 복어는 이동하지 않고 게임 시작 시 상·하·좌·우 중 한 방향을 바라보게 한다.
- 토끼는 복어를 통과하고, 거북이만 닿았을 때 라바와 같은 게임오버 처리를 실행한다.

## 변경한 파일

- `Assets/Scripts/PufferfishHazard.cs` 및 `.meta`
- `Assets/Editor/PufferfishPrefabSetup.cs` 및 `.meta`
- `Assets/Prefabs/Blowfish.prefab` 및 `.meta`
- `Assets/Scenes/InGame_4.unity`
- `Codex/GAME_DESIGN.md`

## 핵심 변경 내용

- 기존 `blowfish` 오브젝트의 스프라이트와 Animator를 유지한 `Blowfish` 프리팹을 생성했다.
- 복어에 `1.05 × 0.95` 크기의 `BoxCollider2D` Trigger를 연결했다.
- 복어는 시작할 때 0도, 90도, 180도, 270도 중 하나로 회전하며 위치 이동은 하지 않는다.
- Trigger에 들어온 오브젝트에서 활성화된 `TurtleController`만 감지하므로 로컬 토끼와 네트워크 토끼를 모두 무시한다.
- 거북이가 닿으면 컨트롤러, 운반 스킬, Rigidbody2D, Collider2D, Animator를 멈추고 렌더러를 숨긴 뒤 `Time.timeScale`을 0으로 설정한다.
- `InGame_4`의 기존 복어 위치 `(4.29259, 0.55762, 0)`를 유지한 채 프리팹 인스턴스로 연결했다.

## 확인 및 테스트 결과

- Unity 6000.3.15f1에서 복어 프리팹과 `InGame_4` 씬 임포트 성공을 확인했다.
- Unity 에디터 로그에서 복어 프리팹 생성 및 거북이 전용 Trigger 연결 완료 메시지를 확인했다.
- `Assembly-CSharp.csproj`와 `Assembly-CSharp-Editor.csproj` MSBuild 결과 오류 0개로 통과했다.
- 기존 Unity 패키지 참조의 어셈블리 버전 충돌 경고는 남아 있으나 이번 변경의 컴파일 오류는 아니다.

## 남은 확인 및 주의 사항

- 실제 플레이 모드에서 토끼가 통과하는지, 거북이가 닿으면 화면이 정지하는지 한 번 수동 확인이 필요하다.
- 복어 방향은 실행할 때마다 무작위로 정해지므로 네트워크 Host와 Client 화면에서 방향이 서로 다르게 보일 수 있으나 충돌 판정에는 영향을 주지 않는다.
