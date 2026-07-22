# 캐릭터 상호 충돌 비활성화 작업

## 작업 목적

- 토끼와 거북이가 서로 밀거나 가로막지 않고 통과할 수 있게 한다.
- 바닥, 상자, 열쇠 등 다른 게임 오브젝트와의 기존 충돌은 유지한다.

## 변경한 파일

- `ProjectSettings/TagManager.asset`
- `ProjectSettings/Physics2DSettings.asset`
- `Assets/Prefabs/DualPlayNetworkPlayer.prefab`
- `Assets/Scenes/InGame_1.unity`
- `Assets/Scenes/InGame_sample.unity`

## 핵심 변경 내용

- 비어 있던 사용자 레이어 7을 `Player` 레이어로 등록했다.
- Physics 2D 레이어 충돌 행렬에서 `Player ↔ Player` 충돌만 비활성화했다.
- `InGame_1`과 샘플 씬의 토끼·거북이를 `Player` 레이어로 변경했다.
- 실제 멀티플레이에서 생성되는 `DualPlayNetworkPlayer` 프리팹도 `Player` 레이어로 변경했다.
- 다른 레이어와의 충돌 설정은 변경하지 않았다.

## 확인 결과

- 레이어 7 이름이 `Player`로 저장된 것을 확인했다.
- Physics 2D 충돌 행렬에서 레이어 7의 자기 자신 충돌 비트만 꺼진 것을 확인했다.
- 씬 캐릭터 4개와 네트워크 플레이어 프리팹이 모두 레이어 7을 사용하는지 정적 검사했다.
- C# 스크립트는 변경하지 않았다.
- 플레이 모드 검증은 팀에서 직접 진행하기로 한 기존 요청에 따라 실행하지 않았다.

## 수동 확인 절차

1. Host와 Client가 캐릭터를 선택하고 `InGame_1`에 입장한다.
2. 토끼와 거북이가 서로 마주 보고 이동했을 때 밀리지 않고 겹쳐 지나가는지 확인한다.
3. 두 캐릭터 모두 바닥과 상자에는 기존처럼 충돌하는지 확인한다.

## 주의 사항

- 앞으로 추가하는 플레이어 캐릭터 프리팹도 `Player` 레이어를 사용해야 같은 규칙이 적용된다.
