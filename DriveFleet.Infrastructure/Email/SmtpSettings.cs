using DriveFleet.Domain.Entities;

namespace DriveFleet.Infrastructure.Email;

/// <summary>
/// Represents the SMTP configuration used to send emails.
/// </summary>
public class SmtpSettings
{
    //endereço do servidor SMTP
    public string Host { get; set; } = string.Empty;

    //porta usada pelo SMTP
    public int Port { get; set; }

    //conta usada para autenticação
    public string Username { get; set; } = string.Empty;

    //password/app password SMTP
    public string Password { get; set; } = string.Empty;

    //endereço que aparece como remetente
    public string FromEmail { get; set; } = string.Empty;

    //nome apresentado ao utilizador
    public string FromName { get; set; } = string.Empty;

    //se a ligação deve usar SSL/TLS
    public bool UseSsl { get; set; }
}
