FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY InterviewPlatform.Shared/InterviewPlatform.Shared.csproj InterviewPlatform.Shared/
COPY InterviewPlatform.Api/InterviewPlatform.Api.csproj InterviewPlatform.Api/
RUN dotnet restore InterviewPlatform.Api/InterviewPlatform.Api.csproj

COPY InterviewPlatform.Shared/ InterviewPlatform.Shared/
COPY InterviewPlatform.Api/ InterviewPlatform.Api/
RUN dotnet publish InterviewPlatform.Api/InterviewPlatform.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InterviewPlatform.Api.dll"]
