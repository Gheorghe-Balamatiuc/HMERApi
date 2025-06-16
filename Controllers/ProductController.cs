using AutoMapper;
using HMERApi.Models;
using HMERApi.Models.DTO;
using HMERApi.Repository.IRepository;
using HMERApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HMERApi.Controllers;

/// <summary>
/// API controller for managing Product resources
/// Provides endpoints for CRUD operations on handwritten math expressions
/// </summary>
[ApiController]
[Route("[controller]")]
public partial class ProductController(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IProcessService processService,
    ILogger<ProductController> logger,
    IMapper mapper
    ) : ControllerBase
{
    /// <summary>
    /// Creates a new product by processing an uploaded image of a handwritten math expression
    /// </summary>
    /// <param name="productToAdd">The product data to add</param>
    /// <returns>The created product</returns>
    /// <response code="201">Returns the newly created product</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an error processing the image</response>
    [HttpPost]
    [RequestSizeLimit(3 * 1024 * 1024)] // Limit file size to 3MB
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDTO productToAdd)
    {
        try
        {
            // Validate input
            if (productToAdd.ImageFile == null)
            {
                return BadRequest("Image file is required.");
            }

            if (productToAdd.ImageFile.Length > 3 * 1024 * 1024)
            {
                return BadRequest("Image size should not exceed 3MB.");
            }

            // Save the image file
            string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];
            string createdImageName = await fileService.SaveFileAsync(productToAdd.ImageFile, allowedExtensions);
            
            string imagePrediction, speechOutput;

            // Process the image using the Python model
            try
            {
                (imagePrediction, speechOutput) = await processService.RunAsync(createdImageName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing image with Python script");
                return StatusCode(500, "Failed to process image");
            }

            // Create and save the product
            var product = new Product
            {
                Image = createdImageName,
                ImagePrediction = imagePrediction,
                PredictionDescription = speechOutput.Trim()
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

    /// <summary>
    /// Updates an existing product with a new image and prediction
    /// </summary>
    /// <param name="id">ID of the product to update</param>
    /// <param name="productToUpdate">The updated product data</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the product was successfully updated</response>
    /// <response code="404">If the product was not found</response>
    /// <response code="500">If there was an error updating the product</response>
    [HttpPut("{id}")]
    [RequestSizeLimit(3 * 1024 * 1024)] // Limit file size to 3MB
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDTO productToUpdate)
    {
        try
        {
            // Find the existing product
            var existingProduct = await unitOfWork.ProductRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            // Process new image if provided
            string oldImage = existingProduct.Image;
            if (productToUpdate.ImageFile != null)
            {
                // Validate file size
                if (productToUpdate.ImageFile.Length > 3 * 1024 * 1024)
                {
                    return BadRequest("Image size should not exceed 3MB.");
                }
                
                // Save the new image
                string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];
                string createdImageName = await fileService.SaveFileAsync(productToUpdate.ImageFile, allowedExtensions);

                string imagePrediction, speechOutput;

                // Process the new image
                try
                {
                    (imagePrediction, speechOutput) = await processService.RunAsync(createdImageName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing image with Python script");
                    return StatusCode(500, "Failed to process image");
                }

                // Update the product with new data
                existingProduct.Image = createdImageName;
                existingProduct.ImagePrediction = imagePrediction;
                existingProduct.PredictionDescription = speechOutput.Trim();

                // Delete the old image file
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

    /// <summary>
    /// Deletes a product by ID
    /// </summary>
    /// <param name="id">ID of the product to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the product was successfully deleted</response>
    /// <response code="404">If the product was not found</response>
    /// <response code="500">If there was an error deleting the product</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            // Find the existing product
            var existingProduct = await unitOfWork.ProductRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            // Delete product from database
            await unitOfWork.ProductRepository.DeleteByIdAsync(existingProduct.Id);
            
            // Delete the associated image file
            fileService.DeleteFile(existingProduct.Image);

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    /// <param name="id">ID of the product to retrieve</param>
    /// <returns>The product if found</returns>
    /// <response code="200">Returns the requested product</response>
    /// <response code="404">If the product was not found</response>
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

    /// <summary>
    /// Gets all products
    /// </summary>
    /// <returns>List of all products</returns>
    /// <response code="200">Returns all products</response>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await unitOfWork.ProductRepository.GetAllAsync();
        return Ok(products);
    }
}