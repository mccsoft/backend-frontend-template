# How to send emails

This guide contains information about sending emails from projects based on Template App and how to configure it.

## SMTP settings

You can configure your SMTP settings in `appsettings.json`. By default there's an `Email` section that is set up like this:

```json
  "Email": {
    "SiteUrl": "https://templateapp.mcc-soft.de", // this is the URL of your website. It will be available in email templates, since you'll need them for URL links.
    "Host": "smtp.gmail.com",
    "Port": 587,
    "IsSecureConnection": true,
    "Login": "mcc.template.app@gmail.com",
    "Password": "871H2ZzBWAS5",
    "From": "mcc.template.app@gmail.com",
    "FromName": "Template App"
  }
```

You could send up to 100 emails per day via GMail account. If your project is small or just starting, the easiest way is to just register another GMail account and use it. After your first production deployments you could register on some of the widly used SMTP services: e.g. Mailersend, Sendpulse (which will give you 10k+ free monthly emails, but will verify your website) or Amazon (which is paid from the start).

## Sending emails

You start by calling `services.AddMailing(Configuration.GetSection("Email"));` in your `Startup.cs`. This will register a couple of service, most importantly `IMailSender`.

After that you could resolve `IMailSender` from DI and just

```csharp
var model = new ExampleEmailModel()
{
    Username = "Vasiliy Pupkin"
};
await mailSender.Send("mcc.template.app@gmail.com" /* address to send email to */, model);
```

You could check [TestDataController](https://github.com/mccsoft/backend-frontend-template/blob/99c5ae8e924d42940b576f7949add4822190e51e/webapi/src/MccSoft.TemplateApp.App/Controllers/TestDataController.cs#L39) for an example of sending an email.

## Create new templates

You could notice, that to send an email we just supplied a Model. However, we also need a template to render the model. Templates for `ExampleEmailModel` are in [MccSoft.TemplateApp.App/Views/Emails/Example](https://github.com/mccsoft/backend-frontend-template/tree/master/webapi/src/MccSoft.TemplateApp.App/Views/Emails/Example) For every new kind of email you should set up 3 things:

1. Add a Model class, inherit from `EmailModelBase`. Preferably the class name should end with `EmailModel` (e.g. `UserRegistrationEmailModel`).
1. Add a folder for email templates. Folder shall be named like your Model (e.g. `Views/Emails/UserRegistration`)
1. Create a templates for SUBJECT and CONTENT of your emails. You could just copy `ExampleEmailModel_content.cshtml` and `ExampleEmailModel_subject.cshtml` as a starting point.
1. Adjust your templates.

That's it!

## Testing senders locally

You can use **smtp4dev** to view and test your emails before actually integrating them.
Just execute the `Tokyo/scripts/smtp4dev/start_smtp4dev.bat` script, and set the **host** and **port** in the `appsettings.Development.json` to `localhost` and `2525` respectively.

You can access **Web UI** by opening http://localhost:3000
