# InGame 1·2·3 씬 UI 복원 작업 기록

## 요청

- 병합 후 `InGame_1`, `InGame_2`, `InGame_3`에 씬 기반 게임 HUD를 다시 구성
- 기존 타일맵, 상자, Key/KeyStone 등 게임 오브젝트는 보존

## 작업 내용

- 세 씬에 동일한 `Gameplay UI > Network HUD` 구조를 저장
- 각 HUD에 다음 오브젝트 구성
  - `RoleText`: 로컬 플레이어 역할 표시
  - `SessionText`: 호스트·클라이언트 접속 상태 표시
  - `DisconnectButton`: 연결 종료 및 메인 화면 복귀
- `DualPlaySceneUi`의 Gameplay HUD 모드와 각 UI 참조 연결
- 1920×1080 기준 `CanvasScaler` 적용
- 기존 `Sprite-00012_0` 9-slice 버튼과 기존 TMP 도트 폰트 재사용
- 기존 EventSystem을 유지하여 씬마다 하나만 존재하도록 구성

## 병합 손상 복구

- `InGame_1`에 남아 있던 끊어진 HUD 부모·텍스트 참조를 Unity가 감지함
- 불완전한 기존 HUD 조각만 제거하고 새 Canvas/HUD로 교체
- 깨진 로컬 fileID 참조가 씬에 남아 있지 않음을 확인

## 보존 확인

- UI 외 GameObject 이름 개수 비교 완료
- Key, KeyStone, TurtleCarryBox 및 PrefabInstance 개수가 작업 전후 동일함을 확인
- 세 씬 모두 다음 항목이 정확히 하나씩 존재함
  - Canvas
  - EventSystem
  - Gameplay UI
  - Network HUD
  - DualPlaySceneUi

## 검증

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: 오류 0개
- `git diff --check`: 오류 없음
- 실제 2인 플레이 동작 테스트는 팀에서 진행
