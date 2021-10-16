#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["member_center/member_center.csproj", "member_center/"]
RUN dotnet restore "member_center/member_center.csproj"
COPY . .
WORKDIR "/src/member_center"
RUN dotnet build "member_center.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "member_center.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "member_center.dll"]