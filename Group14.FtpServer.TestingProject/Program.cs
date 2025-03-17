using Group14.FtpServer;

class Program
{
    static void Main(string[] args)
    {
        var options = new FtpServerOptions
        {
            RootPath = @"C:\FtpRoot"
        };

        var server = new FtpServer(options);

        server.Start();

        Console.WriteLine("FTP SERVE RUNS!");
        Console.ReadKey();
        server.Stop();
    }
}