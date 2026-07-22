# 네트워크 출구 포탈 구현

## 요청 내용

- 현재 열려 있던 `InGame_portaldev` 씬의 포탈에 들어온 플레이어가 다시 나가지 못하게 한다.
- 거북이와 토끼가 모두 포탈에 들어오면 양쪽 화면을 검게 페이드한다.
- 페이드가 끝나면 포탈 컴포넌트에서 지정한 씬으로 네트워크 이동한다.

## 작업 내용

- `Portal` 오브젝트에 트리거용 `BoxCollider2D`, `NetworkObject`, `DualPlayExitPortal`을 구성했다.
- 포탈 진입 요청을 서버에서 플레이어 오브젝트와 거리로 검증하고, 클라이언트 ID별로 한 번만 집계하도록 구현했다.
- 먼저 진입한 플레이어는 이동 컨트롤러, 충돌체와 Rigidbody2D 시뮬레이션을 잠그고 포탈 안의 역할별 위치에 고정되도록 구현했다.
- 두 플레이어가 모두 진입하면 모든 클라이언트에서 `CanvasGroup` 기반 검은색 페이드를 재생하도록 구성했다.
- 호스트가 Netcode SceneManager를 통해 대상 씬을 한 번만 로드하도록 구현했다.
- `Portal`의 `DualPlay Exit Portal > Target Scene` 슬롯에서 다음 씬을 직접 지정할 수 있게 했다.
- 현재 대상은 `InGame_2`이며, 해당 씬을 Build Settings에 등록했다.
- 페이드 전용 `Portal Transition UI/Fade Overlay`를 현재 씬에 추가했다.

## 보존 범위

- 다른 게임 씬은 수정하지 않았다.
- 기존 플레이어 컨트롤러 스크립트는 수정하지 않고 별도 포탈 잠금 컴포넌트를 추가했다.
- 작업 전부터 존재하던 폰트, 애니메이션과 포탈 개발 씬 변경 사항을 유지했다.

## 확인 내용

- Runtime 및 Editor C# 컴파일: 오류 0개.
- 포탈 트리거, NetworkObject, 대상 SceneAsset, 페이드 CanvasGroup 직렬화 참조를 확인했다.
- `InGame_2` Build Settings 등록을 확인했다.
- Unity 플레이 모드 네트워크 테스트는 팀에서 진행하기로 하여 실행하지 않았다.
