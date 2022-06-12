# Authentication
This project contains OpenIddict as an authentication server and JWT token provider.

OpenIddict is set up using [OpenIddictExternalAuthentication](https://github.com/Shaddix/OpenIddictExternalAuthentication). Check their Readme for documentation.

## Authentication page / flow
This project contains Razor-based authentication page.
1. Authentication page is not a part of SPA, because Resource-Owner-Password-Flow (i.e. login/password pair entered on some arbitrary page and sent to server via http API call) is considered insecure.
2. So to authenticate from SPA we need to use Auth-Code-Flow, and in this flow AuthenticationServer should have it's own UI (i.e. login form).
3. You could reuse CSS classes/variables from SPA within authentication page. Check [razor.scss](frontend/src/razor.scss) that is compiled to [frontend.css](webapi/src/MccSoft.TemplateApp.App/wwwroot/css/frontend.css).
    1. Authentication page should have as little JS as possible, so it's not possible to inject malicious code into it.

### How to authenticate from SPA
There are generally 2 ways of how authentication should be integrated in SPA in terms of UI:
1. If you only use some 3rd party authentication (i.e. Google or Azure AD)
   1. You could place 3rd party authentication buttons (e.g. 'Login with Google') right within your SPA. When user presses the button, call `openExternalLoginPopup('Google')`, passing the name of your 3rd party provider.
   2. When user presses the button popup window will open with Google authentication UI.
   3. When user authenticates, the promise returned from `openExternalLoginPopup` will succeed and you could use your user's access/refresh token.
2. If you use project-specific login/passwords (i.e. you need to store your users Logins/passwords in your DB), i.e. 3rd party auth is not enough.
   1. In this case you'd need to present the login form to the user, and this form is NOT located within SPA.
   2. So, you to start this flow run `redirectToLoginPage()` function.
   3. It will actually redirect the user to the server Login UI.
   4. To receive the result of login process, you need to pass `signInRedirectHandler` prop to `<OpenIdCallback />` component wrapping your application.
      1. it's not possible to use promises here, since user actually leaves your page, so all JS is unloaded.

For both ways, check the basic code from [openid folder](frontend/src/pages/unauthorized/openid), and make sure that your URL and client id settings (specified in [openid-settings.ts](frontend/src/pages/unauthorized/openid/openid-settings.ts)) are ok .
General idea is that Auth-Code flow involves http redirects that needs to be handled by SPA. In our case, these redirects are handled by `<OpenIdCallback />` component, that should wrap your whole application.

Technically you could also use redirects for 3rd party auth (by altering `openExternalLoginPopup` and changing `getManager().signinPopup` to `getManager().signinRedirect`) or use popup for your own login/passwords (by altering `redirectToLoginPage` and calling `getManager().signinPopup()`), but User Experience in these cases is usually suboptimal.

# What about BFF (Backend For Frontend)
We are considering it as well, but it's not yet decided/implemented.

Practically, e.g. Auth0 consider [it's ok](https://community.auth0.com/t/can-i-save-refresh-token-into-localstorage-if-refresh-token-rotation-is-enabled/46761/3) to use access/refresh tokens in localstorage if refresh token rotation is implemented (we do have refresh token rotation configured).
