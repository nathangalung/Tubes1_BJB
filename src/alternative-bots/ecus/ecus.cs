using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class ecus : Bot
{
    // The ecus method starts our bot
    static void Main(string[] args)
    {
        new ecus().Start();
    }

    // Constructor, which loads the bot config file
    ecus() : base(BotInfo.FromFile("ecus.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {

        BodyColor = Color.FromArgb(0xFF, 0x8C, 0x00);   // Dark Orange
        TurretColor = Color.FromArgb(0xFF, 0xA5, 0x00); // Orange
        RadarColor = Color.FromArgb(0xFF, 0xD7, 0x00);  // Gold
        BulletColor = Color.FromArgb(0xFF, 0x45, 0x00); // Orange-Red
        ScanColor = Color.FromArgb(0xFF, 0xFF, 0x00);   // Bright Yellow 
        TracksColor = Color.FromArgb(0x99, 0x33, 0x00); // Dark   
        Brownish-Orange
        GunColor = Color.FromArgb(0xCC, 0x55, 0x00);    // Medium Orange
      
        // Repeat while the bot is running
        while (IsRunning)
        {
            Forward(100);
            TurnGunRight(360);
            Back(100);
            TurnGunRight(360);
        }
    }

    // We saw another bot -> fire!
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        Fire(1);
    }

    // We were hit by a bullet -> turn perpendicular to the bullet
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Calculate the bearing to the direction of the bullet
        var bearing = CalcBearing(evt.Bullet.Direction);

        // Turn 90 degrees to the bullet direction based on the bearing
        TurnLeft(90 - bearing);
    }
}

