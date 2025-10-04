# .NET SSO Probe

Web and WPF applications demonstrating Single Sign-On (SSO) with Keycloak.

## Keycloak Setup

1. In Keycloak, create a new realm named `sso-probe`.
2. Create a new client named `dotnet-sso-web-probe` with default settings.
3. Set the Valid Redirect URIs to `http://localhost:5000/*`.

## Web Application

A simple web application to demonstrate SSO with Keycloak.

```powershell
psake PublishWeb
```

## WPF Application

A simple WPF application to demonstrate SSO with Keycloak.

```powershell
psake PublishWpf
```
