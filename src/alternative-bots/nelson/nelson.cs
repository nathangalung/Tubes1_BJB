using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Nelson : Bot
{
    private bool movingForward = true;
    
    private int randomMoveCounter = 0;
    private const int RANDOM_MOVE_INTERVAL = 30; 

    private const double MARGIN = 60;

    private int turnsSinceScan = 0;      
    private const int SCAN_THRESHOLD = 20; 

    private bool dodgeMode = false;
    private int dodgeTurnsLeft = 0;

    static void Main()
    {
        new Nelson().Start();
    }

    public Nelson() : base(BotInfo.FromFile("Nelson.json")) { }

    public override void Run()
    {
        BodyColor   = Color.Lime;
        GunColor    = Color.Green;
        RadarColor  = Color.DarkCyan;
        BulletColor = Color.Yellow;
        ScanColor   = Color.LightPink;

        AdjustGunForBodyTurn   = true;
        AdjustRadarForBodyTurn = true;

        movingForward = true;
        TargetSpeed   = 5;
        TurnRate      = 0;

        while (IsRunning)
        {
            DoFallbackRadarIfLost();

            StayAwayFromWalls();

            randomMoveCounter++;
            if (randomMoveCounter > RANDOM_MOVE_INTERVAL)
            {
                DoRandomMove();
                randomMoveCounter = 0;  
            }

            if (dodgeMode)
                PerformDodgeManeuver();

            Go();
            turnsSinceScan++;
        }
    }

    //*************************************************************
    // Event Handlers
    //*************************************************************

    public override void OnScannedBot(ScannedBotEvent e)
    {
        turnsSinceScan = 0;

        double radarBearing = RadarBearingTo(e.X, e.Y);

        double overshoot = 5;
        radarBearing += (radarBearing < 0) ? -overshoot : overshoot;
        RadarTurnRate = Clip(radarBearing, -MaxRadarTurnRate, MaxRadarTurnRate);

        double distance = DistanceTo(e.X, e.Y);
        double firepower;
        if (distance < 200)
            firepower = 3.5;
        else if (distance < 500)
            firepower = 2.5;
        else if (distance < 800)
            firepower = 1.5;
        else
            firepower = 0.5;

        if (GunHeat == 0)
        {
            double gunBearing = GunBearingTo(e.X, e.Y);
            GunTurnRate = Clip(gunBearing, -MaxGunTurnRate, MaxGunTurnRate);

            SetFire(firepower);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (e.IsRammed)
            ReverseDirection();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        dodgeMode       = true;
        dodgeTurnsLeft  = 15; 
    }

    //*************************************************************
    // Helper Methods
    //*************************************************************

    private void ReverseDirection()
    {
        movingForward = !movingForward;
        TargetSpeed   = (movingForward) ? 5 : -5;
    }

    private void StayAwayFromWalls()
    {
        if (X < MARGIN)
            TurnRate = 10;
        else if (X > (ArenaWidth - MARGIN))
            TurnRate = -10;
        else if (Y < MARGIN)
            TurnRate = 10;
        else if (Y > (ArenaHeight - MARGIN))
            TurnRate = -10;
    }

    private void DoRandomMove()
    {
        double angle = new Random().NextDouble() * 45;     
        double dist  = new Random().NextDouble() * 300;    

        bool turnRight = (new Random().Next(2) == 0);
        if (turnRight)
            TurnRate = Clip(angle, -MaxTurnRate, MaxTurnRate);
        else
            TurnRate = Clip(-angle, -MaxTurnRate, MaxTurnRate);

        movingForward = true;
        TargetSpeed   = 5; 
    }

    private void DoFallbackRadarIfLost()
    {
        if (turnsSinceScan > SCAN_THRESHOLD)
        {
            RadarTurnRate = (RadarTurnRate >= 0) ? MaxRadarTurnRate : -MaxRadarTurnRate;
        }
    }

    private void PerformDodgeManeuver()
    {
        if (dodgeTurnsLeft > 0)
        {
            TurnRate = (dodgeTurnsLeft % 2 == 0) ? 5 : -5;
            dodgeTurnsLeft--;
        }
        else
        {
            dodgeMode = false;
        }
    }

    private double Clip(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
