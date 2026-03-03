using PackageFoodManagementSystem.Repository.Models;
using PackageFoodManagementSystem.Services.Interfaces;
using PackageFoodManagementSystem.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        // --- READ OPERATIONS ---

        public IEnumerable<Product> GetAllProducts() => _repo.GetAllProducts();

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await Task.Run(() => _repo.GetAllProducts());
        }

        public Product GetProductById(int id) => _repo.GetProductById(id);

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await Task.Run(() => _repo.GetProductById(id));
        }

        // --- WRITE OPERATIONS ---

        public void CreateProduct(Product product)
        {
            _repo.AddProduct(product);
            _repo.Save();
        }

        public void UpdateProduct(Product product)
        {
            _repo.UpdateProduct(product);
            _repo.Save();
        }

        // Added to support the Task-based calls in AdminController and BatchService
        public async Task UpdateProductAsync(Product product)
        {
            await Task.Run(() =>
            {
                _repo.UpdateProduct(product);
                _repo.Save();
            });
        }

        public void DeleteProduct(int id)
        {
            var product = _repo.GetProductById(id);
            if (product != null)
            {
                _repo.RemoveProduct(product);
                _repo.Save();
            }
        }

        // --- INTERFACE COMPLIANCE ---

        public IEnumerable<Product> GetMenuForCustomer() => _repo.GetAllProducts();
        public void CreateNewProduct(Product product) => CreateProduct(product);
        public void RemoveProduct(int id) => DeleteProduct(id);
    }
}