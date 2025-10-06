# .NET SSO Probe

Web and WPF applications demonstrating Single Sign-On (SSO) with Keycloak.

## Keycloak Setup

Start keycloak using Podman or Docker using `podman compose up` or `docker compose up`.

1. In Keycloak, create a new realm named `sso-probe`.
2. Create a new client named `sso-dotnet-web-probe` with default settings.
3. Set the Valid Redirect URIs to `http://localhost:5000/*`.
4. Create test user in the `sso-probe` realm for example, username: `test`, password: `test`.

### Entra ID SAML Identity Provider

1. Create a new Entra ID Enterprise Application in the Azure portal.
2. Configure SAML-based SSO for the application.
3. Copy the `API Federation Metadata URL` from the `Single sign-on` SAML configuration.
4. In Keycloak, navigate to the `sso-probe` realm.
5. Go to Identity Providers and select `SAML v2.0`.
6. Set the Alias to `entraid-saml`.


## web-probe

A simple web application to demonstrate SSO with Keycloak.

To publish the web application, run:

```powershell
psake PublishWeb
```

## wpf-probe

A simple WPF application to demonstrate SSO with Keycloak.

To publish the WPF application, run:

```powershell
psake PublishWpf
```
