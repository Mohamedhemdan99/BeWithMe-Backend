namespace BeWithMe.DTOs
{
    public class CalculateAge
    {
        public static int CalcAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // Check if birthday has occurred this year
            if (birthDate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
