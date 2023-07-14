namespace ConsoleApp1
{
    internal class Program
    {

        public class Car {

            public int? maxSpeed ;
            public string? brand;
            public bool isSport = false;
           
            public Car(string brand)
            {
                this.brand = brand;
                Console.WriteLine($"Car {brand} is created");
            }

            public string SetGetCarBrand(string brand)
            {
                return brand;
            }

        }







        static void Main(string[] args)
        {
            Type type = typeof(Car);
            var methods = type.GetMethods();
            Car car = new Car(null);
            
            var result = methods.First(a => a.Name == typeof(string))
                .Invoke(car, new string[] { "Mercedes"});
            
            Console.WriteLine(result);
        }
    }
}