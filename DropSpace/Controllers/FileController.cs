using DropSpace.Contracts.Dtos;
using DropSpace.Extensions;
using DropSpace.Models;
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
        [HttpDelete]
        public async Task<ActionResult> Delete(DeleteFileModel deleteFileModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await fileService.Delete(deleteFileModel.FileId, deleteFileModel.SessionId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            return Ok();
        }


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
            }
            catch (Exception ex)
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
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("Download")]
        public async Task<ActionResult> DownloadFileChunk(DownloadChunkModel downloadChunkModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var data = await fileService.GetChunkData(downloadChunkModel);

            return File(data.Content, data.ContentType);
        }

        [HttpGet]
        public async Task<ActionResult> GetFileInfo(Guid fileId)
        {
            try
            {
                var file = await fileService.GetFile(fileId);

                return Ok(new FileModelDto(file.Id, file.ByteSize, file.ByteSize.ToMBytes(), file.FileName));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
