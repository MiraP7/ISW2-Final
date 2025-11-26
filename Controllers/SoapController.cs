using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISW2_Primer_parcial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SoapController : ControllerBase
    {
        private static List<SoapService> _registeredServices = new();

        /// <summary>
        /// Obtiene información sobre cómo usar la API de SOAP Consumer
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(new
            {
                titulo = "SOAP Service Consumer",
                version = "1.0",
                descripcion = "Herramienta para consumir servicios SOAP usando dotnet-svcutil",
                pasos = new[]
                {
                    new { numero = 1, descripcion = "Instalar dotnet-svcutil globalmente" },
                    new { numero = 2, descripcion = "Generar clases proxy desde WSDL" },
                    new { numero = 3, descripcion = "Crear cliente SOAP en tu código" },
                    new { numero = 4, descripcion = "Invocar métodos del servicio remoto" }
                },
                documentacion = new
                {
                    herramienta = "https://github.com/dotnet/wcf/tree/main/src/dotnet-svcutil",
                    guia_instalacion = "dotnet tool install --global dotnet-svcutil",
                    ejemplo_uso = "dotnet-svcutil https://ejemplo.com/servicio.wsdl"
                }
            });
        }

        /// <summary>
        /// Obtiene las instrucciones para instalar dotnet-svcutil
        /// </summary>
        [HttpPost("install-tool")]
        public async Task<IActionResult> InstallTool()
        {
            await Task.Delay(500); // Simular proceso
            return Ok(new
            {
                estado = "success",
                mensaje = "Instrucciones para instalar dotnet-svcutil",
                comando = "dotnet tool install --global dotnet-svcutil",
                descripcion = "dotnet-svcutil es una herramienta que genera clases C# proxy a partir de un archivo WSDL",
                proximo_paso = "Usar 'dotnet-svcutil [URL_WSDL]' para generar las clases proxy"
            });
        }

        /// <summary>
        /// Genera proxy desde una URL WSDL
        /// </summary>
        [HttpPost("generate-proxy")]
        public async Task<IActionResult> GenerateProxy([FromBody] GenerateProxyRequest request)
        {
            if (string.IsNullOrEmpty(request.WsdlUrl))
            {
                return BadRequest(new { error = "La URL del WSDL es requerida" });
            }

            await Task.Delay(1000); // Simular generación
            return Ok(new
            {
                estado = "success",
                mensaje = "Proxy generado exitosamente",
                wsdlUrl = request.WsdlUrl,
                archivosGenerados = new[]
                {
                    "Reference.cs",
                    "*.wsdl",
                    "*.xsd"
                },
                proximo_paso = "Incluir los archivos generados en tu proyecto y crear un cliente",
                ejemplo_codigo = request.WsdlUrl.Contains("dneonline.com/calculator") ? @"
var binding = new System.ServiceModel.BasicHttpBinding();
var endpoint = new System.ServiceModel.EndpointAddress(""http://www.dneonline.com/calculator.asmx"");
var client = new CalculatorSoapClient(binding, endpoint);

try
{
    // Ejemplo: sumar 15 + 27
    int resultado = await client.AddAsync(15, 27);
    Console.WriteLine($""Resultado de 15 + 27 = {resultado}"");
    
    // Ejemplo: multiplicar 10 * 5
    int multiplicacion = await client.MultiplyAsync(10, 5);
    Console.WriteLine($""Resultado de 10 * 5 = {multiplicacion}"");
}
catch (Exception ex)
{
    Console.WriteLine($""Error: {ex.Message}"");
}" : @"
var binding = new System.ServiceModel.BasicHttpBinding();
var endpoint = new System.ServiceModel.EndpointAddress(""[URL_DEL_SERVICIO]"");
var client = new ServicioClient(binding, endpoint);

try
{
    var resultado = await client.MetodoAsync(parametros);
    Console.WriteLine(resultado);
}
catch (Exception ex)
{
    Console.WriteLine($""Error: {ex.Message}"");
}"
            });
        }

        /// <summary>
        /// Consume un servicio SOAP registrado
        /// </summary>
        [HttpPost("consume-service")]
        public async Task<IActionResult> ConsumeService([FromBody] ConsumeServiceRequest request)
        {
            if (string.IsNullOrEmpty(request.ServiceName) || string.IsNullOrEmpty(request.MethodName))
            {
                return BadRequest(new { error = "Service name y method name son requeridos" });
            }

            // Simular consumo del servicio
            await Task.Delay(800);

            return Ok(new
            {
                estado = "success",
                mensaje = "Servicio consumido exitosamente",
                servicio = request.ServiceName,
                metodo = request.MethodName,
                parametros = request.Parameters,
                respuesta = new
                {
                    Status = "Success",
                    Message = $"Datos obtenidos del servicio {request.ServiceName}",
                    Data = new
                    {
                        Id = 1,
                        Nombre = "Ejemplo",
                        Valor = 100,
                        Timestamp = DateTime.UtcNow
                    }
                }
            });
        }

        /// <summary>
        /// Registra un nuevo servicio SOAP
        /// </summary>
        [HttpPost("register-service")]
        public IActionResult RegisterService([FromBody] RegisterServiceRequest request)
        {
            if (string.IsNullOrEmpty(request.ServiceName) || string.IsNullOrEmpty(request.WsdlUrl))
            {
                return BadRequest(new { error = "Service name y WSDL URL son requeridos" });
            }

            var service = new SoapService
            {
                Id = Guid.NewGuid(),
                ServiceName = request.ServiceName,
                WsdlUrl = request.WsdlUrl,
                RegistrationDate = DateTime.UtcNow
            };

            _registeredServices.Add(service);

            return Ok(new
            {
                estado = "success",
                mensaje = "Servicio registrado exitosamente",
                servicio = service
            });
        }

        /// <summary>
        /// Obtiene lista de servicios SOAP registrados
        /// </summary>
        [HttpGet("services")]
        public IActionResult GetServices()
        {
            if (_registeredServices.Count == 0)
            {
                return Ok(new
                {
                    mensaje = "No hay servicios registrados",
                    servicios = new List<object>(),
                    total = 0
                });
            }

            return Ok(new
            {
                mensaje = "Servicios SOAP registrados",
                servicios = _registeredServices,
                total = _registeredServices.Count
            });
        }

        /// <summary>
        /// Elimina un servicio registrado
        /// </summary>
        [HttpDelete("services/{id}")]
        public IActionResult DeleteService(Guid id)
        {
            var service = _registeredServices.FirstOrDefault(s => s.Id == id);
            if (service == null)
            {
                return NotFound(new { error = "Servicio no encontrado" });
            }

            _registeredServices.Remove(service);
            return Ok(new
            {
                estado = "success",
                mensaje = "Servicio eliminado exitosamente",
                servicio = service
            });
        }

        /// <summary>
        /// Obtiene ejemplos de consumo de SOAP
        /// </summary>
        [HttpGet("ejemplos")]
        public IActionResult GetExamples()
        {
            return Ok(new
            {
                titulo = "Ejemplos de Consumo de Servicios SOAP",
                servicios_disponibles = new object[]
                {
                    new
                    {
                        nombre = "DNE Online Calculator Service",
                        url_wsdl = "http://www.dneonline.com/calculator.asmx?wsdl",
                        url_servicio = "http://www.dneonline.com/calculator.asmx",
                        metodos = new[] { "Add", "Subtract", "Multiply", "Divide" },
                        descripcion = "Servicio SOAP simple para operaciones matemáticas"
                    }
                },
                pasos = new object[]
                {
                    new
                    {
                        titulo = "Paso 1: Instalar herramienta",
                        comando = "dotnet tool install --global dotnet-svcutil"
                    },
                    new
                    {
                        titulo = "Paso 2: Generar proxy desde WSDL del Calculator",
                        comando = "dotnet-svcutil http://www.dneonline.com/calculator.asmx?wsdl -o CalculatorProxy"
                    },
                    new
                    {
                        titulo = "Paso 3: Crear cliente SOAP en código",
                        codigo = @"
using System.ServiceModel;

var binding = new BasicHttpBinding();
var endpoint = new EndpointAddress(""http://www.dneonline.com/calculator.asmx"");
var client = new CalculatorSoapClient(binding, endpoint);

try
{
    // Sumar dos números
    int resultado = await client.AddAsync(10, 5);
    Console.WriteLine($""10 + 5 = {resultado}"");

    // Restar
    int resta = await client.SubtractAsync(10, 5);
    Console.WriteLine($""10 - 5 = {resta}"");

    // Multiplicar
    int mult = await client.MultiplyAsync(10, 5);
    Console.WriteLine($""10 * 5 = {mult}"");

    // Dividir
    int div = await client.DivideAsync(10, 5);
    Console.WriteLine($""10 / 5 = {div}"");
}
catch (Exception ex)
{
    Console.WriteLine($""Error al conectar con el servicio: {ex.Message}"");
}"
                    },
                    new
                    {
                        titulo = "Paso 4: Desde la API REST",
                        metodo = "POST",
                        endpoint = "/api/soap/register-service",
                        payload = new
                        {
                            serviceName = "Calculator Service",
                            wsdlUrl = "http://www.dneonline.com/calculator.asmx?wsdl"
                        }
                    }
                }
            });
        }
    }

    // DTOs
    public class GenerateProxyRequest
    {
        public string? WsdlUrl { get; set; }
    }

    public class ConsumeServiceRequest
    {
        public string? ServiceName { get; set; }
        public string? MethodName { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class RegisterServiceRequest
    {
        public string? ServiceName { get; set; }
        public string? WsdlUrl { get; set; }
    }

    public class SoapService
    {
        public Guid Id { get; set; }
        public string? ServiceName { get; set; }
        public string? WsdlUrl { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
