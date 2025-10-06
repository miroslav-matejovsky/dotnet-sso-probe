# .NET SSO Probe

Web and WPF applications demonstrating Single Sign-On (SSO) with Keycloak.

## Keycloak Setup

Start keycloak using Podman or Docker using `podman compose up` or `docker compose up`.

1. In Keycloak, create a new realm named `sso-probe`.
2. On realm settings -> User Profile remove first name and last name required attributes. (This is optional but simplifies user creation.)
3. Create test user in the `sso-probe` realm for example, username: `test`, password: `test`.

### Entra ID SAML Identity Provider

1. Create a new Entra ID Enterprise Application in the Azure portal.
2. Configure SAML-based SSO for the application.
3. Set the `Identifier (Entity ID)` to `http://localhost:8080/realms/sso-probe`
4. Set the `Reply URL (Assertion Consumer Service URL)` to `http://localhost:8080/realms/sso-probe/broker/entraid-saml/endpoint`
5. Copy the `API Federation Metadata URL` from the `Single sign-on` SAML configuration.
6. In Keycloak, navigate to the `sso-probe` realm.
7. Go to Identity Providers and select `SAML v2.0`.
8. Set the Alias to `entraid-saml`.
9. Paste the `API Federation Metadata URL` into the `Import from URL` (you should see green dot-check icon).
10. Click on `Add` and then `Save` to create the identity provider.

### Mapping SAML to Keycloak Attributes

First you will probably see some identifier mapped from Entra.
Search this identifier in the keycloak debug logs and you should find SAML xml response where you can find received claims.

Then, create a new mapper in the `entraid-saml` identity provider:

1. username: `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name`
2. email: `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name`

## web-probe

A simple web application to demonstrate SSO with Keycloak.

### Keycloak client

1. Create a new client named `sso-dotnet-web-probe` with default settings.
2. Set the Valid Redirect URIs to `http://localhost:5000/*`.
3. Set the Web Origins to `http://localhost:5000`.

### Publishing

```powershell
psake PublishWeb
```

## wpf-probe

A simple WPF application to demonstrate SSO with Keycloak.

### Keycloak client

For token exchange to work, the client must be configured with authentication and `Standard Token Exchange` enabled.

1. Create a new client named `sso-dotnet-wpf-probe` with `Client Authentication` enabled.
2. Enable `Client Authentication` so that `Standard Token Exchange` can be used.
3. Check `Standard Token Exchange` in the `Authentication Flow` section.

!!! SAML identity providers are not supported at this time in Keycloak for token exchange !!!

### Publishing

```powershell
psake PublishWpf
```

## Links

- <https://www.tymiq.com/post/seamless-sso-for-desktop-applications>
- [External to Internal Keycloak Token Exchange](https://www.keycloak.org/securing-apps/token-exchange#_external-token-to-internal-token-exchange)
