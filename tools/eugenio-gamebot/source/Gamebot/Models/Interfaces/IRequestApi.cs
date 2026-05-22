using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamebot.Models.Interfaces
{
    public interface IRequestApi
    {
        Task<ExternalResponse<Tout>> PostAsync<Tin, Tout>(string uri, Tin objectDat, Dictionary<string, string> attribute = null, string token = "") where Tin : class where Tout : class;
    }
}
