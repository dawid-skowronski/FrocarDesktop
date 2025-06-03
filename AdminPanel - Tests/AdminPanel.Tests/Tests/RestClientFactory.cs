using RestSharp;

namespace AdminPanel.Tests
{
    public static class RestClientFactory
    {
        private static IRestClient _client;

        public static void SetClient(IRestClient client)
        {
            _client = client;
        }

        public static IRestClient CreateClient(string baseUrl)
        {
            return _client ?? new RestClient(baseUrl);
        }
    }
}