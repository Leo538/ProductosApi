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

        private readonly ILogger<ProductosController> _logger;

        public ProductosController(ILogger<ProductosController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public ActionResult<Producto> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("El id debe ser mayor que 0.");

            lock (_lock)
            {
                var p = _productos.FirstOrDefault(x => x.Id == id);
                return p is null ? NotFound($"No se encontró un producto con Id={id}") : Ok(p);
            }
        }
        [HttpGet]
        public ActionResult<List<Producto>> GetAll()
        {
            lock (_lock)
            {
                var p = _productos;
                return p;
            }
        }

        [HttpPost]
        public ActionResult<Producto> PostProducto(Producto producto)
        {
            lock (_lock)
            {
                producto.Id = _nextId;
                _nextId++;
                _productos.Add(producto);
                return producto;
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Producto actualizado)
        {
            if (actualizado is null)
                return BadRequest("El cuerpo de la solictud es requerido.");

            if (string.IsNullOrWhiteSpace(actualizado.Nombre))
                return BadRequest("El nombre es obligatorio.");

            if (actualizado.Precio < 0)
                return BadRequest("El precio no puede ser negativo");

            lock (_lock)
            {
                var p = _productos.FirstOrDefault(x => x.Id == id);
                if (p is null) return NotFound();

                var before = new Producto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Disponible = p.Disponible
                };

                p.Nombre = actualizado.Nombre;
                p.Precio = actualizado.Precio;
                p.Disponible = actualizado.Disponible;

                var changedFields = new List<string>();
                if (before.Nombre != p.Nombre) changedFields.Add(nameof(p.Nombre));
                if (before.Precio != p.Precio) changedFields.Add(nameof(p.Precio));
                if (before.Disponible != p.Disponible) changedFields.Add(nameof(p.Disponible));

                var response = new
                {
                    message = "Producto actualizado correctamente",
                    before,
                    after = p,
                    changedFields
                };

                return Ok(response);
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            lock (_lock)
            {
                var p = _productos.FirstOrDefault(x => x.Id == id);
                if (p is null)
                    return NotFound(); 

                _productos.Remove(p);
                return NoContent();   
            }
        }
    }
}
