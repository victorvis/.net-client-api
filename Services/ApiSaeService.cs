using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Configuration;
using System.Net;
using SaeClient.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using SaeClient.Models;
using System.Runtime.Caching;

namespace SaeClient.Services
{
    /// <summary>
    /// Classe de exemplo de integração chamando uma API ArqNet3.
    /// Utiliza um usuário de sistema para fazer a autenticação.
    /// </summary>
    public class ApiSaeService: IApiSaeService
    {
        private const string _cacheKey = "ApiUsuarioLogon";
        private readonly HttpClient _httpClient;
        private readonly string _urlLogon;
        private readonly string _username;
        private readonly string _password;
        private readonly string _urlApi;

        public ApiSaeService()
        {
            _httpClient = new HttpClient();
            _urlApi = ConfigurationManager.AppSettings["apiIntegracao.urlApi"];
            _urlLogon = ConfigurationManager.AppSettings["apiIntegracao.urlLogon"];
            _username = ConfigurationManager.AppSettings["apiIntegracao.username"];
            _password = ConfigurationManager.AppSettings["apiIntegracao.password"];

            if (string.IsNullOrEmpty(_urlApi))
                throw new ConfigurationErrorsException("A chave 'apiIntegracao.urlApi' não foi definida na sessão 'appSettings' do arquivo de configuração.");
            if (string.IsNullOrEmpty(_urlLogon))
                throw new ConfigurationErrorsException("A chave 'apiIntegracao.urlLogon' não foi definida na sessão 'appSettings' do arquivo de configuração.");
            if (string.IsNullOrEmpty(_username))
                throw new ConfigurationErrorsException("A chave 'apiIntegracao.username' não foi definida na sessão 'appSettings' do arquivo de configuração.");
            if (string.IsNullOrEmpty(_password))
                throw new ConfigurationErrorsException("A chave 'apiIntegracao.password' não foi definida na sessão 'appSettings' do arquivo de configuração.");
        }

        public async Task<IEnumerable<MunicipioDto>> ConsultaMunicipiosPorServico(int NroIntServico)
        {
            var token = await ObtemAccessTokenCache();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
            var url = $"{_urlApi}/api/municipio/lista-por-servico?NroIntServico={NroIntServico}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new WebException($"Erro '{(int)response.StatusCode}' ao acessar a API na URL '{url}'. Mensagem: '{response.Content.ReadAsStringAsync().Result}'");

            return await response.Content.ReadAsAsync<IEnumerable<MunicipioDto>>();
        }

        public async Task<IEnumerable<string>> ConsultaDiasDisponiveis(int NroIntLocalServico)
        {
            var token = await ObtemAccessTokenCache();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
            var url = $"{_urlApi}/api/agendamento/dias-disponiveis?NroIntLocalServico={NroIntLocalServico}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new WebException($"Erro '{(int)response.StatusCode}' ao acessar a API na URL '{url}'. Mensagem: '{response.Content.ReadAsStringAsync().Result}'");

            return await response.Content.ReadAsAsync<IEnumerable<string>>();
        }

        public IEnumerable<string> ConsultaDiasDisponiveisSync(int NroIntLocalServico)
        {
            var token = ObtemAccessTokenSync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
            var url = $"{_urlApi}/api/agendamento/dias-disponiveis?NroIntLocalServico={NroIntLocalServico}";
            var response = _httpClient.GetAsync(url);

            if (!response.Result.IsSuccessStatusCode)
                throw new WebException($"Erro '{(int)response.Result.StatusCode}' ao acessar a API na URL '{url}'. Mensagem: '{response.Result.Content.ReadAsStringAsync().Result}'");

            return response.Result.Content.ReadAsAsync<IEnumerable<string>>().Result;
        }

        /// <summary>
        /// Mantem o access em um MemoryCache para evitar repetir o logon a cada chamada da API.
        /// O tempo do cache configurado de acordo com tempo de expiração do token.
        /// </summary>
        /// <returns>Retorna um access token</returns>
        private async Task<TokenDto> ObtemAccessTokenCache()
        {
            TokenDto token;

            var cache = MemoryCache.Default;
            // Obtem do cache OU chama a API
            if (!cache.Contains(_cacheKey))
            {
                token = await ObtemAccessToken();
                cache.Add(_cacheKey, token, DateTimeOffset.Now.AddSeconds(token.ExpiresIn));
            }
            return (TokenDto)cache.Get(_cacheKey);
        }


        /// <summary>
        /// Acessa a API de Logon da Apm3 passando um usuário de sistema e obtem o access token
        /// </summary>
        /// <returns>Retorna um access token</returns>
        private async Task<TokenDto> ObtemAccessToken()
        {
            var nameValueCollection = new List<KeyValuePair<string, string>> {
                { new KeyValuePair<string, string>("grant_type", "password") },
                { new KeyValuePair<string, string>("username", _username) },
                { new KeyValuePair<string, string>("password", _password) }
            };

            using (HttpContent content = new FormUrlEncodedContent(nameValueCollection))
            {
                var response = await _httpClient.PostAsync(_urlLogon, content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    throw new WebException($"Erro '{(int)response.StatusCode}' ao acessar o serviço de autentição do SoeAuth na Url '{_urlLogon}'. Mensagem: '{response.Content.ReadAsStringAsync().Result}'");

                return await response.Content.ReadAsAsync<TokenDto>();
            }

        }

        private TokenDto ObtemAccessTokenSync()
        {
            var nameValueCollection = new List<KeyValuePair<string, string>> {
                { new KeyValuePair<string, string>("grant_type", "password") },
                { new KeyValuePair<string, string>("username", _username) },
                { new KeyValuePair<string, string>("password", _password) }
            };

            using (HttpContent content = new FormUrlEncodedContent(nameValueCollection))
            {
                var response = _httpClient.PostAsync(_urlLogon, content);

                if (!response.Result.IsSuccessStatusCode)
                    throw new WebException($"Erro '{(int)response.Result.StatusCode}' ao acessar o serviço de autentição do SoeAuth na Url '{_urlLogon}'. Mensagem: '{response.Result.Content.ReadAsStringAsync().Result}'");

                return response.Result.Content.ReadAsAsync<TokenDto>().Result;
            }

        }
    }
}
