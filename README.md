# Shop Owner Simulator

**웹 기반 경제 시뮬레이션 게임**

[![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-blue)](https://blazor.net/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![PlayFab](https://img.shields.io/badge/Backend-PlayFab-green)](https://playfab.com/)

## 🎮 게임 소개

Shop Owner Simulator는 플레이어가 던전에 용병을 파견하여 자원을 모으고, 아이템을 제작하며, 개인 상점과 거래소를 운영하는 웹 기반 타이쿤 시뮬레이션 게임입니다.

### 핵심 기능
- **던전 탐험**: 용병을 파견하여 자원 채집
- **아이템 제작**: 수집한 자원으로 장비 제작
- **개인 상점**: 자동 판매 시스템
- **플레이어 거래소**: 다른 플레이어와의 거래
- **용병 강화**: 장비 장착으로 능력치 상승
- **방치형 요소**: 접속하지 않아도 게임 진행

## 🛠️ 기술 스택

- **Frontend**: Blazor WebAssembly (.NET 8)
- **Backend**: C# (PlayFab REST API)
- **Database**: PlayFab + DynamoDB
- **Hosting**: Cloudflare Pages
- **Styling**: CSS3 + Responsive Design
- **Charts**: Chart.js, D3.js

## 📋 프로젝트 구조

```
ShopOwnerSimulator/
├── Pages/                    # Razor 페이지 컴포넌트
│   ├── Dashboard.razor
│   ├── Mining.razor
│   ├── Crafting.razor
│   ├── Exchange.razor
│   ├── PersonalShop.razor
│   ├── Inventory.razor
│   ├── Mercenary.razor
│   ├── Tavern.razor
│   ├── GeneralShop.razor
│   └── Login.razor
├── Components/               # 재사용 가능한 컴포넌트
│   ├── Layout/
│   ├── Common/
│   └── [기타 컴포넌트]
├── Services/                 # 비즈니스 로직
│   ├── Interfaces/
│   └── Implementations/
├── Models/                   # 데이터 모델
│   ├── Entities/
│   ├── Enums/
│   └── DTOs/
├── State/                    # 전역 상태 관리
└── wwwroot/                  # 정적 파일
    ├── css/
    ├── js/
    └── lib/
```

## 🚀 시작하기

### 필수 요구사항
- .NET 8 SDK
- Node.js (선택사항)
- PlayFab 계정

### 설치 및 실행

1. **저장소 클론**
```bash
git clone 
cd ShopOwnerSimulator
```

2. **환경 변수 설정**
```bash
cp .env.example .env
# .env 파일 수정
PLAYFAB_TITLE_ID=your_title_id
PLAYFAB_SECRET_KEY=your_secret_key
```

3. **로컬 개발 실행**
```bash
dotnet watch run
```

브라우저에서 `https://localhost:5001` 접속

4. **프로덕션 빌드**
```bash
dotnet publish -c Release
```

## 🔄 CI/CD 파이프라인

GitHub Actions를 통한 자동 배포:

1. `main` 브랜치에 push
2. GitHub Actions 실행
3. ASP.NET Core 빌드
4. Cloudflare Pages 배포
5. 라이브 서비스 업데이트

## 🎯 게임 시스템

### 던전 시스템
- 여러 레벨의 던전 제공
- 용병 능력치에 따라 보상 시간 변동
- 자동 완료 및 보상 배분

### 제작 시스템
- 다양한 레시피 제공
- 재료 소비량 및 생산량 정의
- 성공/실패 로직

### 거래 시스템
- **개인 상점**: 정해진 가격으로 자동 판매
- **플레이어 거래소**: 플레이어 간 거래
- 시세 변동 시뮬레이션

### 용병 시스템
- 용병 고용 및 관리
- 경험치 및 레벨 시스템
- 장비 장착으로 능력치 상승
- 던전 투입으로 경험치 획득

## 📊 데이터 모델

### 주요 엔티티
- `Player`: 플레이어 정보
- `Mercenary`: 용병
- `InventoryItem`: 인벤토리 아이템
- `PersonalShopListing`: 개인 상점 등록
- `ExchangeOrder`: 거래소 주문
- `DungeonProgress`: 던전 진행 상태

## 🔐 보안

- PlayFab을 통한 플레이어 인증
- Environment Variables를 통한 비밀키 관리
- GitHub Secrets를 통한 자동 배포 시큐리티

## 📈 성능 최적화

- Blazor WebAssembly 최적화 (PublishTrimmed)
- 로컬 스토리지 캐싱
- API 요청 최소화
- CSS/JS 번들 최적화

## 🛡️ 에러 처리

- Try-catch 기반 예외 처리
- 사용자 친화적 에러 메시지
- 콘솔 로깅 및 디버깅

## 🎨 UI/UX 특징

- 반응형 디자인 (모바일 최적화)
- 직관적인 네비게이션
- 부드러운 애니메이션
- 진행 상태 시각화 (프로그레스 바, 타이머)

## 📝 개발 로드맵

### Phase 1: 기초 (완료)
- [x] 프로젝트 구조 설계
- [x] 핵심 시스템 구현
- [x] UI/UX 디자인

### Phase 2: 콘텐츠 추가 (진행중)
- [ ] 더 많은 던전 추가
- [ ] 특수 이벤트 시스템

---

**마지막 업데이트**: 2025-11-27
**버전**: 1.0.0