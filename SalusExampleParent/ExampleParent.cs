namespace SalusExampleParent;

internal class ExampleParent
{
    public ExampleParent(ExampleDbContext context)
    {
    }

    public async Task Run()
    {
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
