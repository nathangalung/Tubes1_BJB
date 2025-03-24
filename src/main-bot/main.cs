using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BJB : Bot
{
    private bool movingForward = true;
    private Random random = new Random();
    private int turnDirection = 1;
    private const double MaxX = 700;
    private const double MaxY = 500;
    private const double MinX = 100;
    private const double MinY = 100;
    private const double CenterX = 400;
    private const double CenterY = 300;
    private const double SafetyDistance = 150;
    private const double PostCollisionMove = 100;

    static void Main()
    {
        new BJB().Start();
    }

    BJB() : base(BotInfo.FromFile("main.json")) { }

    public override void Run()
    {
        BodyColor = Color.Pink;   
        TurretColor = Color.Red;  
        RadarColor = Color.Red; 

        while (IsRunning)
        {
            CheckAndMove();

            SetTurnRight(random.Next(-180, 180));
            WaitFor(new TurnCompleteCondition(this));
        }
    }

    private void CheckAndMove()
    {
        if (X < MinX || X > MaxX || Y < MinY || Y > MaxY)
        {
            MoveToCenter();
        }
        else
        {
            MoveRandomly();
        }
    }

    private void MoveToCenter()
    {
        TurnToFaceTarget(CenterX, CenterY);
        Forward(DistanceTo(CenterX, CenterY));
        WaitFor(new MoveCompleteCondition(this, DistanceTo(CenterX, CenterY)));
    }

    private void MoveRandomly()
    {
        double distance = random.Next(100, 200);

        if (movingForward)
        {
            if (X + distance > MaxX - SafetyDistance || X - distance < MinX + SafetyDistance ||
                Y + distance > MaxY - SafetyDistance || Y - distance < MinY + SafetyDistance)
            {
                ReverseDirection();
            }
            else
            {
                SetForward(distance);
            }
        }
        else
        {
            if (X + distance > MaxX - SafetyDistance || X - distance < MinX + SafetyDistance ||
                Y + distance > MaxY - SafetyDistance || Y - distance < MinY + SafetyDistance)
            {
                ReverseDirection();
            }
            else
            {
                SetBack(distance);
            }
        }
        SetTurnRight(random.Next(-45, 45));
    }

    public override void OnHitWall(HitWallEvent e)
    {
        MoveToCenter();
    }

    public void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(400);
            movingForward = false;
        }
        else
        {
            SetForward(400);
            movingForward = true;
        }
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

    private void SmartFire(double distance)
    {
        if (distance > 200 || Energy < 15)
            Fire(1);
        else if (distance > 100)
            Fire(2);
        else if (distance < 10 && Energy > 15)
            Fire(5);
        else
            Fire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        var direction = DirectionTo(e.X, e.Y);
        var gunBearing = NormalizeRelativeAngle(direction - GunDirection);
        TurnGunLeft(gunBearing);
    }

    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        turnDirection = bearing >= 0 ? 1 : -1;
        TurnLeft(bearing);
    }
}

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        return bot.TurnRemaining == 0;
    }
}

public class MoveCompleteCondition : Condition
{
    private readonly Bot bot;
    private readonly double distance;

    public MoveCompleteCondition(Bot bot, double distance)
    {
        this.bot = bot;
        this.distance = distance;
    }

    public override bool Test()
    {
        return bot.DistanceRemaining <= 5;
    }
}