# Cloudflare Pages 배포 가이드

## 1. Blazor WebAssembly 빌드

```powershell
cd ShopOwnerSimulator.Client
dotnet publish -c Release -o ../publish
```

빌드 결과물은 `publish/wwwroot` 폴더에 생성됩니다.

## 2. Cloudflare Pages 설정

### 프로젝트 설정
- **Framework preset**: None (또는 Custom)
- **Build command**: `dotnet publish ShopOwnerSimulator.Client/ShopOwnerSimulator.Client.csproj -c Release -o publish`
- **Build output directory**: `publish/wwwroot`

### 환경 변수
빌드 환경에 .NET SDK가 필요하므로 다음 환경 변수를 설정:
- `DOTNET_VERSION`: `8.0` (또는 사용 중인 .NET 버전)

## 3. API 서버 배포

API 서버(`ShopOwnerSimulator`)는 별도로 배포해야 합니다:
- **옵션 1**: AWS App Runner, Elastic Beanstalk
- **옵션 2**: Azure App Service
- **옵션 3**: Docker 컨테이너 (AWS ECS, Azure Container Instances 등)

## 4. 프로덕션 API URL 설정

배포 후 `appsettings.json`의 `ApiBaseAddress`를 실제 API 서버 주소로 변경:

```json
{
  "ApiBaseAddress": "https://your-api-server.com/"
}
```

## 5. CORS 설정

API 서버의 `Program.cs`에서 Cloudflare Pages 도메인을 CORS에 추가:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:5001", 
            "https://localhost:5001",
            "https://your-cloudflare-pages.pages.dev"  // 추가
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
```

## 6. 로컬 테스트

### API 서버 실행
```powershell
cd ShopOwnerSimulator
dotnet run
```

### Blazor 클라이언트 실행
```powershell
cd ShopOwnerSimulator.Client
dotnet run
```

브라우저에서 `https://localhost:5001` 접속

## 참고사항

- Blazor WASM은 정적 파일로 배포되므로 Cloudflare Pages에 최적화되어 있습니다
- API 서버는 별도 호스팅이 필요합니다 (백엔드 로직과 PlayFab SDK 사용 때문)
- GitHub Actions를 통한 자동 배포도 가능합니다
