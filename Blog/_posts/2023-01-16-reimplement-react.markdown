---
layout: post
title:  "Refactoring to React"
date:   2023-01-16 20:36:58 -0400
categories: 
---

Since the beginning, CookTime was mainly an ASP.NET Razor page application with a small amount of React, mainly for the very interactive recipe edit page.
This was fine for a while, but I've been using React more at work and I've decided that Razor pages, while good, lack the development maturity, mindshare, and interactivity of React. 
I've resolved to reimplement the CookTime UI as a React single page application (SPA) with these goals:

1. Make frontend development interactive with Hot Module Reload (HMR)
1. Create re-useable and testable frontend components
1. Use proper DTOs instead of marshalling database objects between server and client (exploring using GraphQL for this?).

This post will be a development diary of all the changes necessary to accomplish this goal.
Let's dive in!

# Day 1: Getting something running
I'm going to use [create-react-app](https://create-react-app.dev) to scaffold the initial state of the client app.
I did this by running `npx create-react-app client-app --template typescript` at the root of the repo, which gets a React SPA set up in the `client-app` directory.

Next I need to integrate the frontend development server with the backend server.
This is required only for _development_ though setting it up is a little tedious and confusing so I'll try my best to explain it to future me.
In production, the ASP.NET server would be serving the SPA as static pre-compiled HTML, JS, and CSS.
This is great because clients can cache resources to make page loads faster and server resources are mostly spent on serving REST calls instead of rendering HTML for the client.
However this means that if we want HMR, the ASP.NET server has to open and maintain a connection to browser clients and push changes to files as they are made.
This used to be the case with `Microsoft.AspNetCore.SpaServices.Extensions` and to configure it you would have to add startup code like this:

```csharp
app.Map(
    "/js", ctx =>
    {
        ctx.UseSpa(spa =>
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:8080/js");
        });
    });
```

This tells ASP.NET that any call to a path starting with `/js` should be proxied to `http://localhost:8080/js` where, hopefully, a second server is running listing for filesystem events to deliver the nice HMR experience.
In this case, ASP.NET is the _first_ process that receives calls and decides to handle them or proxy them to another process.

The ASP.NET team decided they [don't want to maintain this anymore](https://github.com/dotnet/AspNetCore.Docs/issues/17902) citing "simplicity" arguments so we have to now invert which process handles the request first:
1. *Before*:  ASP.NET sees the request first, if it matches a specific path (like `/js`) the request is proxied to the React development server.
1. *After*: The React deveopment server sees the request first, if it matches a specific path (like `/api`) the request is proxied to the ASP.NET development server.

Same result? Yes. Better? Probably not. Does it make the web development world harder by deprecating a working solution? Yes!

So we replace `Microsoft.AspNetCore.SpaServices.Extensions` with `Microsoft.AspNetCore.SpaProxy` and modify our `csproj` file to tell it how to start the development server.
The full documentation for the new NuGet is [here](https://github.com/dotnet/AspNetCore.Docs/issues/26373) and that page does a better job than me.

With all that done, the standard `create-react-app` page loads with the spinning Rutherfordian atom.

Success!
![Create react app template](assets/react-day1.png)

# Day 2: Authentication and Authorization

Before we get to the fun parts of re-writing the Razor pages as React components, we have to deal with the un-fun features of signing in and authorizing users.
Addressing this led me down a rabbit-hole of authentication acronymns and open-ended questioning whether or not you should be able to use your Google account to sign in.
In the end, I decided that CookTime will
1. *Continue* to be its own authentication and authorization provider.
1. *Continue* to use ASP.NET Identity cookies to identify users.

The _alternative_ of course would have been to 
1. Delegate to an authentication provider (like Google, Facebook, Apple, Microsoft, etc...) and implement the correct [OIDC](https://openid.net/connect/) flow
1. Configure _each_ authentication provider and map claims appropriately so CookTime doesn't care who provided the JWT.

I simply do not have the patience or desire to setup OAuth authentication for CookTime and if this costs us users then so be it.
The whole process is mind-meltingly boring.

Having decided that, I need to reimplement the Identity flows using React components instead of `forms` for
1. Registration
1. Sign in
1. Sign out
1. Password reset
1. Email verification

## Registration
The registration form is "simple" (Ha! Nothing with authentication is simple!).
A user providers the following information (taken from the current sign up form).

![Alt text](assets/react-day2-1.png)

Then in the backend, we take this and check to see if a user with the same `username` _or_ `email` has previously been registered.
Assuming not, we try to create the user and if their password passes the complexity checks then they get a database entry in the `AspNetUsers` table.
We also fire off an email to the email provided to validate that this is a real email!
It should be simple enough to:
1. Make a React form that accepts these 4 inputs
1. POST the information to the server
1. Create the user if needed

## References
1. https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies
1. https://jcbaey.com/authentication-in-spa-reactjs-and-vuejs-the-right-way/?utm_source=medium&utm_campaign=spa-authentication
