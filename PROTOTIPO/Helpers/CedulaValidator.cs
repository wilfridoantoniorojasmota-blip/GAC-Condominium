using System.Text.RegularExpressions;

namespace PROTOTIPO.Helpers
{
    public static class CedulaValidator
    {
        /// <summary>
        /// Valida una cédula dominicana mediante:
        /// 1. Formato 000-0000000-0
        /// 2. Algoritmo del dígito verificador
        /// </summary>
        public static bool EsValida(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return false;

            // Eliminar guiones y espacios
            string numeros = Regex.Replace(cedula, @"[\s-]", "");

            // Debe contener exactamente 11 dígitos
            if (!Regex.IsMatch(numeros, @"^\d{11}$"))
                return false;

            // Los primeros 10 dígitos son los utilizados
            // para calcular el dígito verificador.
            int suma = 0;

            for (int i = 0; i < 10; i++)
            {
                int digito = numeros[i] - '0';

                // Multiplicadores alternados 1, 2
                int multiplicador = (i % 2 == 0) ? 1 : 2;

                int resultado = digito * multiplicador;

                // Si el resultado tiene dos dígitos,
                // se suman sus dígitos.
                suma += resultado >= 10
                    ? (resultado / 10) + (resultado % 10)
                    : resultado;
            }

            // Calcular dígito verificador
            int digitoVerificador =
                (10 - (suma % 10)) % 10;

            int ultimoDigito =
                numeros[10] - '0';

            return digitoVerificador == ultimoDigito;
        }


        /// <summary>
        /// Devuelve la cédula con formato 000-0000000-0.
        /// </summary>
        public static string Formatear(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return cedula;

            string numeros = Regex.Replace(cedula, @"\D", "");

            if (numeros.Length > 11)
                numeros = numeros.Substring(0, 11);

            if (numeros.Length <= 3)
                return numeros;

            if (numeros.Length <= 10)
                return $"{numeros.Substring(0, 3)}-{numeros.Substring(3)}";

            return $"{numeros.Substring(0, 3)}-" +
                   $"{numeros.Substring(3, 7)}-" +
                   $"{numeros.Substring(10, 1)}";
        }
    }
}