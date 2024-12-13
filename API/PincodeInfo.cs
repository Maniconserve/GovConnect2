using Newtonsoft.Json;
[JsonObject]
public class PostOfficeResponse
{
    [JsonProperty("Message")]
    public string Message { get; set; }

    [JsonProperty("Status")]
    public string Status { get; set; }

    [JsonProperty("PostOffice")]
    public List<PostOffice> PostOffice { get; set; }
}
[JsonObject]
public class PostOffice
{
    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Description")]
    public string Description { get; set; }

    [JsonProperty("BranchType")]
    public string BranchType { get; set; }

    [JsonProperty("DeliveryStatus")]
    public string DeliveryStatus { get; set; }

    [JsonProperty("Circle")]
    public string Circle { get; set; }

    [JsonProperty("District")]
    public string District { get; set; }

    [JsonProperty("Division")]
    public string Division { get; set; }

    [JsonProperty("Region")]
    public string Region { get; set; }

    [JsonProperty("State")]
    public string State { get; set; }

    [JsonProperty("Country")]
    public string Country { get; set; }

    [JsonProperty("Pincode")]
    public string Pincode { get; set; }
}
