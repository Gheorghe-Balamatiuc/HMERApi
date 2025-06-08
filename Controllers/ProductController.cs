using System.Diagnostics;
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

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "Services/HMER/inference.py --config Services/HMER/14.yaml --image_path uploads/" + createdImageName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                logger.LogError("Failed to start Python process");
                return StatusCode(500, "Failed to process image");
            }

            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                logger.LogError("Python process failed with exit code {ExitCode}: {Stderr}", process.ExitCode, stderr);
                return StatusCode(500, "Failed to process image");
            }

            logger.LogInformation("Python process completed successfully: {Stdout}", stdout);

            var product = new Product
            {
                Image = createdImageName,
                ImagePrediction = stdout.Replace(" ", "").Trim()
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

                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "Services/HMER/inference.py --config Services/HMER/14.yaml --image_path uploads/" + createdImageName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                {
                    logger.LogError("Failed to start Python process");
                    return StatusCode(500, "Failed to process image");
                }

                string stdout = await process.StandardOutput.ReadToEndAsync();
                string stderr = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    logger.LogError("Python process failed with exit code {ExitCode}: {Stderr}", process.ExitCode, stderr);
                    return StatusCode(500, "Failed to process image");
                }

                logger.LogInformation("Python process completed successfully: {Stdout}", stdout);

                existingProduct.Image = createdImageName;
                existingProduct.ImagePrediction = stdout.Replace(" ", "").Trim();

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