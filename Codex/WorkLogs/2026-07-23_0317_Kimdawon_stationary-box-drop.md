# 거북이 상자 정지 내려놓기

## 작업 목적

- 거북이가 들고 있던 상자를 내려놓을 때 상자가 거북이의 이동 방향으로 밀려나는 현상을 없앤다.

## 변경한 파일

- `Assets/Scripts/TurtleCarrySkill.cs`
- `Codex/WorkLogs/2026-07-23_0317_Kimdawon_stationary-box-drop.md`

## 핵심 변경 내용

- 내려놓을 때 거북이의 현재 속도에 비율을 곱해 상자에 전달하던 처리를 제거했다.
- 상자를 내려놓을 때 전달 속도를 항상 `Vector2.zero`로 설정해 현재 위치에서 정지하도록 했다.
- 더 이상 사용하지 않는 `releaseVelocityRatio` Inspector 필드를 제거했다.
- 상자의 방향별 운반 위치, 확대 효과, 색상과 충돌 처리는 변경하지 않았다.

## 확인 또는 테스트 결과

- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity:quiet` 결과 오류 0개로 컴파일을 통과했다.
- Unity 패키지 참조에서 기존 `System.IO.Compression`, `System.Net.Http` 버전 충돌 경고 2개가 출력됐으며 이번 변경과는 무관하다.
- 실제 내려놓기 동작은 팀에서 Unity 플레이 테스트를 진행한다.

## 남은 작업이나 주의 사항

- 기존 씬에 직렬화되어 있던 `releaseVelocityRatio` 값은 대응 필드가 제거되어 더 이상 동작에 사용되지 않는다.
- 내려놓은 뒤 거북이가 상자에 직접 충돌하는 일반 물리 반응은 유지된다.
