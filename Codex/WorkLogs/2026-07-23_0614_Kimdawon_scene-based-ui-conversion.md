# 씬 기반 UI 전환 작업 기록

## 요청

- 코드에서 즉석으로 그리던 UI를 Unity 씬의 `Canvas`, `Button`, `TMP_Text` 오브젝트로 전환
- 기존 도트 버튼·폰트·캐릭터 에셋을 유지하고 Inspector에서 직접 편집 가능하게 구성
- 완성된 Start 타이틀 에셋은 수정하지 않음

## 작업 내용

- `Start`
  - 기존 `Button_Sample`을 `StartButton`으로 유지·연결
  - 동일한 기존 도트 에셋으로 `QuitButton` 추가
  - `DualPlayMainMenu`가 씬에 저장된 버튼 참조만 사용하도록 변경
- `DualPlayConnectionTestScene`
  - `Scene UI/ConnectionPanel` 아래에 주소 입력창, 포트, 호스트·클라이언트·연결 해제 버튼, 상태 텍스트를 실제 씬 오브젝트로 저장
- `CharacterSelectScene`
  - `Scene UI/CharacterSelectPanel` 아래에 거북이·토끼 선택 버튼과 상태 UI를 실제 씬 오브젝트로 저장
- `Level`, `InGame_1`
  - `Network HUD`를 씬에 저장하고 역할·접속 상태·연결 해제 버튼을 Inspector에서 편집 가능하게 구성
- `DualPlaySceneUi` 추가
  - 씬 오브젝트의 버튼 이벤트와 네트워크 상태 텍스트만 연결
- `DualPlayNetworkLauncher`
  - 기존 IMGUI 화면 생성 및 런타임 배치 코드를 제거
  - 활성 씬의 `DualPlaySceneUi`를 찾아 네트워크 기능만 바인딩하도록 변경
- `DualPlayNetworkBootstrap` 프리팹
  - 더 이상 사용하지 않는 런타임 UI 스타일 참조 제거

## 보존 사항

- `Sprite-00011.aseprite` 타이틀 에셋 미수정
- 기존 `Sprite-00012.aseprite` 9-slice 버튼 스프라이트 재사용
- 기존 TMP 폰트 및 캐릭터 스프라이트 재사용
- Unity 배치 종료가 만든 TMP 동적 폰트 캐시 변경은 즉시 원복

## 확인

- 다섯 씬에 필요한 UI 오브젝트와 직렬화 참조가 존재함을 확인
- 각 씬의 EventSystem이 하나씩만 존재함을 확인
- Start 타이틀과 기존 버튼 스프라이트 GUID 유지 확인
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`: 오류 0개
- `git diff --check`: 오류 없음
- 실제 2인 플레이 동작 테스트는 팀에서 진행
