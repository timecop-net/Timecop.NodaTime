FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/Timecop.NodaTime/Timecop.NodaTime.csproj ./src/Timecop.NodaTime/Timecop.NodaTime.csproj
COPY test/Timecop.NodaTime.Tests/Timecop.NodaTime.Tests.csproj ./test/Timecop.NodaTime.Tests/Timecop.NodaTime.Tests.csproj
RUN dotnet restore

# copy everything else and build app
COPY ./ ./
WORKDIR /source
RUN dotnet build -c release -o /out/package --no-restore

FROM build as test

RUN dotnet test

FROM build as pack-and-push
WORKDIR /source

ARG PackageVersion
ARG NuGetApiKey

RUN dotnet pack ./src/Timecop.NodaTime/Timecop.NodaTime.csproj -o /out/package -c Release
RUN dotnet nuget push /out/package/Timecop.NodaTime.$PackageVersion.nupkg -k $NuGetApiKey -s https://api.nuget.org/v3/index.json