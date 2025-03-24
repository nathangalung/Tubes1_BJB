using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Kenan : Bot
{   
    
    int cornerIndex = 0; 
    bool stopWhenSeeEnemy = false;
    
    private static readonly Point[] Corners = {
        new Point(40, 40),
        new Point(40, 560),
        new Point(760, 560),
        new Point(760, 40)
    };
    
    private const double KoordinatTengahX = 400;
    private const double KoordinatTengahY = 300;


    static void Main(string[] args)
    {
        new Kenan().Start();
    }

    Kenan() : base(BotInfo.FromFile("kenan.json")) {}

    public override void Run()
    {
        BodyColor = Color.DarkGreen;  
        TurretColor = Color.Yellow; 
        RadarColor = Color.Yellow;  
        
        int gunIncrement = 3;
        
        GoToCorner();

        while (IsRunning)
        {
            int iterations = PenyesuaianArahDerajat();
            int halfIterations = iterations / 2;
            
            for (int i = 0; i < iterations && IsRunning; i++)
            {
                if (i == halfIterations){
                    SenjataKeTengah();
                }
                else {
                    TurnGunRight(gunIncrement);
                }
            }
            gunIncrement *= -1;
        }
    }

    private int PenyesuaianArahDerajat()
    {
        double distFromCenterX = Math.Abs(X - KoordinatTengahX) / KoordinatTengahX;
        double distFromCenterY = Math.Abs(Y - KoordinatTengahY) / KoordinatTengahY;
        
        double cornerFactor = Math.Sqrt(distFromCenterX * distFromCenterX + distFromCenterY * distFromCenterY) / Math.Sqrt(2);
        
        int iterations = 60 - (int)(cornerFactor * 30);
        
        iterations = Math.Max(30, Math.Min(60, iterations));
        if (iterations % 2 != 0) iterations--;
        
        return iterations;
    }
    private void SenjataKeTengah()
    {
        double bearing = GunBearingTo(KoordinatTengahX, KoordinatTengahY);
        TurnGunLeft(bearing);
    }
    public override void OnScannedBot(ScannedBotEvent e)
    {
        var bearingFromGun = GunBearingTo(e.X, e.Y);

        TurnGunLeft(bearingFromGun);

        var distance = DistanceTo(e.X, e.Y);
        if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
            SmartFire(distance);

        if (bearingFromGun == 0)
            Rescan();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        var direction = DirectionTo(e.X, e.Y);
        var gunBearing = NormalizeRelativeAngle(direction - GunDirection);
        TurnGunLeft(gunBearing);

    }
    private void GoToCorner()
    {
        stopWhenSeeEnemy = false;

        Point targetCorner = Corners[cornerIndex];

        double bearing = BearingTo(targetCorner.X, targetCorner.Y);

        TurnLeft(bearing);

        stopWhenSeeEnemy = true;

        Forward(DistanceTo(targetCorner.X, targetCorner.Y));

        TurnGunRight(cornerIndex * 90);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        cornerIndex = (cornerIndex + 1) % Corners.Length; 
        GoToCorner();

    }
    private void SmartFire(double distance)
    {
        if (distance > 200 || Energy < 15)
            Fire(1);
        else if (distance > 100)
            Fire(2);
        else if (distance < 10 || Energy >15)
            Fire(5);
        else
            Fire(3);
    }
    public override void OnWonRound(WonRoundEvent e)
    {
        TurnLeft(36_000);
    }
}