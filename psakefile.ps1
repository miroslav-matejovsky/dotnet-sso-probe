task PublishWeb {
    dotnet publish dotnet-sso-web-probe -c Release -o .\bin\dotnet-sso-web-probe
}

task PublishWpf {
    dotnet publish dotnet-sso-wpf-probe -c Release -r win-x64 -o .\bin\dotnet-sso-wpf-probe
}

task Clean {
    Remove-Item -Recurse -Force .\bin
}

task StartKeycloak {
    Start-Process -FilePath "docker" -ArgumentList "run --rm -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:21.1.1 start-dev"
}