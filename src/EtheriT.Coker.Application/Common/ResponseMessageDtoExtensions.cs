using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Common
{
    public static class ResponseMessageDtoExtensions
    {
        public static IActionResult ToActionResult(this ResponseMessageDto result, ControllerBase controller)
        {
            if (result == null)
                return controller.StatusCode(500);

            if (result.Success)
                return controller.Ok(result);

            return result.ErrorCode switch
            {
                ErrorCodeEnum.NotFound => controller.NotFound(result),
                ErrorCodeEnum.Unauthorized => controller.Unauthorized(result),
                ErrorCodeEnum.Forbidden => controller.StatusCode(403, result),
                ErrorCodeEnum.Conflict => controller.Conflict(result),
                ErrorCodeEnum.ValidationError => controller.BadRequest(result),
                ErrorCodeEnum.BadRequest => controller.BadRequest(result),
                ErrorCodeEnum.ServerError => controller.StatusCode(500, result),
                _ => controller.BadRequest(result)
            };
        }
    }
}
