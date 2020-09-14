using SaeClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaeClient.Services.Interfaces
{
    public interface IApiSaeService
    {
        Task<IEnumerable<string>> ConsultaDiasDisponiveis(int NroIntLocalServico);
        Task<IEnumerable<MunicipioDto>> ConsultaMunicipiosPorServico(int NroIntServico);
    }
}
