using System.Reflection;

namespace GHelperLinux
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("G-Helper for Linux");
            Console.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine("Porting Windows G-Helper to Linux using asus-linux tools");
            Console.WriteLine("=================================\n");

            var controller = new LinuxAsusControl();
            
            if (!controller.Initialize())
            {
                Console.WriteLine("Failed to initialize ASUS Linux control.");
                Console.WriteLine("\nPlease ensure asus-linux tools are installed:");
                Console.WriteLine("  Ubuntu/Debian: sudo apt install asusctl supergfxctl");
                Console.WriteLine("  Fedora: sudo dnf install asusctl supergfxctl");
                Console.WriteLine("  Arch: sudo pacman -S asusctl supergfxctl");
                Console.WriteLine("  Or visit: https://asus-linux.org/");
                Environment.Exit(1);
                return;
            }

            // Show current system status
            controller.ShowSystemInfo();

            if (args.Length > 0)
            {
                HandleCommand(controller, args);
            }
            else
            {
                RunInteractiveMode(controller);
            }
        }

        static void HandleCommand(LinuxAsusControl controller, string[] args)
        {
            string command = args[0].ToLower();

            switch (command)
            {
                case "--performance":
                case "-p":
                    if (args.Length > 1)
                    {
                        controller.SetPerformanceMode(args[1]);
                    }
                    else
                    {
                        Console.WriteLine($"Current performance mode: {controller.GetPerformanceMode()}");
                    }
                    break;

                case "--gpu":
                case "-g":
                    if (args.Length > 1)
                    {
                        controller.SetGPUMode(args[1]);
                    }
                    else
                    {
                        Console.WriteLine($"Current GPU mode: {controller.GetGPUMode()}");
                    }
                    break;

                case "--battery":
                case "-b":
                    if (args.Length > 1 && int.TryParse(args[1], out int limit))
                    {
                        controller.SetBatteryLimit(limit);
                    }
                    else
                    {
                        int currentLimit = controller.GetBatteryLimit();
                        if (currentLimit > 0)
                        {
                            Console.WriteLine($"Current battery charge limit: {currentLimit}%");
                        }
                        else
                        {
                            Console.WriteLine("Battery charge limit not available or not set");
                        }
                    }
                    break;

                case "--keyboard":
                case "-k":
                    if (args.Length > 1 && int.TryParse(args[1], out int brightness))
                    {
                        controller.SetKeyboardBrightness(brightness);
                    }
                    else
                    {
                        int currentBrightness = controller.GetKeyboardBrightness();
                        if (currentBrightness >= 0)
                        {
                            Console.WriteLine($"Current keyboard brightness: {currentBrightness}");
                        }
                        else
                        {
                            Console.WriteLine("Keyboard brightness not available");
                        }
                    }
                    break;

                case "--status":
                case "-s":
                    controller.ShowSystemInfo();
                    break;

                case "--profiles":
                    controller.ListAvailableProfiles();
                    break;

                case "--help":
                case "-h":
                    ShowHelp();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        static void RunInteractiveMode(LinuxAsusControl controller)
        {
            Console.WriteLine("Interactive mode - type 'help' for commands or 'quit' to exit");
            Console.WriteLine("Available commands: performance, gpu, battery, keyboard, status, profiles, help, quit\n");

            while (true)
            {
                Console.Write("ghelper> ");
                string? input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(input))
                    continue;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0];

                switch (command)
                {
                    case "performance":
                    case "p":
                        if (parts.Length > 1)
                        {
                            controller.SetPerformanceMode(parts[1]);
                        }
                        else
                        {
                            Console.WriteLine($"Current: {controller.GetPerformanceMode()}");
                            Console.WriteLine("Usage: performance <Quiet|Balanced|Performance>");
                        }
                        break;

                    case "gpu":
                    case "g":
                        if (parts.Length > 1)
                        {
                            controller.SetGPUMode(parts[1]);
                        }
                        else
                        {
                            Console.WriteLine($"Current: {controller.GetGPUMode()}");
                            Console.WriteLine("Usage: gpu <Integrated|Hybrid|Vfio>");
                        }
                        break;

                    case "battery":
                    case "b":
                        if (parts.Length > 1 && int.TryParse(parts[1], out int limit))
                        {
                            controller.SetBatteryLimit(limit);
                        }
                        else
                        {
                            int currentLimit = controller.GetBatteryLimit();
                            if (currentLimit > 0)
                            {
                                Console.WriteLine($"Current: {currentLimit}%");
                            }
                            else
                            {
                                Console.WriteLine("Not available or not set");
                            }
                            Console.WriteLine("Usage: battery <20-100>");
                        }
                        break;

                    case "keyboard":
                    case "k":
                        if (parts.Length > 1 && int.TryParse(parts[1], out int brightness))
                        {
                            controller.SetKeyboardBrightness(brightness);
                        }
                        else
                        {
                            int currentBrightness = controller.GetKeyboardBrightness();
                            if (currentBrightness >= 0)
                            {
                                Console.WriteLine($"Current: {currentBrightness}");
                            }
                            else
                            {
                                Console.WriteLine("Not available");
                            }
                            Console.WriteLine("Usage: keyboard <0-3>");
                        }
                        break;

                    case "status":
                    case "s":
                        controller.ShowSystemInfo();
                        break;

                    case "profiles":
                        controller.ListAvailableProfiles();
                        break;

                    case "help":
                    case "h":
                        ShowInteractiveHelp();
                        break;

                    case "quit":
                    case "q": 
                    case "exit":
                        Console.WriteLine("Goodbye!");
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }

                Console.WriteLine();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("\nG-Helper Linux - ASUS laptop control using asus-linux tools");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  ghelper-linux [command] [value]");
            Console.WriteLine("\nCommands:");
            Console.WriteLine("  -p, --performance [mode]    Set/get performance mode (Quiet|Balanced|Performance)");
            Console.WriteLine("  -g, --gpu [mode]            Set/get GPU mode (Integrated|Hybrid|Vfio)");
            Console.WriteLine("  -b, --battery [limit]       Set/get battery charge limit (20-100)");
            Console.WriteLine("  -k, --keyboard [brightness] Set/get keyboard brightness (0-3)");
            Console.WriteLine("  -s, --status                Show system status");
            Console.WriteLine("      --profiles              List available performance profiles");
            Console.WriteLine("  -h, --help                  Show this help");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  ghelper-linux --performance Quiet");
            Console.WriteLine("  ghelper-linux --gpu Integrated");
            Console.WriteLine("  ghelper-linux --battery 80");
            Console.WriteLine("  ghelper-linux --status");
            Console.WriteLine("\nWithout arguments, starts interactive mode.");
        }

        static void ShowInteractiveHelp()
        {
            Console.WriteLine("Interactive mode commands:");
            Console.WriteLine("  performance [mode]       Set/get performance mode");
            Console.WriteLine("  gpu [mode]              Set/get GPU mode");
            Console.WriteLine("  battery [limit]         Set/get battery charge limit");
            Console.WriteLine("  keyboard [brightness]   Set/get keyboard brightness");
            Console.WriteLine("  status                  Show system status");
            Console.WriteLine("  profiles                List available profiles");
            Console.WriteLine("  help                    Show this help");
            Console.WriteLine("  quit                    Exit interactive mode");
        }
    }
}