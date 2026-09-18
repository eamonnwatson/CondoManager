# syntax=docker/dockerfile:1.7

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG VERSION=1.0.4

WORKDIR /src

COPY ["Directory.Build.props", "Directory.Packages.props", "CondoScope.slnx", "./"]
COPY ["src/CondoScope.Web/CondoScope.Web.csproj", "src/CondoScope.Web/"]
COPY ["src/CondoScope.Application/CondoScope.Application.csproj", "src/CondoScope.Application/"]
COPY ["src/CondoScope.Domain/CondoScope.Domain.csproj", "src/CondoScope.Domain/"]
COPY ["src/CondoScope.Persistence/CondoScope.Persistence.csproj", "src/CondoScope.Persistence/"]

RUN dotnet restore "src/CondoScope.Web/CondoScope.Web.csproj" -a $TARGETARCH

COPY . .

RUN dotnet publish "./src/CondoScope.Web/CondoScope.Web.csproj" \
	-c Release \
	-o /app/publish \
	-a $TARGETARCH \
	/p:Version=$VERSION \
	/p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV DATABASE_PATH=/data/condoscope.db
ENV ASPNETCORE_ENVIRONMENT=Production

RUN mkdir /data

EXPOSE 80
VOLUME ["/data"]

USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CondoScope.Web.dll"]