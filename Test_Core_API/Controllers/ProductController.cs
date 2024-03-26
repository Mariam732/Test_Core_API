using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Test_Core_API.Entity;
using Test_Core_API.Model;

namespace Test_Core_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        public Context Contxt { get; }

        public ProductController(Context context)
        {
            Contxt = context;
        }


        // GET: api/Product
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            List<Product> products = Contxt.products.ToList();
            if (products is null)
            {
                // if there is no data it will return status code 404 (not found)
                return NotFound();
            }
            // else return list of products
            return Ok(new { msg = "All Data is retervied successfully !!", Products = products });
        }



        // GET: api/Product/{id}

        //[HttpGet]
        //[Route("{id:alpha}", Name = "GetOneProduct")]
        //public IActionResult GetProduct(string id)
        //{
        //    Product product = Contxt.products.Find(id);
        //    if (product is null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(new { msg = "One Product is retervied successfully !!", product });

        //}


        // GET: api/Product/{name}


        [HttpGet]
        [Route("{name:alpha}", Name = "GetProduct")]
        public IActionResult GetByName(string name)
        {
            Product product = Contxt.products.FirstOrDefault(s => s.Name == name);

            if (product is null)
            {
                return NotFound();
            }
            return Ok(product);

        }


        // POST: api/Product


        [HttpPost]

        public IActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                Contxt.products.Add(product);
                Contxt.SaveChanges();
                string url = Url.Link("GetProduct", new { id = product.Name});
                return Created(url, product);
                // return Ok(new {msg ="insert operation is done" , newProduct = product});
            }
            return BadRequest(new { msg = "Error!!!", Errors = ModelState });
        }




        // PUT: api/Product


        [HttpPut]
        public IActionResult UpdateProduct(Product? product)
        {
            if (ModelState.IsValid)
            {
                Contxt.products.Update(product);
                Contxt.SaveChanges();
                return Ok(new { msg = "Update operation is done", updateProduct = product });

            }
            return BadRequest(ModelState);
        }


        // Delete: api/Product
        //

        [HttpDelete("{name:alpha}")]
        public IActionResult DeleteProduct(string name)
        {
            Product delProduct = Contxt.products.FirstOrDefault(s => s.Name == name);

            if (delProduct is null) return NotFound();
            Contxt.products.Remove(delProduct);
            Contxt.SaveChanges();
            return Ok(new { msg = "Delete operation is done", DeletedProduct = delProduct });

        }


    }

}
