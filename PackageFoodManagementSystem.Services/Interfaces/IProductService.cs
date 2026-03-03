using PackageFoodManagementSystem.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Product GetProductById(int id);

        // ADDED: Async version for inventory synchronization
        Task<Product> GetProductByIdAsync(int id);

        void CreateProduct(Product product);
        void UpdateProduct(Product product);

        // ADDED: Async version to support BatchService updates
        Task UpdateProductAsync(Product product);

        void DeleteProduct(int id);

        // INTERFACE MATCHING METHODS
        IEnumerable<Product> GetMenuForCustomer();
        void CreateNewProduct(Product product);
        void RemoveProduct(int id);
    }
}