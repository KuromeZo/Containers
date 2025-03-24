
interface IHazardNotifier
{
    void NotifyHazard(string message);
}

abstract class Container
{
    private static int contAmount = 1;
    public string id { get; private set; }
    public double CargoMass { get; set; }
    public double ContMass { get; }
    public double MaxPayload { get; }
    public int Height { get; }
    public int Depth { get; }

    protected Container(string type, double contMass, double maxPayload, int height, int depth)
    {
        id = $"KON-{type}-{contAmount++}";
        ContMass = contMass;
        MaxPayload = maxPayload;
        Height = height;
        Depth = depth;
    }

    public void LoadCargo(double mass)
    {
        if (mass > MaxPayload)
            throw new Exception("OverfillException: exceeding the load capacity!");
        CargoMass = mass;
    }

    public void UnloadCargo()
    {
        CargoMass = 0;
    }

    public override string ToString()
    {
        return $"Container {id}, Cargo Mass: {CargoMass} kg, Max Payload: {MaxPayload} kg";
    }
}

class LiquidContainer : Container, IHazardNotifier
{
    public bool IsHazardous { get; }

    public LiquidContainer(double contMass, double maxPayload, int height, int depth, bool isHazardous)
        : base("L", contMass, maxPayload, height, depth)
    {
        IsHazardous = isHazardous;
    }

    public new void LoadCargo(double mass)
    {
        double maxAllowed = IsHazardous ? MaxPayload * 0.5 : MaxPayload * 0.9;
        if (mass > maxAllowed)
        {
            NotifyHazard($"Hazardous operation: attem to overload {id}");
            throw new Exception("OverfillException: hazardous material limit exceeded!");
        }
        base.LoadCargo(mass);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[HAZARD] {message}");
    }
}

class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; }

    public GasContainer(double contMass, double maxPayload, int height, int depth, double pressure)
        : base("G", contMass, maxPayload, height, depth)
    {
        Pressure = pressure;
    }

    public new void UnloadCargo()
    {
        CargoMass *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[HAZARD] {message}");
    }
}

class RefrigeratedContainer : Container
{
    public string ProductType { get; }
    public double Temperature { get; }

    public RefrigeratedContainer(double contMass, double maxPayload, int height, int depth, string productType, double temperature)
        : base("C", contMass, maxPayload, height, depth)
    {
        ProductType = productType;
        Temperature = temperature;
    }

    public override string ToString()
    {
        return base.ToString() + $", Product: {ProductType}, Temperature: {Temperature}°C";
    }
}

class ContainerShip
{
    public List<Container> Containers { get; private set; } = new List<Container>();
    public double MaxSpeed { get; }
    public int MaxContCount { get; }
    public double MaxWeight { get; } 

    public ContainerShip(double maxSpeed, int maxContCount, double maxWeight)
    {
        MaxSpeed = maxSpeed;
        MaxContCount = maxContCount;
        MaxWeight = maxWeight;
    }

    public void LoadCont(Container container)
    {
        if (Containers.Count >= MaxContCount)
            throw new Exception("Ship capacity exceeded!");

        double totalWeight = Containers.Sum(c => c.ContMass + c.CargoMass) + container.ContMass + container.CargoMass;
        if (totalWeight > MaxWeight * 1000) // Convert tons to kg
            throw new Exception("Ship weight limit exceeded!");

        Containers.Add(container);
    }

    public void RemoveCont(string serialNumber)
    {
        Containers.RemoveAll(c => c.id == serialNumber);
    }

    public void ReplaceCont(string serialNumber, Container newContainer)
    {
        RemoveCont(serialNumber);
        LoadCont(newContainer);
    }

    public void TransferCont(ContainerShip targetShip, string serialNumber)
    {
        var container = Containers.FirstOrDefault(c => c.id == serialNumber);
        if (container != null)
        {
            RemoveCont(serialNumber);
            targetShip.LoadCont(container);
        }
    }

    public void PrintShipInfo()
    {
        Console.WriteLine($"Ship Speed: {MaxSpeed} knots, Max Containers: {MaxContCount}, Max Weight: {MaxWeight} tons");
        Console.WriteLine("Containers on board:");
        foreach (var container in Containers)
        {
            Console.WriteLine(container);
        }
    }
}

class Program
{
    static void Main()
    {
        ContainerShip ship1 = new ContainerShip(30, 5, 35);
        ContainerShip ship2 = new ContainerShip(25, 3, 80);

        RefrigeratedContainer refContainer = new RefrigeratedContainer(2000, 10000, 250, 300, "Bananas", 13.3);
        LiquidContainer liquidContainer = new LiquidContainer(1500, 12000, 200, 250, false);
        GasContainer gasContainer = new GasContainer(1800, 9000, 220, 270, 5);

        
        /*Console.WriteLine("\nTrying to overload the ship...");
        RefrigeratedContainer heavyContainer = new RefrigeratedContainer(5000, 50000, 300, 400, "Meat", -15);
        heavyContainer.LoadCargo(45000);
        ship1.LoadCont(heavyContainer);*/
        
        try
        {
            refContainer.LoadCargo(9000);
            liquidContainer.LoadCargo(10000);
            gasContainer.LoadCargo(8500);

            ship1.LoadCont(refContainer);
            ship1.LoadCont(liquidContainer);
            ship1.LoadCont(gasContainer);

            Console.WriteLine("Initial Ship State:");
            ship1.PrintShipInfo();

            Console.WriteLine("\nTransferring container...");
            ship1.TransferCont(ship2, refContainer.id);

            Console.WriteLine("\nShip 1 after transfer:");
            ship1.PrintShipInfo();

            Console.WriteLine("\nShip 2 after receiving container:");
            ship2.PrintShipInfo();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}