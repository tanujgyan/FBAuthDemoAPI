using FBAuthDemoAPI.Models;
using FBAuthDemoAPI.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBAuthDemoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFamilyService _familyService;

        public FamilyController(IFamilyService familyService, ILogger<FamilyController> logger)
        {
            _familyService = familyService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Family>>> GetFamilyData()
        {
            try
            {
                var family = await _familyService.GetFamilyDataAsync("SELECT * FROM c");
                return Ok(family);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while retreiving data");
            }
        }
        [HttpPost]
        public async Task<ActionResult> AddFamilyData(Family family)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _familyService.AddFamilyDataAsync(family);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while inserting data");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteFamilyData(string id)
        {
            try
            {
                await _familyService.DeleteFamilyDataAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while deleting data");
            }
        }
        [HttpPut]
        public async Task<ActionResult> UpdateFamilyData(Family family)
        {
            try
            {
                await _familyService.UpdateFamilyDataAsync(family);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while updating data");
            }
        }
    }
}
