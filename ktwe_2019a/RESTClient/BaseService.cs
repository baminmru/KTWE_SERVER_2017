using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;


namespace RESTClient
{
    class BaseService
    {
      
        protected string _baseAddress = "http://localhost/api";

       public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime NextRefreshTime { get; set; }

        public static string METHOD_POST = "POST";
        public static string METHOD_GET = "GET";

        public BaseService(string baseURL)
        {
            _baseAddress = baseURL;
        }

        public async Task<R> MakeAuthorizedRequest<T, R>(string url, string method, T request)
        {
            try
            {
                return await MakeRequest<T, R>(url, method, request);
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // get

                    }
                    throw;
                    //Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    //using (Stream data = response.GetResponseStream())
                    //using (var reader = new StreamReader(data))
                    //{
                    //    string text = reader.ReadToEnd();
                    //}
                }
            }
        }
        public async Task<R> MakeRequest<T, R>(string url, string method, T request)
        {
            try
            {
               
                var link = $"{_baseAddress}/{url}";
                HttpWebRequest webRequest = HttpWebRequest.CreateHttp(link);
                webRequest.Method = method;
                webRequest.ContentType = "application/vnd.api+json"; //"application/json";
                
                if (AccessToken != "")
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Headers.Add("Authorization", $"Bearer {AccessToken}");
                }
                webRequest.Accept = "*/*";

                if (method == BaseService.METHOD_POST)
                {
                    var data = JsonConvert.SerializeObject(request);
                    System.Diagnostics.Debug.Print(data);
                    webRequest.ContentLength = data.Length;

                    using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        writer.Write(data);
                    }
                }
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var rdr = new StreamReader(stream))
                        {
                            var result = rdr.ReadToEnd();
                            var registerResult = JsonConvert.DeserializeObject<R>(result);
                            return registerResult;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                //_messenger.Send(new Messages.MessageServerRequest(-1));
            }
        }


        public async Task<string> MakeRequest<T>(string url, string method, T request)
        {
            try
            {

                var link = $"{_baseAddress}/{url}";
                HttpWebRequest webRequest = HttpWebRequest.CreateHttp(link);
                webRequest.Method = method;
                webRequest.ContentType = "application/vnd.api+json"; //"application/json";

                if (AccessToken != "")
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Headers.Add("Authorization", $"Bearer {AccessToken}");
                }
                webRequest.Accept = "*/*";

                if (method == BaseService.METHOD_POST)
                {
                    var data = JsonConvert.SerializeObject(request);
                    System.Diagnostics.Debug.Print(data);
                    webRequest.ContentLength = data.Length;

                    using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        writer.Write(data);
                    }
                }
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var rdr = new StreamReader(stream))
                        {
                            var result = rdr.ReadToEnd();
                            return result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                //_messenger.Send(new Messages.MessageServerRequest(-1));
            }
        }

        public async Task<string> MakeRequest(string url, string method, string request)
        {
            try
            {

                var link = $"{_baseAddress}/{url}" +request;
                HttpWebRequest webRequest = HttpWebRequest.CreateHttp(link);
                webRequest.Method = method;
                webRequest.ContentType = "application/vnd.api+json"; //"application/json";

                if (AccessToken != "")
                {
                    webRequest.PreAuthenticate = true;
                    webRequest.Headers.Add("Authorization", $"Bearer {AccessToken}");
                }
                webRequest.Accept = "*/*";

                if (method == BaseService.METHOD_POST)
                {
                    //var data = request;
                    //System.Diagnostics.Debug.Print(data);
                    //webRequest.ContentLength = data.Length;

                    //using (var writer = new StreamWriter(webRequest.GetRequestStream()))
                    //{
                    //    writer.Write(data);
                    //}
                }
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var rdr = new StreamReader(stream))
                        {
                            var result = rdr.ReadToEnd();
                            return result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                //_messenger.Send(new Messages.MessageServerRequest(-1));
            }
        }

    }
}
