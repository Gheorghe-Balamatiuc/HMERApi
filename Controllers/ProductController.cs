using AutoMapper;
using HMERApi.Models;
using HMERApi.Models.DTO;
using HMERApi.Repository.IRepository;
using HMERApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HMERApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    ILogger<ProductController> logger,
    IMapper mapper
    ) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(3 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDTO productToAdd)
    {
        try
        {
            if (productToAdd.ImageFile == null)
            {
                return BadRequest("Image file is required.");
            }

            if (productToAdd.ImageFile.Length > 3 * 1024 * 1024)
            {
                return BadRequest("Image size should not exceed 3MB.");
            }

            string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];
            string createdImageName = await fileService.SaveFileAsync(productToAdd.ImageFile, allowedExtensions);

            var product = new Product
            {
                Image = createdImageName
            };
            await unitOfWork.ProductRepository.CreateAsync(product);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDTO productToUpdate)
    {
        try
        {
            var existingProduct = await unitOfWork.ProductRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            string oldImage = existingProduct.Image;
            if (productToUpdate.ImageFile != null)
            {
                if (productToUpdate.ImageFile.Length > 3 * 1024 * 1024)
                {
                    return BadRequest("Image size should not exceed 3MB.");
                }
                string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];
                string createdImageName = await fileService.SaveFileAsync(productToUpdate.ImageFile, allowedExtensions);

                existingProduct.Image = createdImageName;

                fileService.DeleteFile(oldImage);
            }

            await unitOfWork.ProductRepository.UpdateAsync(existingProduct);

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var existingProduct = await unitOfWork.ProductRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            await unitOfWork.ProductRepository.DeleteByIdAsync(existingProduct.Id);
            fileService.DeleteFile(existingProduct.Image);

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await unitOfWork.ProductRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound("Product not found.");
        }
        return Ok(mapper.Map<ProductNoIdDTO>(product));
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await unitOfWork.ProductRepository.GetAllAsync();
        return Ok(products);
    }
}