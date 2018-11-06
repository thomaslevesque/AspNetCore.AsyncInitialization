namespace AspNetCore.AsyncInitialization
{
    internal class OrderedAsyncInitializer : IOrderedAsyncInitializer
    {
        public OrderedAsyncInitializer(IAsyncInitializer initializer, int order)
        {
            Initializer = initializer;
            Order = order;
        }

        public IAsyncInitializer Initializer { get; }
        public int Order { get; }
    }
}