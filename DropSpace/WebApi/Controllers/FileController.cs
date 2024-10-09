using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using DropSpace.Logic.Extensions;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.WebApi.Controllers.Filters;
using DropSpace.WebApi.Utils.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DropSpace.WebApi.Controllers
{
    [Route("File")]
    [ApiController]
    [Authorize]
    public class FileController(
        IFileService fileService) : ControllerBase
    {
        [HttpDelete]
        [SessionMemberFilter(nameof(deleteFileModel), "SessionId")]
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
        [SessionMemberFilter(nameof(sessionId))]
        public async Task<ActionResult<List<FileModelDto>>> GetAll(Guid sessionId)
        {
            try
            {
                return await fileService.GetAllFiles(sessionId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [SessionMemberFilter(nameof(initiateUploadModel), "SessionId")]
        public async Task<ActionResult<FileModelDto>> CreateUpload(InitiateUploadModel initiateUploadModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
        public async Task<ActionResult<FileModelDto>> UploadChunk(UploadChunkModel uploadChunk)
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

        //[HttpGet("Download")]
        //[SessionMemberFilter(nameof(downloadChunkModel), "SessionId")]
        //public async Task<ActionResult> DownloadFileChunk(DownloadChunkModel downloadChunkModel)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        var data = await fileService.GetChunkData(downloadChunkModel);

        //        return File(data.Content, data.ContentType);
        //    }
        //    catch (NullReferenceException)
        //    {
        //        return NotFound(downloadChunkModel.FileId);
        //    }
        //    catch (Exception er)
        //    {
        //        ModelState.AddModelError(string.Empty, er.Message);

        //        return BadRequest(ModelState);
        //    }

        //}

        [HttpGet("{fileId}")]
        public async Task<ActionResult<FileModelDto>> GetFileInfo(Guid fileId)
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
