
public static class Manager
{
    public static bool  gameStarted = false;
    public static bool  choosingCar = false;
    public static float dynamicLatitude = .0f;
    public static float dynamicLongitude = .0f;
    public static bool  locationQueryComplete = false;
    public static int   selectedVehicle = 0;
    
    public static readonly int DODGE_SELECTED  = 1;
    public static readonly int TUKTUK_SELECTED = 0;

    private static readonly int UNLOCKED = 1;
    private static readonly int LOCKED = 0;

    public static readonly string secondCarUnlockedString = "SecondCarUnlocked";
    public static int secondCarUnlocked= 0;
    public static readonly string completedOutdoorDeliveriesString = "CompletedOutdoorDeliveries";
    public static int completedOutdoorDeliveries = 0;
    public static readonly string multipleRestaurantsUnlockedString = "MultipleRestaurantsUnlocked";
    public static int multipleRestaurantsUnlocked = 0;
    public static readonly string fastestOutdoorDeliveryString = "FastestOutdoorDelivery";
    public static int fastestOutdoorDelivery = 0;
    public static readonly string dynamicLoadingUnlockedString = "DynamicLoadingUnlocked";
    public static int dynamicLoadingUnlocked = 0;
    public static readonly string consecutiveOutdoorDeliveriesString = "ConsecutiveOutdoorDeliveries";
    public static int consecutiveOutdoorDeliveries = 0;
}