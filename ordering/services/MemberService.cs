
using Newtonsoft.Json;
using ordering.aop;
using ordering.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ordering.services
{
    public class MemberService : IMemberService
    {
     


        [PollyHandle(IsCircuitBreaker = true, FallbackMethod = "GetMemberInfoFallback", ExceptionsAllowedBeforeBreaking = 5, SecondsOfBreak = 30, RetryTimes = 3)]
        public async Task<MemberVM> GetMemberInfo(string id)
        {
        

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress =
                    new Uri($"http://member_center");
                var result = await httpClient.GetAsync("/member/" + id);
                result.EnsureSuccessStatusCode();
                var json = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<MemberVM>(json);
                }
            }

            return null;
        }

        public MemberVM GetMemberInfoFallback(string id)
        {
            return null;
        }
    }
}
