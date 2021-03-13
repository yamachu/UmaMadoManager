using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UmaMadoManager.Core.Services;

namespace UmaMadoManager.Windows.Services
{
    public class VersionRepository : IVersionRepository
    {
        internal class VersionJson
        {
            public string Stable { get; set; }
        }

        public async Task<string> GetLatestVersion()
        {
            try
            {
                var json = await new HttpClient().GetFromJsonAsync<VersionJson>("https://raw.githubusercontent.com/yamachu/UmaMadoManager/master/UmaMadoManager.Windows/Version.json");
                return json.Stable;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                return "";
            }
        }
    }
}
