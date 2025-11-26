using Microsoft.AspNetCore.Mvc;
using ISW2_Primer_parcial.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ISW2_Primer_parcial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly CalculatorService _calculatorService;

        public CalculatorController()
        {
            _calculatorService = new CalculatorService();
        }

        /// <summary>
        /// Obtiene información sobre el servicio SOAP Calculator
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(new
            {
                serviceName = "DNE Online Calculator SOAP Service",
                url = "http://www.dneonline.com/calculator.asmx",
                wsdlUrl = "http://www.dneonline.com/calculator.asmx?wsdl",
                description = "Servicio SOAP que proporciona operaciones matemáticas básicas",
                methods = new[]
                {
                    new { name = "Add", description = "Suma dos números", endpoint = "POST /api/calculator/add" },
                    new { name = "Subtract", description = "Resta dos números", endpoint = "POST /api/calculator/subtract" },
                    new { name = "Multiply", description = "Multiplica dos números", endpoint = "POST /api/calculator/multiply" },
                    new { name = "Divide", description = "Divide dos números", endpoint = "POST /api/calculator/divide" }
                }
            });
        }

        /// <summary>
        /// Suma dos números (operación matemática básica)
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CalculatorOperationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int result = await _calculatorService.AddAsync(request.NumberA, request.NumberB);
                return Ok(new CalculatorOperationResponse
                {
                    Operation = "Add",
                    NumberA = request.NumberA,
                    NumberB = request.NumberB,
                    Result = result,
                    Expression = $"{request.NumberA} + {request.NumberB} = {result}",
                    Success = true
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error al realizar la operación",
                    message = ex.Message,
                    operation = "Add"
                });
            }
        }

        /// <summary>
        /// Resta dos números
        /// </summary>
        [HttpPost("subtract")]
        public async Task<IActionResult> Subtract([FromBody] CalculatorOperationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int result = await _calculatorService.SubtractAsync(request.NumberA, request.NumberB);
                return Ok(new CalculatorOperationResponse
                {
                    Operation = "Subtract",
                    NumberA = request.NumberA,
                    NumberB = request.NumberB,
                    Result = result,
                    Expression = $"{request.NumberA} - {request.NumberB} = {result}",
                    Success = true
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error al realizar la operación",
                    message = ex.Message,
                    operation = "Subtract"
                });
            }
        }

        /// <summary>
        /// Multiplica dos números
        /// </summary>
        [HttpPost("multiply")]
        public async Task<IActionResult> Multiply([FromBody] CalculatorOperationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int result = await _calculatorService.MultiplyAsync(request.NumberA, request.NumberB);
                return Ok(new CalculatorOperationResponse
                {
                    Operation = "Multiply",
                    NumberA = request.NumberA,
                    NumberB = request.NumberB,
                    Result = result,
                    Expression = $"{request.NumberA} * {request.NumberB} = {result}",
                    Success = true
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error al realizar la operación",
                    message = ex.Message,
                    operation = "Multiply"
                });
            }
        }

        /// <summary>
        /// Divide dos números
        /// </summary>
        [HttpPost("divide")]
        public async Task<IActionResult> Divide([FromBody] CalculatorOperationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.NumberB == 0)
            {
                return BadRequest(new
                {
                    error = "Error en la operación",
                    message = "No se puede dividir entre cero",
                    operation = "Divide"
                });
            }

            try
            {
                int result = await _calculatorService.DivideAsync(request.NumberA, request.NumberB);
                return Ok(new CalculatorOperationResponse
                {
                    Operation = "Divide",
                    NumberA = request.NumberA,
                    NumberB = request.NumberB,
                    Result = result,
                    Expression = $"{request.NumberA} / {request.NumberB} = {result}",
                    Success = true
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error al realizar la operación",
                    message = ex.Message,
                    operation = "Divide"
                });
            }
        }
    }

    /// <summary>
    /// DTO para solicitar operaciones de calculadora
    /// </summary>
    public class CalculatorOperationRequest
    {
        [Required(ErrorMessage = "NumberA es requerido")]
        public int NumberA { get; set; }

        [Required(ErrorMessage = "NumberB es requerido")]
        public int NumberB { get; set; }
    }

    /// <summary>
    /// DTO para respuestas de operaciones de calculadora
    /// </summary>
    public class CalculatorOperationResponse
    {
        public string? Operation { get; set; }
        public int NumberA { get; set; }
        public int NumberB { get; set; }
        public int Result { get; set; }
        public string? Expression { get; set; }
        public bool Success { get; set; }
    }
}
