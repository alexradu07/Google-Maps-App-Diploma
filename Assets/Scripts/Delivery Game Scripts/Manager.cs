﻿
public static class Manager
{
    public static bool  gameStarted = false;
    public static bool  choosingCar = false;
    public static double dynamicLatitude = .0f;
    public static double dynamicLongitude = .0f;
    public static bool  locationQueryComplete = false;
    public static int   selectedVehicle = 0;

    public static readonly int DODGE_SELECTED  = 1;
    public static readonly int TUKTUK_SELECTED = 0;
}