FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY InterviewPlatform.Shared/InterviewPlatform.Shared.csproj InterviewPlatform.Shared/
COPY InterviewPlatform.Worker/InterviewPlatform.Worker.csproj InterviewPlatform.Worker/
RUN dotnet restore InterviewPlatform.Worker/InterviewPlatform.Worker.csproj

COPY InterviewPlatform.Shared/ InterviewPlatform.Shared/
COPY InterviewPlatform.Worker/ InterviewPlatform.Worker/
RUN dotnet publish InterviewPlatform.Worker/InterviewPlatform.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "InterviewPlatform.Worker.dll"]
