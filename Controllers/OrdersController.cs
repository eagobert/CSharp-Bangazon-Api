using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using bangazon_inc.Data;
using bangazon_inc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;

namespace bangazon_inc.Controllers
{
    [Route("[controller]")]
    [EnableCors("BangazonAllowed")]

    public class OrdersController : Controller
    {
        private BangazonContext _context;
        // Constructor method to create an instance of context to communicate with our database.
        public OrdersController(BangazonContext ctx)
        {
            _context = ctx;
        }
        // GET all
        [HttpGet]
        public IActionResult Get()
        {
            var orders = _context.Orders.ToList();
            if (orders == null)
            {
                return NotFound();
            }
            return Ok(orders);
        }

        // GET url/Orders/{id}
        // Gets one order based on an id
        // Formats it for the purposes of clean JSON
        [HttpGet("{id}", Name = "GetSingleOrder")]
        public IActionResult Get([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Orders orders = _context.Orders.Include("ProductOrders.Product").Single(m => m.OrderId == id);
                List<Product> theseProducts = new List<Product>();
                foreach (OrderProduct orderProduct in orders.ProductOrders)
                {
                    theseProducts.Add(orderProduct.Product);
                }
                List<ProductOnOrderJSON> jSONProducts = new List<ProductOnOrderJSON>();
                foreach (Product product in theseProducts)
                {
                    ProductOnOrderJSON newProduct = new ProductOnOrderJSON()
                    {
                        ProductId = product.ProductId,
                        Name = product.Title,
                        Price = product.Price,
                        Quantity = 1
                    };
                    jSONProducts.Add(newProduct);
                }
                OrderWithProductJSON orderWithProducts = new OrderWithProductJSON()
                {
                    OrderId = orders.OrderId,
                    CustomerId = orders.CustomerId,
                    PaymentTypeId = orders.CustomerPaymentId,
                    Products = jSONProducts
                };
                if (orders == null)
                {
                    return NotFound();
                }

                return Ok(orderWithProducts);
            }
            catch (System.InvalidOperationException ex)
            {
                return NotFound(ex);
            }
        }

        // POST
        // example post request
        /*
       {
           "OrderId: 1,
           "OrderDate": "1483387800",
           "CompleteStatus": 0,
           "CustomerPaymentId": 2,
           "CustomerId": 2
       }
       */
        [HttpPost]
        public IActionResult Post([FromBody]Orders Orders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _context.Orders.Add(Orders);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (OrderExists(Orders.OrderId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }
            return CreatedAtRoute("GetSingleOrder", new { id = Orders.OrderId }, Orders);
        }
        // PUT
        // example put request
        /*
       {
           "OrderId: 1,
           "OrderDate": "1483387800",
           "CompleteStatus": 0,
           "CustomerPaymentId": 2,
           "CustomerId": 2
       }
       */
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Orders modifiedOrders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != modifiedOrders.OrderId)
            {
                return BadRequest();
            }
            _context.Orders.Update(modifiedOrders);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // return new StatusCodeResult(StatusCodes.Status204NoContent);
            return Ok(modifiedOrders);
        }

        // DELETE
        // example delete request
        /*
        {
            "OrderId: 1,
            "OrderDate": "1483387800",
            "CompleteStatus": 0,
            "CustomerPaymentId": 2,
            "CustomerId": 2
        }
        */
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Orders Orders = _context.Orders.Single(o => o.OrderId == id);

            if (Orders == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(Orders);
            _context.SaveChanges();
            return Ok(Orders);
        }

        private bool OrderExists(int OrderId)
        {
            return _context.Orders.Any(o => o.OrderId == OrderId);
        }

    }
}