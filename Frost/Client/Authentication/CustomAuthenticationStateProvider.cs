using Blazored.LocalStorage;
using Frost.Client.Extensions;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Frost.Client.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        public CustomAuthenticationStateProvider(ILocalStorageService service)
        {
            _localStorageService = service;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var session = await _localStorageService.ReadEncryptedItemAsync<LoginResult>("UserSession");
                if(session == null)
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                if (DateTime.Compare(session.jwtExpDate, DateTime.UtcNow) < 0)
                {
                    await _localStorageService.RemoveItemAsync("UserSession");
                    var newSession = await RefreshJwtAsync(session.jwt, session.jwtRefresh);
                    if (newSession != null)
                    {
                        await _localStorageService.SaveItemAsEncryptedAsync("UserSession", newSession);
                        var newClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                        {
                            new Claim (ClaimTypes.NameIdentifier, newSession.Id.ToString()),
                            new Claim (ClaimTypes.Name, newSession.name),
                            new Claim (ClaimTypes.Email, newSession.email)
                        }, "JwtAuth"));
                        return await Task.FromResult(new AuthenticationState(newClaimsPrincipal));
                    }
                    
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim (ClaimTypes.NameIdentifier, session.Id.ToString()),
                    new Claim (ClaimTypes.Name, session.name),
                    new Claim (ClaimTypes.Email, session.email)
                },"JwtAuth"));
                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }
        public async Task UpdateAuthenticationState(LoginResult? session)
        {
            ClaimsPrincipal claimsPrincipal;
            if(session == null)
            {
                claimsPrincipal = _anonymous;
                await _localStorageService.RemoveItemAsync("UserSession");
            }
            else
            {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim (ClaimTypes.NameIdentifier, session.Id.ToString()),
                    new Claim (ClaimTypes.Name, session.name),
                    new Claim (ClaimTypes.Email, session.email)
                    
                }));
                await _localStorageService.SaveItemAsEncryptedAsync("UserSession", session);
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
        public async Task<string> GetToken()
        {
            var result = string.Empty;
            try
            {

                var session = await _localStorageService.ReadEncryptedItemAsync<LoginResult>("UserSession");
                result = session.jwt;
                return result;

            }catch {
                return result;
            }
        }
        public async Task<string> GetRefreshToken()
        {
            var result = string.Empty;
            try
            {
                var session = await _localStorageService.ReadEncryptedItemAsync<LoginResult>("UserSession");
                result = session.jwtRefresh;
            }
            catch
            {
                return result;
            }
            
            return result;
        }
        private async Task<LoginResult?> RefreshJwtAsync(string jwtToken, string refreshToken)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                JwtDTO jwtDTO = new JwtDTO(jwtToken, refreshToken);

                var response = await httpClient.PostAsJsonAsync<JwtDTO>("https://localhost:44350/api/auth/renewToken", jwtDTO);
                if (response.IsSuccessStatusCode)
                {
                    LoginResult result = await response.Content.ReadFromJsonAsync<LoginResult>();
                    return result;
                    
                }
                else
                {
                    return null;
                   
                }
            }
        }
    }
}
