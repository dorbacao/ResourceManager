using ResourceManagerSample;
using System.Net.Http;
using System.Net.Http.Json;

public class EmailApiClient : IDisposable
{
    private readonly HttpClient _http = new HttpClient();
    private readonly string _baseUrl = "http://localhost:5193";

    public async Task<Guid> SubmitEmailAsync(string to, string subject, string body)
    {
        var payload = new
        {
            To = to,
            Subject = subject,
            Body = body
        };

        var response = await _http.PostAsJsonAsync($"{_baseUrl}/email/pending", payload);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<PendingEmailResponse>();

        var rm = EmailTransactionManager.GetResourceManager();
        rm.AddEmailId(json!.Id);

        return json!.Id;
    }
    public async Task ConfirmAsync(Guid emailId)
    {
        var response = await _http.PostAsync($"{_baseUrl}/email/confirm/{emailId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelAsync(Guid emailId)
    {
        var response = await _http.PostAsync($"{_baseUrl}/email/cancel/{emailId}", null);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _http.Dispose();
    }
}

public class PendingEmailResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "";
}
