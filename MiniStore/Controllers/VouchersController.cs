using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStore.DTOs.Voucher;
using MiniStore.Models;
using MiniStore.Services.Interfaces;

namespace MiniStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _voucherService.GetAllAsync();
            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var v = await _voucherService.GetByIdAsync(id);
            if (v == null) return NotFound();
            return Ok(v);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherDto voucherDto)
        {
            var voucher = new Voucher
            {
                Code = voucherDto.Code,
                Description = voucherDto.Description,
                DiscountType = voucherDto.DiscountType,
                DiscountValue = voucherDto.DiscountValue,
                MinimumOrderAmount = voucherDto.MinimumOrderAmount,
                MaximumDiscountAmount = voucherDto.MaximumDiscountAmount,
                StartDate = voucherDto.StartDate,
                EndDate = voucherDto.EndDate,
                Quantity = voucherDto.Quantity,
                UsedCount = voucherDto.UsedCount,
                IsActive = voucherDto.IsActive,
                ApplicableType = voucherDto.ApplicableType,
                ApplicableCategoryId = voucherDto.ApplicableCategoryId,
                ApplicableProductIds = voucherDto.ApplicableProductIds == null ? null : System.Text.Json.JsonSerializer.Serialize(voucherDto.ApplicableProductIds)
            };
            var created = await _voucherService.CreateAsync(voucher);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VoucherDto voucherDto)
        {
            if (id != voucherDto.Id) return BadRequest();
            var voucher = new Voucher
            {
                Id = voucherDto.Id,
                Code = voucherDto.Code,
                Description = voucherDto.Description,
                DiscountType = voucherDto.DiscountType,
                DiscountValue = voucherDto.DiscountValue,
                MinimumOrderAmount = voucherDto.MinimumOrderAmount,
                MaximumDiscountAmount = voucherDto.MaximumDiscountAmount,
                StartDate = voucherDto.StartDate,
                EndDate = voucherDto.EndDate,
                Quantity = voucherDto.Quantity,
                UsedCount = voucherDto.UsedCount,
                IsActive = voucherDto.IsActive,
                ApplicableType = voucherDto.ApplicableType,
                ApplicableCategoryId = voucherDto.ApplicableCategoryId,
                ApplicableProductIds = voucherDto.ApplicableProductIds == null ? null : System.Text.Json.JsonSerializer.Serialize(voucherDto.ApplicableProductIds)
            };
            if (id != voucher.Id) return BadRequest();
            var ok = await _voucherService.UpdateAsync(voucher);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _voucherService.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            var ok = await _voucherService.ToggleAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var list = await _voucherService.GetActiveAsync();
            return Ok(list);
        }

        [Authorize]
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyVoucherRequestDto request)
        {
            var result = await _voucherService.ApplyPreviewAsync(request.Code, request.OrderAmount, request.Items, request.UserId);
            return Ok(result);
        }
    }
}
