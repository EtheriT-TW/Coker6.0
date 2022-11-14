using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.Application.Token;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Net;
using EtheriT.Coker.Application.Dto;
using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using EtheriT.Coker.Core.Models;
using Microsoft.Extensions.Primitives;

namespace EtheriT.Coker.Application.Authorization
{
    public class AccountAppService : IAccountAppService
    {
        private readonly CokerDbContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDistributedCache cache;
        public AccountAppService(
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache
        )
        {
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
        }
        public async Task<LoginOutputDto> Login(LoginInputDto dto)
        {
            LoginOutputDto output = new LoginOutputDto() { Success = false };
            try
            {
                if (string.IsNullOrEmpty(dto.UserName)) throw new Exception("使用者名稱不可為空");
                else if (string.IsNullOrEmpty(dto.Password)) throw new Exception("密碼不可為空");
                var users = db.Users.Where(e => e.Account == dto.UserName || e.CellPhone == dto.UserName || e.Email == dto.UserName);
                if (users.Any())
                {
                    var user = await users.FirstOrDefaultAsync();
                    string password = user.Password;
                    if (passwordHasher.VerifyHashedPassword(password, dto.Password))
                    {
                        DateTime dateTime = DateTime.Now;
                        DateTime EndDateTime = dateTime.AddMinutes(30);
                        Core.Models.Token t = new Core.Models.Token
                        {
                            ip = httpContextAccessor.HttpContext.Connection?.RemoteIpAddress?.ToString(),
                            UserID = user.Id,
                            StartTime = dateTime,
                            EndTime = EndDateTime,
                        };
                        db.Tokens.Add(t);
                        db.SaveChanges();
                        output.Success = true;
                        output.Token = tokenAppService.CreateToken(user.Account);
                        output.Secret = t.id;
                        output.EndDateTime = EndDateTime;
                    }
                    else throw new Exception("帳號或密碼錯誤");
                }
                else throw new Exception("帳號或密碼錯誤");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }

            return output;
        }
        public async Task<LoginOutputDto> Chech() {
            LoginOutputDto response = new LoginOutputDto
            {
                Success=false
            };
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            string? secret = httpContextAccessor.HttpContext?.Request.Headers["secret"].ToString();
            try {
                if (!string.IsNullOrEmpty(name))
                {
                    if (string.IsNullOrEmpty(secret))
                    {
                        response.Success = true;
                    }
                    else {
                        var t = await db.Tokens.Where(e => e.id == Guid.Parse(secret ?? "")).FirstOrDefaultAsync();
                        if (t != null)
                        {
                            var users = await db.Users.Where(e => e.Id == t.UserID).FirstOrDefaultAsync();
                            if (t.EndTime < DateTime.Now.AddMinutes(5))
                            {
                                t.EndTime = DateTime.Now.AddMinutes(30);
                                db.SaveChanges();
                            }
                            response.Success = true;
                            response.Token = tokenAppService.CreateToken(users.Account);
                            response.Secret = t.id;
                            response.EndDateTime = t.EndTime.Value;
                        }
                        else throw new Exception("登入已過期");
                    }
                }
                else throw new Exception("登入已過期");
            } catch (Exception e) {
                response.Success = false;
                response.Error = e.Message;
            }
            var removeToken = db.Tokens.Where(e => e.EndTime < DateTime.Now);
            db.Tokens.RemoveRange(removeToken);
            db.SaveChanges();
            return response;
        }
        public async Task<ResponseMessageDto> Logout() {
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            string? secret = httpContextAccessor.HttpContext?.Request.Headers["secret"].ToString();
            string token = GetCurrentAsync();
            if (!string.IsNullOrEmpty(secret)) {
                var t = await db.Tokens.Where(e => e.id == Guid.Parse(secret ?? "")).FirstOrDefaultAsync();
                if (t != null)
                {
                    db.Tokens.Remove(t);
                    db.SaveChanges();
                }
            }
            await cache.SetStringAsync(
                GetKey(token),
                " ", 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(0.001)
                }
            );
            ResponseMessageDto response = new ResponseMessageDto
            {
                Success = true
            };
            return response;
        }
        private string GetCurrentAsync()
        {
            var authorizationHeader = httpContextAccessor
                .HttpContext.Request.Headers["authorization"];

            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(" ").Last();
        }
        private static string GetKey(string token)
        {
            return $"tokens:{token}:deactivated";
        }
    }
}