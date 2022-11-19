using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using MccSoft.Mailing.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;

namespace MccSoft.Mailing;

public class MailSender : IMailSender
{
    private readonly ILogger _logger;
    private readonly IOptions<MailSenderOptions> _options;
    private readonly MailSettings _mailSettings;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRazorRenderService _razorRenderService;

    public MailSender(
        IRazorRenderService razorRenderService,
        IOptions<MailSenderOptions> options,
        MailSettings mailSettings,
        IBackgroundJobClient backgroundJobClient,
        ILogger<MailSender> logger
    )
    {
        _razorRenderService = razorRenderService;
        _options = options;
        _mailSettings = mailSettings;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public List<EmailModelBase> InterceptedModels { get; private set; } = new();

    private readonly HashSet<Type> _interceptedTypes = new HashSet<Type>();

    /// <summary>
    /// Returns unsubscribe action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Action Intercept<T>() where T : EmailModelBase
    {
        _interceptedTypes.Add(typeof(T));
        return () =>
        {
            _interceptedTypes.Remove(typeof(T));
        };
    }

    public async Task Send<T>(string recipient, T model, List<string> attachments = null)
        where T : EmailModelBase
    {
        model.SiteRootUrl = _options.Value.SiteUrl;
        model.RecipientEmail = recipient;
        if (_interceptedTypes.Contains(typeof(T)))
        {
            InterceptedModels.Add(model);
            return;
        }

        (string Subject, string Content) rendered = await RenderContentAndSubject(model);

        if (!string.IsNullOrEmpty(rendered.Content))
        {
            _backgroundJobClient.Enqueue<MailSender>(
                x => x.Send(recipient, rendered.Subject, rendered.Content, null, attachments)
            );
        }
    }

    private async Task<(string Subject, string Content)> RenderContentAndSubject<T>(T model)
        where T : EmailModelBase
    {
        try
        {
            _logger.LogInformation($"Rendering template for model: {model.GetType()}");

            string subjectViewPath = _mailSettings.EmailViewPathProvider(
                model,
                EmailTemplateType.Subject
            );
            string subject = await RenderTemplate(model, subjectViewPath);
            model.Subject = subject;

            string contentViewPath = _mailSettings.EmailViewPathProvider(
                model,
                EmailTemplateType.Content
            );
            string content = await RenderTemplate(model, contentViewPath);
            return (subject, content);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error rendering template. ModelType: {model.GetType()}");
            throw;
        }
    }

    private static List<string> _globalAttachments = new() { "Views/Emails/Images/logo.png", };

    [AutomaticRetry(
        Attempts = 10,
        DelaysInSeconds = new[] { 60, 120, 240, 480, 600, 1810, 3610, 4010, 5450, 7300 }
    )]
    public virtual async Task Send(
        string recipient,
        string subject,
        string text,
        string from = null,
        List<string> attachments = null
    )
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.Value.FromName, from ?? _options.Value.From));
        message.To.Add(new MailboxAddress("", recipient));

        var bodyBuilder = new BodyBuilder();

        foreach (var attachment in _globalAttachments)
        {
            MimeEntity entity = bodyBuilder.LinkedResources.Add(attachment);
            entity.ContentId = Path.GetFileName(attachment);
        }

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                MimeEntity entity = bodyBuilder.LinkedResources.Add(attachment);
                entity.ContentId = Guid.NewGuid().ToString();
            }
        }

        bodyBuilder.HtmlBody = text;
        message.Subject = subject;

        message.Body = bodyBuilder.ToMessageBody();

        List<string> sentTo = message.To.Union(message.Bcc).Select(x => x.ToString()).ToList();
        _logger.LogInformation($"Sending email on '{subject}' to {string.Join(", ", sentTo)}");

        await RawSendEmail(message);

        _logger.LogInformation($"Email on '{subject}' was sent to {string.Join(", ", sentTo)}");
    }

    private async Task RawSendEmail(MimeMessage message)
    {
        using var client = new SmtpClient();
        var mailOptions = _options.Value;
        await client.ConnectAsync(
            mailOptions.Host,
            mailOptions.Port,
            mailOptions.IsSecureConnection
        );

        await client.AuthenticateAsync(mailOptions.Login, mailOptions.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    internal async Task<string> RenderTemplate<T>(T model, string viewPath)
    {
        // var viewName = DatabaseFileProvider.SerializeToFilePath(new DatabaseFileProvider.EmailTemplateIdentifier
        // {
        //     Guid = emailTemplateId ?? Guid.Empty,
        //     Type = templateName,
        //     SubType = subType,
        // });

        try
        {
            return await _razorRenderService.RenderToStringAsync(viewPath, model);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"There was a problem compiling Razor Email template."
                    + $"Error: {e.Message}. ViewName: {viewPath}. ModelType: {model.GetType()}"
                    + $"Stack trace: {e.StackTrace}",
                e
            );
        }
    }
}
