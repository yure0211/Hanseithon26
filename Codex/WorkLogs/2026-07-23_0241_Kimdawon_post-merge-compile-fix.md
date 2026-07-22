# 병합 후 컴파일 오류 수정

## 작업 목적

- 친구 브랜치들을 `main`에 병합한 뒤 발생한 Unity C# 컴파일 오류의 원인을 확인하고 기존 기능을 보존하면서 해결한다.

## 변경한 파일

- `Assets/Scripts/DualPlaySample/DualPlayNetworkPlayer.cs`
- `Codex/WorkLogs/2026-07-23_0241_Kimdawon_post-merge-compile-fix.md`

## 핵심 변경 내용

- 병합 결과 `HandleAnimationStateChanged`와 `HandleFacingChanged`가 각각 두 번 선언된 것을 확인했다.
- 애니메이터 파라미터까지 동기화하는 확장된 구현을 유지하고, 뒤쪽에 중복으로 남은 이전 구현 두 개만 제거했다.
- `LocalPlayerFollowCamera`가 사용하는 Cinemachine 3.1.7은 이미 `Packages/manifest.json`과 `Packages/packages-lock.json`에 등록되어 있어 패키지 설정과 카메라 코드는 변경하지 않았다.
- Unity 로그에서 Cinemachine 패키지의 임시 폴더를 `Library/PackageCache`의 최종 폴더로 바꾸는 과정이 파일 잠금(`EPERM`)으로 실패한 것을 확인했다.

## 확인 또는 테스트 결과

- 수정 전 `dotnet build Assembly-CSharp.csproj --no-restore`에서 중복 멤버 오류 2개와 아직 복원되지 않은 Cinemachine 참조 오류 3개를 재현했다.
- 중복 멤버 제거 후 같은 명령을 다시 실행해 `CS0111` 오류 2개가 사라진 것을 확인했다.
- 현재 남은 오류 3개는 모두 `LocalPlayerFollowCamera.cs`의 `Unity.Cinemachine` 및 `CinemachineCamera` 참조이며 소스 코드 오류가 아니라 패키지가 로컬 캐시에 생성되지 않은 상태에서 발생한다.
- Unity Editor 로그에서 `com.unity.cinemachine` 임시 패키지 폴더를 최종 폴더로 바꾸는 과정의 `EPERM: operation not permitted` 오류를 확인했다.
- 잠금을 잡고 있던 Asset Import Worker를 재시작하고 패키지 복원을 재요청했으나 실패 상태의 Package Manager가 자동 복구되지 않아 Editor 재실행 전에는 전체 컴파일 검증을 완료할 수 없었다.

## 남은 작업이나 주의 사항

- Cinemachine 패키지 복원은 열려 있는 Unity Editor를 완전히 종료한 뒤 프로젝트를 한 번만 다시 열어 파일 잠금을 해제해야 한다.
- 재실행 후 Unity Package Manager가 3.1.7을 복원하면 `LocalPlayerFollowCamera`의 타입 참조 오류도 해소되어야 한다.
- 재실행 전 열린 씬과 프리팹을 저장해야 하며, 재실행 후 Console과 전체 C# 컴파일을 다시 확인해야 한다.
