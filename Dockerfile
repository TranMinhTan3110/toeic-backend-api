# Dùng .NET 9 SDK để build code
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy các file cấu hình project (để Docker cache lại bước restore thư viện)
COPY ["Src/ToeicBackend.API/ToeicBackend.API.csproj", "Src/ToeicBackend.API/"]
COPY ["Src/ToeicBackend.Application/ToeicBackend.Application.csproj", "Src/ToeicBackend.Application/"]
COPY ["Src/ToeicBackend.Infrastructure/ToeicBackend.Infrastructure.csproj", "Src/ToeicBackend.Infrastructure/"]
COPY ["Src/ToeicBackend.Domain/ToeicBackend.Domain.csproj", "Src/ToeicBackend.Domain/"]
COPY ["Src/ToeicBackend.Seeder/ToeicBackend.Seeder.csproj", "Src/ToeicBackend.Seeder/"]

# Khôi phục các thư viện
RUN dotnet restore "Src/ToeicBackend.API/ToeicBackend.API.csproj"

# Copy toàn bộ code thực tế vào
COPY . .
WORKDIR "/src/Src/ToeicBackend.API"

# Tiến hành Build và Publish ra file tối ưu nhất
RUN dotnet publish "ToeicBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------
# Tạo một bể cá SIÊU NHẸ chỉ dùng để chạy (không chứa rác của quá trình build)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Mở cổng 8080 cho bên ngoài gọi vào
EXPOSE 8080

# Chép thành phẩm từ bước build sang đây
COPY --from=build /app/publish .

# Định nghĩa cổng chạy mặc định cho .NET 9
ENV ASPNETCORE_HTTP_PORTS=8080

# Lệnh khởi chạy API
ENTRYPOINT ["dotnet", "ToeicBackend.API.dll"]
