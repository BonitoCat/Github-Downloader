dotnet restore ../Github-Downloader-cli/Github-Downloader-cli.csproj -r linux-x64
dotnet publish ../Github-Downloader-cli/Github-Downloader-cli.csproj \
    --no-restore \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    --output ./github-downloader-cli-linux-x64/usr/local/bin \
    /p:PublishSingleFile=true \
    /p:PublishReadyToRun=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

read _
