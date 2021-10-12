
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace member_center.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConsulController : ControllerBase
    {
      
        IConfiguration _configuration;
        public ConsulController(IConfiguration configuration)
        {
        
            _configuration = configuration;
        }

      

        [HttpGet("getConfig")]
        public string GetConfig(string key)
        {
            return _configuration[key];
        }
    }
}
