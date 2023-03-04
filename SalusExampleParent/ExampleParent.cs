namespace SalusExampleParent;

internal class ExampleParent
{
    private readonly ExampleDbContext _context;

    public ExampleParent(ExampleDbContext context)
    {
        _context = context;
    }

    public async Task Run()
    {
        _context.ExampleData.Add(new ExampleData { Data = "Example" });
        _context.SaveChanges();


        const int DELAY = 10000;

        while (true)
        {
            Console.WriteLine("Running");

            for (int i = 0; i < DELAY / 100; i++)
            {
                await Task.Delay(100);
                if (Console.KeyAvailable)
                {
                    Console.WriteLine("Key pressed - exiting");
                    return;
                }
            }
        }

    }
}
