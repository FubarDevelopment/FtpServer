FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["samples/TestFtpServer/TestFtpServer.csproj", "samples/TestFtpServer/"]
COPY ["samples/TestFtpServer.Api/TestFtpServer.Api.csproj", "samples/TestFtpServer.Api/"]
COPY ["samples/TestFtpServer.Shell/TestFtpServer.Shell.csproj", "samples/TestFtpServer.Shell/"]
COPY ["src/FubarDev.FtpServer.FileSystem.InMemory/FubarDev.FtpServer.FileSystem.InMemory.csproj", "src/FubarDev.FtpServer.FileSystem.InMemory/"]
COPY ["src/FubarDev.FtpServer.Abstractions/FubarDev.FtpServer.Abstractions.csproj", "src/FubarDev.FtpServer.Abstractions/"]
COPY ["src/FubarDev.FtpServer.FileSystem.DotNet/FubarDev.FtpServer.FileSystem.DotNet.csproj", "src/FubarDev.FtpServer.FileSystem.DotNet/"]
COPY ["src/FubarDev.FtpServer.MembershipProvider.Pam/FubarDev.FtpServer.MembershipProvider.Pam.csproj", "src/FubarDev.FtpServer.MembershipProvider.Pam/"]
COPY ["src/FubarDev.FtpServer.FileSystem.Unix/FubarDev.FtpServer.FileSystem.Unix.csproj", "src/FubarDev.FtpServer.FileSystem.Unix/"]
COPY ["src/FubarDev.FtpServer.FileSystem.GoogleDrive/FubarDev.FtpServer.FileSystem.GoogleDrive.csproj", "src/FubarDev.FtpServer.FileSystem.GoogleDrive/"]
COPY ["src/FubarDev.FtpServer/FubarDev.FtpServer.csproj", "src/FubarDev.FtpServer/"]
COPY ["src/FubarDev.FtpServer.Commands/FubarDev.FtpServer.Commands.csproj", "src/FubarDev.FtpServer.Commands/"]
RUN dotnet restore "samples/TestFtpServer/TestFtpServer.csproj"
RUN dotnet restore "samples/TestFtpServer.Shell/TestFtpServer.Shell.csproj"
COPY . .
WORKDIR "/src/samples/TestFtpServer"
RUN dotnet build "TestFtpServer.csproj" -c Release -o /app
WORKDIR "/src/samples/TestFtpServer.Shell"
RUN dotnet build "TestFtpServer.Shell.csproj" -c Release -o /app

FROM build AS publish
WORKDIR "/src/samples/TestFtpServer"
RUN dotnet publish "TestFtpServer.csproj" -c Release -o /app
WORKDIR "/src/samples/TestFtpServer.Shell"
RUN dotnet publish "TestFtpServer.Shell.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir /usr/share/SharpFtpServer
RUN echo "{ \"authentication\": \"pam\", \"umask\": \"0002\", \"server\": { \"pasv\": { \"range\": \"10000:10099\" } }, \"layout\": \"pam-home\", \"backend\": \"unix\" }" > /usr/share/SharpFtpServer/appsettings.Production.json
VOLUME ["/usr/share/SharpFtpServer"]
EXPOSE 21
EXPOSE 10000-10099
ENTRYPOINT ["dotnet", "ftpserver.dll"]
