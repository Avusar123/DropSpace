using DropSpace.Models;
using DropSpace.Models.Data;
using DropSpace.Requirements;
using DropSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DropSpace.Controllers
{
    [Route("File")]
    public class FileController(
        IFileService fileService,
        IAuthorizationService authorizationService) : Controller
    {
        [HttpPost]
        public async Task<ActionResult> CreateUpload(InitiateUploadModel initiateUploadModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authresult = await authorizationService
                .AuthorizeAsync(User, initiateUploadModel.SessionId, new MemberRequirement());

            if (!authresult.Succeeded)
            {
                return Forbid();
            }

            try
            {
                return Json(await fileService.CreateUpload(initiateUploadModel));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> UploadChunk(UploadChunkModel uploadChunk)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                return Json(await fileService.UploadNewChunk(uploadChunk));
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
    }
}
