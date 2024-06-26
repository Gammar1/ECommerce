﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        /// <summary>
        /// Just to check if the API server is up.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public string Index()
        {
            return "API server is up and running";
        }
    }
}
