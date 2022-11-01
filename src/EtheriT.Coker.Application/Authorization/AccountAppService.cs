using EtheriT.Coker.Application.Authorizaion.Dto;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.Authorization
{
    public class AccountAppService: IAccountAppService
    {
        private readonly CokerDbContext db;
        public AccountAppService(CokerDbContext db) {
            this.db = db; 
        }
        public async Task<LoginOutputDto> Login(LoginInputDto dto) {
            LoginOutputDto output = new LoginOutputDto() { Success = false };
            try
            {
                var user = db.Users.Where(e => e.Account == dto.UserName || e.CellPhone == dto.UserName || e.Email == dto.UserName);
                   //.Where(e => e.Password == dto.Password);
                if (user.Any()) output.Success = true;
                else throw new Exception("帳號或密碼錯誤");
            }
            catch (Exception e)
            {
                output.Success = false;
                output.Error = e.Message;
            }
           
            return output;
        }
    }
}