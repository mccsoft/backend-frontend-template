using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MccSoft.Mailing.Models;

namespace MccSoft.Mailing;

public interface IMailSender
{
    List<EmailModelBase> InterceptedModels { get; }
    Action Intercept<T>() where T : EmailModelBase;

    Task Send(
        string recipient,
        string subject,
        string text,
        string from = null,
        List<string> attachments = null
    );

    Task Send<T>(string recipient, T model, List<string> attachments = null)
        where T : EmailModelBase;
}
