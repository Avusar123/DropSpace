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
        IAuthorizationService authorizationService) : ControllerBase
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

        [HttpGet("All/{sessionId}")]
        public async Task<ActionResult<List<FileModelDto>>> GetAll(Guid sessionId)
        {
            var authresult = await authorizationService
                .AuthorizeAsync(User, sessionId, new MemberRequirement());

            if (!authresult.Succeeded)
            {
                return Forbid();
            }


            try
            {
                return await fileService.GetAllFiles(sessionId);
            }
            catch (Exception er)
            {
                ModelState.AddModelError(string.Empty, er.Message);

                return BadRequest(ModelState);
            }
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
                return Ok(await fileService.CreateUpload(initiateUploadModel));
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
                return Ok(await fileService.UploadNewChunk(uploadChunk));
            }
            catch (NullReferenceException ex)
            {
                return NotFound(ex.InnerException);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("Download")]
        public async Task<ActionResult> DownloadFileChunk(DownloadChunkModel downloadChunkModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var data = await fileService.GetChunkData(downloadChunkModel);

                return File(data.Content, data.ContentType);
            }
            catch (NullReferenceException)
            {
                return NotFound(downloadChunkModel.FileId);
            }
            catch (Exception er)
            {
                ModelState.AddModelError(string.Empty, er.Message);

                return BadRequest(ModelState);
            }

        }

        [HttpGet("{fileId}")]
        public async Task<ActionResult> GetFileInfo(Guid fileId)
        {
            try
            {
                var file = await fileService.GetFile(fileId);

                return Ok(file.ToDto());
            }
            catch (NullReferenceException)
            {
                return NotFound(fileId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
