namespace Scynett.Hubtel.Payments.Configuration;

public class HubtelSettings
{
    public const string SectionName = "Hubtel";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string MerchantAccountNumber { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.hubtel.com";
    public int TimeoutSeconds { get; set; } = 30;
    public string PrimaryCallbackEndPoint { get; internal set; }
}
