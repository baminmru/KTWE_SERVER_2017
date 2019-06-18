using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTClient
{
    public class Client
    {

        BaseService baseService;
        public Client( string URL)
        {
            baseService = new BaseService(URL);
        }

        // return JSON STRING
        public string Process( string url,string method, string param)
        {

            if (baseService.AccessToken != "")
            {
                return baseService.MakeRequest(url, method, param).Result;
            }
            return @"{""error"":""yes""}";

        }

        public string Process<T>(string url, string method, T request)
        {

            if (baseService.AccessToken != "")
            {
                return baseService.MakeRequest<T>(url, method, request).Result;
            }
            return @"{""error"":""yes""}";

        }

        public async Task<TokenResp> Login(string Login, string Password,string Secret, string ID,string Scope)
        {
            try
            {
                var request = new userDto()
                {
                    username = Login,
                    password = Password,
                    client_id=ID,
                    client_secret=Secret,
                    scope=Scope
                };
                var registerResult = await baseService.MakeRequest<userDto, TokenResp>("oauth2/token", BaseService.METHOD_POST, request);
                baseService.AccessToken = registerResult.access_token;
                baseService.RefreshToken = registerResult.refresh_token;
                baseService.NextRefreshTime = DateTime.Now.AddSeconds(registerResult.expire_in);
                return registerResult;
            }
            catch (Exception ex)
            {
               
                return new TokenResp
                {
                    access_token = "",
                    token_type = "error: " + ex.Message,
                    expire_in = 0,
                    refresh_token = ex.Message,
                };
            }
        }


        public DateTime NextRefreshTime { get { return baseService.NextRefreshTime; } }
        public async Task<TokenResp> Refresh(string Login, string Password, string Secret, string ID, string Scope)
        {
            try
            {
                var request = new refreshDto()
                {
                    username = Login,
                    password = Password,
                    client_id = ID,
                    client_secret = Secret,
                    scope = Scope,
                    refresh_token = baseService.RefreshToken
                };
                var registerResult = await baseService.MakeRequest<refreshDto, TokenResp>("oauth2/token", BaseService.METHOD_POST, request);
                baseService.AccessToken = registerResult.access_token;
                baseService.RefreshToken = registerResult.refresh_token;
                baseService.NextRefreshTime = DateTime.Now.AddSeconds(registerResult.expire_in);
                return registerResult;
            }
            catch (Exception ex)
            {

                return new TokenResp
                {
                    access_token = "",
                    token_type = "error:" + ex.Message,
                    expire_in = 0,
                    refresh_token = ex.Message,
                };
            }
        }


    }
}

