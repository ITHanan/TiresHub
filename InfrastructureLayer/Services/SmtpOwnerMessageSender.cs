using System.Net;
using System.Net.Mail;
using ApplicationLayer.Features.OwnerCommunication;
using Microsoft.Extensions.Configuration;

namespace InfrastructureLayer.Services;

public sealed class SmtpOwnerMessageSender : IOwnerMessageSender
{
    private readonly IConfiguration _configuration;
    public SmtpOwnerMessageSender(IConfiguration configuration) => _configuration = configuration;

    public async Task SendEmailAsync(string recipient, string message, IReadOnlyList<string> evidenceUrls, CancellationToken ct)
    {
        var settings = _configuration.GetSection("OwnerCommunication:Smtp");
        var host = settings["Host"];
        var from = settings["From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("SMTP is not configured.");
        var allowedHosts = (settings["AllowedPhotoHosts"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (allowedHosts.Length == 0 || evidenceUrls.Count > 10)
            throw new InvalidOperationException("Photo host allowlist is missing or too many photos were supplied.");
        var port = int.TryParse(settings["Port"], out var configuredPort) ? configuredPort : 587;
        using var mail = new MailMessage(from, recipient)
        {
            Subject = "Vehicle inspection report",
            Body = message + "\n\nInspection evidence:\n" + string.Join("\n", evidenceUrls)
        };
        using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        http.Timeout = TimeSpan.FromSeconds(30);
        const int maxPhotoBytes = 5 * 1024 * 1024;
        for (var index = 0; index < evidenceUrls.Count; index++)
        {
            if (!Uri.TryCreate(evidenceUrls[index], UriKind.Absolute, out var photoUri)
                || photoUri.Scheme != Uri.UriSchemeHttps
                || !allowedHosts.Contains(photoUri.IdnHost, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("A photo URL is not from an allowed HTTPS host.");
            using var response = await http.GetAsync(photoUri, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType is not ("image/jpeg" or "image/png" or "image/webp")
                || response.Content.Headers.ContentLength > maxPhotoBytes)
                throw new InvalidOperationException("Photo type or size is not supported.");
            using var input = await response.Content.ReadAsStreamAsync(ct);
            var data = new MemoryStream();
            var buffer = new byte[81920];
            int read;
            while ((read = await input.ReadAsync(buffer, ct)) > 0)
            {
                if (data.Length + read > maxPhotoBytes) { data.Dispose(); throw new InvalidOperationException("Photo is too large."); }
                await data.WriteAsync(buffer.AsMemory(0, read), ct);
            }
            data.Position = 0;
            var extension = mediaType == "image/jpeg" ? "jpg" : mediaType == "image/png" ? "png" : "webp";
            mail.Attachments.Add(new Attachment(data, $"inspection-{index + 1}.{extension}", mediaType));
        }
        using var client = new SmtpClient(host, port) { EnableSsl = true };
        if (!string.IsNullOrWhiteSpace(settings["Username"]))
            client.Credentials = new NetworkCredential(settings["Username"], settings["Password"]);
        await client.SendMailAsync(mail, ct);
    }
}
