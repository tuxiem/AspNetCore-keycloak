# ASP.NET Core - Keycloak authorization guide

## Introduction
This repo is for anyone interrested in adding Keycloak authorization to an ASP.NET Core app with OIDC.  
I had troubles finding resources about the subject for ASP.NET Core and got quite a lot of grey hair in the process.  
I ended up cracking the issue and created this solution and guide to help anyone having the same problem.  
Feel free to edit the code, by submitting a PR.  
May the code be with you :relaxed:  

## Start
Clone the repo and choose if you want the dotnet 3 or dotnet 6 project, then open KeycloakAuth.sln in Visual Studio or VS Code.  

## Appsettings.json
In order to get the application to work, you need to configure the application.json with the appropriate Keycloak metadata from your environment.  
Login to your Keycloak admin page, to get the the needed varibles.  
  
Name | Example Value | Docker env name
------------ | ------------- | -------------
ServerRealm | https://keycloak.example.com/auth/realms/keycloak-realm | Keycloak__ServerRealm
Metadata | https://keycloak.example.com/auth/realms/keycloak-realm/.well-known/openid-configuration | Keycloak__Metadata
ClientId | ![Keycloak ClientId](/images/Keycloak_1.png) | Keycloak__ClientId
ClientSecret | ![Keycloak ClientSecret](/images/Keycloak_3.png) | Keycloak__ClientSecret

## Policy vs. Roles
The code uses a policy example, but also comments on how to use roles within the code.  
The major difference is that based on claims you can create certain policies, that only users with claim x, y and z can access.  
If you have 15 different roles in your app, the authorize attribute can be quite confusing to read for each controller action.  
Use whatever suit your needs, the examples should be there.  

## Keycloak Configuration
In order to get authorization to work with Keycloak, you will need to add a new role to Client Scopes.  
1. Login to Keycloak Admin page
2. Goto Client Scopes
3. Goto Roles
4. Goto Mappers
5. Click Create
6. Give the new role a name
7. Mapper Type = User Client Role
8. Multivated must be on
9. Token claim name must be "role"
10. Add to access token must be on

![Keycloak Client Scope](/images/Keycloak_2.png)

## Login/Logout
I needed a silent authorization, there is no login or logout function built in.  
You can probably find other examples on github, where they do this.  

## Token exchange
There is a token exchange example in KeycloakAuthDotNet6 that needs to be uncommented in the HomeController, it implements middleware handling the exchange token and refresh token creation.  
Official documentation on token exchange: https://www.keycloak.org/docs/latest/securing_apps/#_token-exchange  
The appsettings needs to be updated with the Audience and token exchange point.
Name | Example Value | Docker env name
------------ | ------------- | -------------
TokenExchange | https://keycloak.example.com/auth/realms/keycloak-realm/protocol/openid-connect/token | Keycloak__TokenExchange
Audience | Client role connected to the service that needs to be token exchanged fx. "example-service", see https://www.keycloak.org/docs/latest/securing_apps/#internal-token-to-internal-token-exchange for more info | Keycloak__Audience
  
Depending on your token lifetime settings, it might be a good idea to do a refresh token before calling the token-exchange endpoint.  
1. Obtain an access token from the client
2. Use the refresh token, to get a new access token
3. Use the new access token, to do a token exchange on the audience connected to the other keycloak client
Remember to grant the nessesary roles and permissions on both clients.  

## Dotnet 6
In order to see tokens/claims in dotnet 6, you will have to install the package `System.IdentityModel.Tokens.Jwt`.  
For some reason, it's not updated in the authentication dependency.  

## Docker
In the repo, there is a dockerfile for the dotnet6 version, that can be used to build an image.  
If you are using another version, just find the correct image tag in docker hub.  
If you are using http and not https, you will need to change the aspnetcore ports accordingly in the file.  
Override environment variables with ``` -e varName="someVar" ```, see appsettings.json for the names to override.  

```dockerfile
Docker build . -t keycloakauth
Docker run -it --rm -p 5001:5001 keycloakauth
```

## Troubleshooting
### Invalid redirect URI
Keycloak tells you "invalid redirect uri" - you need to add your apps uri ex: https://localhost:44556 to the valid redirect URIs and web origins.

![Keycloak URI](/images/Keycloak_5.png)

### Access denied
You are presented with the access denied page.  
Copy your access token from the HomeController to jwt.io and look for what claims you have.  
They need to match the role names configured in Keycloak and in the policy.  
If you use Active Directory, sometimes the sync is very slow, renew your kerberos token and restart Keycloak or force a sync.  

![Keycloak Roles](/images/Keycloak_4.png)

### Token exchange failing
You are not granted an exchange token for your service.  
Insert a breakpoint in the TokenExchange.cs file, where the access token is returned and verify the claims are correct by validating the token in jwt.io.  
You should see, that the claims and settings belong to the exchanged client.  
If this is correct, validate the settings in Keycloak are correct, and that the service has permissions to exchange a token on service X.  
