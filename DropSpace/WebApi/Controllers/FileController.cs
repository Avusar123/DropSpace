using DropSpace.Contracts.Dtos;
using DropSpace.Contracts.Models;
using DropSpace.Logic.Services.Interfaces;
using DropSpace.WebApi.Controllers.Filters.MemberFilter;
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
        [SessionMemberFilter(nameof(deleteFileModel))]
        public async Task<ActionResult> Delete(DeleteFileModel deleteFileModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await fileService.Delete(deleteFileModel.FileId);
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

        [HttpGet("{fileId}")]
        public async Task<ActionResult<FileModelDto>> GetFileInfo(Guid fileId)
        {
            try
            {
                var file = await fileService.GetFile(fileId);

                return Ok(file);
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
