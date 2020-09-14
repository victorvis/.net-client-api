using SaeClient.Services.Interfaces;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SaeClient.Controllers
{
    public class ClientController : Controller
    {
        private readonly IApiSaeService _apiSae;

        public ClientController(IApiSaeService apiSae)
        {
            _apiSae = apiSae;
        }

        public async Task<ActionResult> Agendar(int NroIntServico)
        {
            var municipios = await _apiSae.ConsultaMunicipiosPorServico(NroIntServico);
            ViewBag.municipio = new SelectList(municipios, "nroIntMunicipio", "nome", 90000);

            return View();
        }
    }
}