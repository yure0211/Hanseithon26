# 거북이 상자 운반 스킬 프로토타입 작업 기록

## 작업 목적

기존 캐릭터 및 네트워크 스크립트를 수정하지 않고, 현재 Unity 에디터에 열려 있는 `InGame_skilldev` 씬에서 거북이가 가까운 상자를 들어 운반하고 내려놓을 수 있는 전용 스킬을 만든다.

## 변경한 파일

- `Assets/Scripts/TurtleCarrySkill.cs` 및 `.meta`
- `Assets/Scripts/TurtleCarryableBox.cs` 및 `.meta`
- `Assets/Editor/TurtleCarrySkillSceneSetup.cs` 및 `.meta`
- `Assets/Editor.meta`
- `Assets/Scenes/InGame_skilldev.unity` 및 `.meta`
- `Codex/WorkLogs/2026-07-22_2230_Kimdawon_turtle-carry-skill.md`

## 핵심 변경 내용

- 거북이 전용 운반 스킬을 새 컴포넌트 `TurtleCarrySkill`로 분리했다.
- `E` 키를 누르면 반경 1.75 안의 가장 가까운 `TurtleCarryableBox`를 들고, 다시 누르면 내려놓는다.
- 이동 입력 방향을 기준으로 거북이 앞 1.15 위치에 상자가 따라오도록 구현했다.
- 운반 중에는 상자를 Kinematic 상태로 바꾸고 거북이와 상자의 충돌을 잠시 무시하며, 내려놓을 때 원래 물리 설정과 충돌을 복구한다.
- 운반 가능 상자를 갈색 2D 오브젝트로 만들고 `Rigidbody2D`, `BoxCollider2D`, `TurtleCarryableBox`를 구성했다.
- 현재 열린 `InGame_skilldev` 씬의 비활성 `Turtle`을 테스트 가능하게 활성화하고 스킬 컴포넌트와 상자를 배치했다.
- 동일한 씬 구성을 다시 적용할 수 있도록 `Tools/Hanseithon/Setup Turtle Carry Skill` 에디터 메뉴를 추가했다.
- 기존 `TurtleController`, `BunnyController`, `DualPlaySample` 네트워크 스크립트는 수정하지 않았다.

## 확인 또는 테스트 결과

- Unity 현재 활성 씬이 `Assets/Scenes/InGame_skilldev.unity`인 것을 확인했다.
- Unity 자동 컴파일 후 `Assembly-CSharp.dll`과 `Assembly-CSharp-Editor.dll`이 갱신됐다.
- Unity Editor 로그 기준 C# 컴파일 오류 0건이다.
- Unity가 생성한 최신 컴파일 응답 파일로 런타임 및 에디터 어셈블리를 다시 검사했으며 두 컴파일 모두 종료 코드 0을 확인했다.
- Unity Editor 로그에서 `거북이 운반 스킬과 운반 상자를 InGame_skilldev 씬에 배치했습니다.` 메시지를 확인했다.
- 씬 파일에서 활성화된 `Turtle`, `TurtleCarrySkill`, `TurtleCarryBox`, `TurtleCarryableBox` 직렬화 참조를 확인했다.
- 기존 캐릭터 및 네트워크 스크립트의 Git diff가 없음을 확인했다.

## 수동 확인 절차

1. `InGame_skilldev` 씬에서 Play를 누른다.
2. `WASD` 또는 방향키로 거북이를 상자 가까이 이동한다.
3. `E`를 눌러 상자를 들고, 이동 방향 앞에서 상자가 따라오는지 확인한다.
4. `E`를 다시 눌러 상자를 내려놓고 물리 충돌이 복구되는지 확인한다.

## 남은 작업이나 주의 사항

- 이번 구현은 현재 열린 스킬 개발 씬을 위한 로컬 프로토타입이며 NGO 네트워크 동기화는 포함하지 않았다.
- `InGame_skilldev`를 실제 게임 흐름에서 사용하려면 Build Settings와 네트워크 씬 전환 설정을 별도로 합의해야 한다.
- 거북이 운반 능력을 최종 기획으로 확정할지와 상자를 활용할 협동 퍼즐은 팀 합의가 필요하다.
- 현재 세션에 Unity MCP 도구 연결이 없어 Unity 에디터의 자동 컴파일 및 에디터 설치 스크립트로 씬을 구성했다.
