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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace EtheriT.Coker.Application.Authorization
{
    public class AccountAppService : IAccountAppService
    {
        private readonly CokerDbContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly ITokenAppService tokenAppService;
        private readonly LoginUserData loginUserData;
        private readonly IHttpContextAccessor httpContextAccessor;
        public AccountAppService(
            CokerDbContext db,
            IPasswordHasher passwordHasher,
            ITokenAppService tokenAppService,
            LoginUserData loginUserData,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.tokenAppService = tokenAppService;
            this.loginUserData = loginUserData;
            this.httpContextAccessor = httpContextAccessor;
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
                        long bindID = 0;
                        var defaultWeb = await db.MappingUserAndWebsites.Where(m => m.UserId == user.Id).FirstOrDefaultAsync(); 
                        if(defaultWeb!= null) bindID = defaultWeb.UserId;
                        Core.Models.Token t = new Core.Models.Token
                        {
                            ip = loginUserData.GetClientIP()??"",
                            UserID = user.Id,
                            StartTime = dateTime,
                            EndTime = EndDateTime,
                            websiteId = bindID
                        };
                        db.Tokens.Add(t);
                        db.SaveChanges();
                        output.Success = true;
                        output.Token = await tokenAppService.CreateToken(user.Account, t.id);
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
        [Authorize]
        public async Task<UserDto> GetCurrentUser() {
            ClaimsPrincipal user = httpContextAccessor.HttpContext?.User;
            string name = user.Identity?.Name;
            UserDto output = new UserDto();
            try
            {
                var theUser = await db.Users
                    .Where(e => e.Account == name).Where(e => !e.IsDeleted).FirstOrDefaultAsync();
                if (theUser != null)
                {
                    var o = from w in db.Websites
                            join m in db.MappingUserAndWebsites on w.Id equals m.WebsiteId
                            where m.UserId == theUser.Id
                            select new Webs.Dto.WebsDto
                            {
                                Id = w.Id,
                                Name = w.Title
                            };
                    output.Account = theUser.Account;
                    output.UserName = theUser.Name;
                    output.Webs = await o.ToListAsync();
                }
            }
            catch {
                output.Account = "";
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
                            response.Token = await tokenAppService.CreateToken(users.Account,t.id);
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
            if (!string.IsNullOrEmpty(secret)) {
                var t = await db.Tokens.Where(e => e.id == Guid.Parse(secret ?? "")).FirstOrDefaultAsync();
                if (t != null)
                {
                    db.Tokens.Remove(t);
                    db.SaveChanges();
                }
            }
            return new ResponseMessageDto
            {
                Success = await tokenAppService.DelToken()
            };
        }
    }
}