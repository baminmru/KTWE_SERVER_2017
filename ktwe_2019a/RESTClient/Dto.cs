using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTClient
{
    public class TokenResp
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public  int expire_in { get; set; }

        public string refresh_token { get; set; }

        public Int64 refresh_expire_in { get; set; }
        public string session_state { get; set; }

        public string scope { get; set; }
    }

    public class userDto
    {
        public string grant_type { get; set; } = "password";
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string scope { get; set; } = "read";
    }

    public class refreshDto
    {
        public string grant_type { get; set; } = "password";
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
    }

    /*
    public class ErrorDto {
        public string code { get; set; }
        public string message { get; set; }
        public string [] Params ;
    }

    public class EmptyRequest
    {
    }

    public class PaginatedDto
    {
        public int page { get; set; }
        public int nbPages { get; set; }
        public int pageSize { get; set; }
        public int count { get; set; }
        public int totalCount { get; set; }
        public List<RxMessageDto> list { get; set; }
    }

    public class RxMessageDto
    {
        public string msgId { get; set; }
        public int port { get; set; }
        public Int64 timestamp { get; set; }
        public string devAddr { get; set; }
        public int fcntUp { get; set; }
        public int fcntDown { get; set; }
        public float frequency { get; set; }
        public string modulation { get; set; }
        public int bandwidth { get; set; }
        public string adr { get; set; }
        public string codingRate { get; set; }
        public string devEui { get; set; }
        public string payload { get; set; }
        public bool encripted { get; set; }
        public string keySessionId { get; set; }
    }

    public class TxMessageDto
    {
        public string msgId { get; set; }
        public Int64 timestamp { get; set; }
        public string devEui { get; set; }
        public int port { get; set; }
        public string payload { get; set; }
        public string contentType { get; set; }
        public string txEvent { get; set; }
        public string txStatus { get; set; }
        public bool ack { get; set; }
        public int nbRetry { get; set; }
        public int maxRetry { get; set; }
        public int timeToLive { get; set; }
        public string keySessionId { get; set; }
        public int fcntDown { get; set; }
        public List<TxMessageHistoricDto> historic { get; set; }
        
    }
    public class TxMessageHistoricDto
    {
        public Int64 timestamp { get; set; }
        public string txStatus { get; set; }
    }

    public class TxMessageDtoShort
    {
        
        public int port { get; set; }
        public string payload { get; set; }
        public string contentType { get; set; }
        public bool ack { get; set; }
        public int maxRetry { get; set; }
        public int timeToLive { get; set; }
    }

    */

}
