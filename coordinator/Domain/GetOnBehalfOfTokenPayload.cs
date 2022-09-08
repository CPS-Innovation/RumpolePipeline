namespace coordinator.Domain
{
    public class GetOnBehalfOfTokenPayload
    {
        public string Token { get; set; }

        public string RequestedScope { get; set; }
    }
}
