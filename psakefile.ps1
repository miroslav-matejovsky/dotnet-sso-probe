task PublishWeb {
    dotnet publish dotnet-sso-web-probe -c Release -o .\bin\dotnet-sso-web-probe
}

task PublishWpf {
    dotnet publish dotnet-sso-wpf-probe -c Release -r win-x64 -o .\bin\dotnet-sso-wpf-probe
}

task Clean {
    Remove-Item -Recurse -Force .\bin
}