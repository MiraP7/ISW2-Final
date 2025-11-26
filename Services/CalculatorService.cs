using System;
using System.ServiceModel;
using System.Threading.Tasks;
using ISW2_Primer_parcial.ServiceReferences;

namespace ISW2_Primer_parcial.Services
{
    /// <summary>
    /// Servicio para consumir el SOAP Calculator Service de DNE Online
    /// </summary>
    public class CalculatorService
    {
        private readonly string _serviceUrl = "http://www.dneonline.com/calculator.asmx";
        private readonly BasicHttpBinding _binding;

        public CalculatorService()
        {
            _binding = new BasicHttpBinding
            {
                Security = new BasicHttpSecurity
                {
                    Mode = BasicHttpSecurityMode.None
                }
            };
        }

        /// <summary>
        /// Suma dos números usando el servicio SOAP
        /// </summary>
        public async Task<int> AddAsync(int a, int b)
        {
            try
            {
                var endpoint = new EndpointAddress(_serviceUrl);
                var client = new CalculatorSoapClient(_binding, endpoint);
                
                int result = await client.AddAsync(a, b);
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al sumar {a} + {b}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Resta dos números usando el servicio SOAP
        /// </summary>
        public async Task<int> SubtractAsync(int a, int b)
        {
            try
            {
                var endpoint = new EndpointAddress(_serviceUrl);
                var client = new CalculatorSoapClient(_binding, endpoint);
                
                int result = await client.SubtractAsync(a, b);
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al restar {a} - {b}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Multiplica dos números usando el servicio SOAP
        /// </summary>
        public async Task<int> MultiplyAsync(int a, int b)
        {
            try
            {
                var endpoint = new EndpointAddress(_serviceUrl);
                var client = new CalculatorSoapClient(_binding, endpoint);
                
                int result = await client.MultiplyAsync(a, b);
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al multiplicar {a} * {b}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Divide dos números usando el servicio SOAP
        /// </summary>
        public async Task<int> DivideAsync(int a, int b)
        {
            try
            {
                if (b == 0)
                {
                    throw new ArgumentException("No se puede dividir entre cero");
                }

                var endpoint = new EndpointAddress(_serviceUrl);
                var client = new CalculatorSoapClient(_binding, endpoint);
                
                int result = await client.DivideAsync(a, b);
                return result;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al dividir {a} / {b}: {ex.Message}", ex);
            }
        }
    }
}
