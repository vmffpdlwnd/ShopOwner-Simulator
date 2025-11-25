# ShopOwner Simulator

던전에서 얻은 아이템을 판매하는 상점 시뮬레이터 게임

## 기술 스택

- **프론트엔드**: Blazor WebAssembly (.NET 8)
- **백엔드**: ASP.NET Core Web API (.NET 6)
- **배포**: 
  - 클라이언트: Cloudflare Pages
  - API 서버: AWS/Azure (추후 결정)

## 프로젝트 구조

```
ShopOwner-Simulator/
├── ShopOwnerSimulator/              # API 서버
│   ├── Controllers/                 # API 컨트롤러
│   ├── Models/                      # 데이터 모델
│   ├── Services/                    # 비즈니스 로직
│   └── Program.cs                   # API 서버 진입점
├── ShopOwnerSimulator.Client/       # Blazor WASM 클라이언트
│   ├── Pages/                       # Blazor 페이지
│   ├── Services/                    # API 클라이언트 서비스
│   ├── Models/                      # 클라이언트 모델
│   └── Program.cs                   # 클라이언트 진입점
└── .github/workflows/               # GitHub Actions
```

## 로컬 개발 환경 설정

### 1. API 서버 실행

```powershell
cd ShopOwnerSimulator
dotnet run
```

API 서버: `https://localhost:5001`  
Swagger UI: `https://localhost:5001/swagger`

### 2. Blazor 클라이언트 실행

새 터미널에서:

```powershell
cd ShopOwnerSimulator.Client
dotnet run
```

클라이언트: `https://localhost:5101`

## 배포

자세한 배포 가이드는 [CLOUDFLARE_DEPLOY.md](CLOUDFLARE_DEPLOY.md)를 참조하세요.

### Cloudflare Pages 자동 배포

`main` 브랜치에 푸시하면 GitHub Actions가 자동으로 Cloudflare Pages에 배포합니다.

필요한 GitHub Secrets:
- `CLOUDFLARE_API_TOKEN`
- `CLOUDFLARE_ACCOUNT_ID`

## 기능

- [ ] 사용자 생성 및 로그인
- [ ] 던전 탐험
- [ ] 아이템 획득
- [ ] 상점 운영
- [ ] 아이템 판매
- [ ] 경제 시스템

