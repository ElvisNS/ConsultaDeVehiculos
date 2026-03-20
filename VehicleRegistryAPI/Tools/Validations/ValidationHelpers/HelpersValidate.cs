using System.Text.RegularExpressions;

namespace VehicleRegistryAPI.Tools.Validations.ValidationHelpers
{
    public class HelpersValidate
    {
        public static bool IsValidName(string name)
        {
            // Permite letras y espacios (incluye acentos y letras internacionales)
            return name.All(c => Char.IsLetter(c) || Char.IsWhiteSpace(c));
        }

        public static bool IsValidNationalId(string nationalId)
        {
            //000-0000000-0
            return Regex.IsMatch(nationalId, @"^\d{3}-\d{7}-\d{1}$");
        }

        public static bool IsValidPlate(string plateNumber)
        {
            //A123456
            return Regex.IsMatch(plateNumber, @"^[a-zA-Z]{1}[0-9]{6}$");
        }

    }
}
