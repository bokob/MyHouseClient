# 충북대학교 컴퓨터공학과 졸업작품

## PLA2ER (2조)
| 이름 | 복무창 (팀장)                           | 조민규                                     | 이정인                                      | WU YIXIN                                  |  
| --- | --------------------------------------- | ------------------------------------------ | ------------------------------------------ | --------------------------------------- |  
|GitHub| [BokMuchang](https://github.com/bokob) | [Cho Min Gyu](https://github.com/2019037023) | [Jeongin Lee](https://github.com/dlqorns) | [WU YIXIN](https://github.com/a865619661) |  
___
## 의존성 (Dependency)
```
Unity
- 2021.3.20f1

PUN2
- 2.46
```
___  
## 개요 (Overview)
인기 게임 대부분의 문제 
- 긴 게임 세션과 복잡한 회원가입으로 사용자 피로감 유발 
- 간편하게 즐기려는 이들의 요구를 충족시키지 못함 
___   
## 시스템 아키텍처 (Architecture)
![깃허브에 올릴 시스템아키텍처](https://github.com/user-attachments/assets/dd3468b2-eaa3-4fe7-9b28-0a2ed0b749ff)
- 최대 4인 멀티 가능
- PUN2, Photon Realtime 이용
- Name Server는 AppId, GameVersion, 지역을 이용해 어떤 Photon Server에 연결할 지 결정하는 역할
- Master Server는 Lobby 진입, 방 생성, 방 입장 관리 역할
- Game Server는 클라이언트가 이동해 실제 게임을 진행하는 서버, 게임 내 동기화 및 멀티 기능은 RPC(Remote Procedure Call) 기능을 이용해 주로 이루어짐
___   
## 게임 로직  (Main Logic)
<img width="377" alt="image" src="https://github.com/user-attachments/assets/071df063-e2a5-44f6-a707-86d6cdb9b904">

- 플레이어는 집주인과 강도라는 두 역할로 플레이 가능
- 집주인은 모든 무기를 사용하고 TPS 시점으로 플레이
- 강도는 근접 무기를 사용하고 탑 뷰 시점으로 플레이
- 싱글 모드 시, 집주인으로 플레이
  - 파괴 효과 존재
- 멀티 모드 시, 마스터 클라이언트가 집주인으로, 그 외의 플레이어는 강도로 플레이
  - 마스터 클라이언트 처치 시, 처치한 클라이언트가 마스터 클라이언트가 되어 집주인으로 플레이하게 됨 (Host Migration)
  - 처치된 플레이어는 프로그램 강제 종료
___  
## 구현 결과(Implement Result)
![구현 결과](https://github.com/user-attachments/assets/1e2c3123-210a-41f0-a73f-27837cf24524)
___  
## 게임 플레이  (Game Play)
<img width="377" alt="image" src="https://github.com/user-attachments/assets/071df063-e2a5-44f6-a707-86d6cdb9b904">

- 플레이어는 집주인과 강도라는 두 역할로 플레이 가능
- 집주인은 모든 무기를 사용하고 TPS 시점으로 플레이
- 강도는 근접 무기를 사용하고 탑 뷰 시점으로 플레이
- 싱글 모드 시, 집주인으로 플레이
  - 파괴 효과 존재
- 멀티 모드 시, 마스터 클라이언트가 집주인으로, 그 외의 플레이어는 강도로 플레이
  - 마스터 클라이언트 처치 시, 처치한 클라이언트가 마스터 클라이언트가 되어 집주인으로 플레이하게 됨 (Host Migration)
  - 처치된 플레이어는 프로그램 강제 종료
___  
## 포스터  (Poster)
<img width="411" alt="포스터" src="https://github.com/user-attachments/assets/ec551080-0986-4c14-8bc2-ffee2c8777ae">

___  
## 상 (Award)
- 2024학년도 전자정보대학 캡스톤디자인 경진대회 최우수상 수상 (2024.11)
<img width="411" alt="최우수상" src="https://github.com/user-attachments/assets/f5521bae-50fe-4bd4-a728-8811b8781c83">

___  
## 사용된 오픈소스 (Open Source Used)
Mesh Cutting: https://github.com/KristinLague/Mesh-Cutting  
Unity Fracture: https://github.com/ElasticSea/unity-fracture
