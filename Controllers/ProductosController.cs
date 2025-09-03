using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductosApi.Productos;

namespace ProductosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private static readonly List<Producto> _productos = new()
        {
            new Producto { Id = 1, Nombre = "Manzanas", Precio = 1.50m, Disponible = true },
            new Producto { Id = 2, Nombre = "Uvas", Precio = 2.75m, Disponible = true },
            new Producto { Id = 3, Nombre = "Duraznos", Precio = 3.10m, Disponible = false },
        };
        private static int _nextId = 4;
        private static readonly object _lock = new();

        [HttpGet("{id:int}")]
        public ActionResult<Producto> GetById(int id)
        {
            lock (_lock)
            {
                var p = _productos.FirstOrDefault(x => x.Id == id);
                return p is null ? NotFound() : Ok(p);
            }
        }

    }
}
