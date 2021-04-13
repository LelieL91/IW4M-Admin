﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibraryCore.Dtos;
using SharedLibraryCore.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Services;
using WebfrontCore.Controllers.API.Dtos;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WebfrontCore.Controllers.API
{
    /// <summary>
    /// api controller for client operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : BaseController
    {
        private readonly IResourceQueryHelper<FindClientRequest, FindClientResult> _clientQueryHelper;
        private readonly ILogger _logger;
        private readonly ClientService _clientService;

        public ClientController(ILogger<ClientController> logger,
            IResourceQueryHelper<FindClientRequest, FindClientResult> clientQueryHelper,
            ClientService clientService, IManager manager) : base(manager)
        {
            _logger = logger;
            _clientQueryHelper = clientQueryHelper;
            _clientService = clientService;
        }

        [HttpGet("find")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAsync([FromQuery] FindClientRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse()
                {
                    Messages = ModelState.Values
                        .SelectMany(_value => _value.Errors.Select(_error => _error.ErrorMessage)).ToArray()
                });
            }

            try
            {
                var results = await _clientQueryHelper.QueryResource(request);

                return Ok(new FindClientResponse
                {
                    TotalFoundClients = results.TotalResultCount,
                    Clients = results.Results
                });
            }

            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to retrieve clients with query - {@request}", request);

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse()
                {
                    Messages = new[] {e.Message}
                });
            }
        }


        [HttpPost("{clientId:int}/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginAsync([FromRoute] int clientId,
            [FromBody, Required] PasswordRequest request)
        {
            if (clientId == 0)
            {
                return Unauthorized();
            }

            if (Authorized)
            {
                return Ok();
            }

            try
            {
                var privilegedClient = await _clientService.GetClientForLogin(clientId);
                var loginSuccess = false;

                if (!Authorized)
                {
                    loginSuccess =
                        Manager.TokenAuthenticator.AuthorizeToken(privilegedClient.NetworkId, request.Password) ||
                        (await Task.FromResult(SharedLibraryCore.Helpers.Hashing.Hash(request.Password,
                            privilegedClient.PasswordSalt)))[0] == privilegedClient.Password;
                }

                if (loginSuccess)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, privilegedClient.Name),
                        new Claim(ClaimTypes.Role, privilegedClient.Level.ToString()),
                        new Claim(ClaimTypes.Sid, privilegedClient.ClientId.ToString()),
                        new Claim(ClaimTypes.PrimarySid, privilegedClient.NetworkId.ToString("X"))
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);
                    await SignInAsync(claimsPrinciple);

                    return Ok();
                }
            }

            catch (Exception)
            {
                return Unauthorized();
            }

            return Unauthorized();
        }

        [HttpPost("{clientId:int}/logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();
        }

        public class PasswordRequest
        {
            public string Password { get; set; }
        }
    }
}