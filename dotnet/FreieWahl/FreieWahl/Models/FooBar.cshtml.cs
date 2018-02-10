using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreieWahl.Models
{
    public class FooBarModel : PageModel
    {
        static int _count = 0;
        private readonly int _instanceCount;
        private readonly string _name;

        public FooBarModel()
        {
            _instanceCount = ++_count;
        }

        public FooBarModel(string name)
        {
            _instanceCount = ++_count;
            _name = name;
        }

        public int Count => _instanceCount;

        public string Name => _name;
    }
}
