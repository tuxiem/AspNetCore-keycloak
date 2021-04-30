# ASP.NET Core - Keycloak authorization guide

## Introduction
This repo is for anyone interrested in adding keycloak authorization to a C# app.  
I had troubles finding resources about the subject for ASP.NET Core and got quite a lot of grey hair in the process.  
I ended up cracking the issue and created this solution and guide to help anyone having the same problem.  
Feel free to edit the code, by submitting a PR.  
May the code be with you :relaxed:  

## Start
Clone the repo and open KeycloakAuth.sln in Visual Studio or VS Code.  

## Appsettings.json
In order to get the application to work, you need to configure the application.json with the appropriate Keycloak metadata from your environment.  
Login to your Keycloak admin page, to get the the needed varibles.  
  
Name | Example Value | Docker env name
------------ | ------------- | -------------
ServerRealm | https://keycloak.example.com/auth/realms/keycloak-realm" | Keycloak__ServerRealm
Metadata | "https://keycloak.example.com/auth/realms/keycloak-realm/.well-known/openid-configuration" | Keycloak__Metadata
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

## Docker
In the repo, there is a dockerfile that can be used to build an image.  
If you are using http and not https, you will need to change the aspnetcore ports accordingly in the file.  
Override environment variables with ``` -e varName="someVar" ```, see appsettings.json for the names to override.  

```dockerfile
Docker build . -t keycloakauth
Docker run -it --rm -p 5001:5001 keycloakauth
```

## Errors
Keycloak tells you "invalid redirect uri" - you need to add your apps uri ex: https://localhost:44556 to the valid redirect URIs and web origins.

![Keycloak URI](/images/Keycloak_5.png)

You are presented with the access denied page.  
Copy your access token from the HomeController to jwt.io and look for what claims you have.  
They need to match the role names configured in Keycloak and in the policy.  
If you use Active Directory, sometimes the sync is very slow, renew your kerberos token and restart Keycloak or force a sync.  

![Keycloak Roles](/images/Keycloak_4.png)
