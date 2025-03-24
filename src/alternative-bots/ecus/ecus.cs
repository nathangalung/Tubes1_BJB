using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Ecus : Bot
{
    // Game state variables
    private int enemies;
    private bool stopWhenSeeEnemy = false;
    
    // Boundary constants
    private const int MIN_Y_LOWER = 60;
    private const int MAX_Y_LOWER = 150;
    private const int MIN_Y_UPPER = 450;
    private const int MAX_Y_UPPER = 540;
    private const int MIN_X_LEFT = 80;
    private const int MAX_X_LEFT = 200;
    private const int MIN_X_RIGHT = 600;
    private const int MAX_X_RIGHT = 720;
    private const double ARENA_CENTER_X = 400;
    private const double ARENA_CENTER_Y = 300;
    private const int WALL_SAFETY_DISTANCE = 100;
    private const int STUCK_THRESHOLD = 3;

    // Movement state
    private int currentZone = 0; // 0: top, 1: right, 2: bottom, 3: left
    private bool needToReposition = true;
    private bool moveClockwise = true;
    private bool movingForward = true;
    private double lastX = 0;
    private double lastY = 0;
    private int stuckCounter = 0;
    private Random random = new Random();

    // Main method to start the bot
    static void Main(string[] args) => new Ecus().Start();

    // Constructor loads the bot config file
    Ecus() : base(BotInfo.FromFile("ecus.json")) { }

    // Called when a new round is started
    public override void Run()
    {
        // Set bot colors
        SetBotColors();
        
        enemies = EnemyCount;
        InitializeMovement();
        lastX = X;
        lastY = Y;

        // Main game loop
        while (IsRunning)
        {

            CheckIfStuck();
            CheckPosition();
            
            if (needToReposition)
            {
                MoveToAllowedZone();
                continue;
            }
            
            RandomizedMovement();
            ScanningMode();

            lastX = X;
            lastY = Y;
        }
    }

    // Check if stuck
    private void CheckIfStuck() 
    {
        double distanceMoved = Math.Sqrt(Math.Pow(X - lastX, 2) + Math.Pow(Y - lastY, 2));
        
        if (distanceMoved < 2.0) 
        {
            stuckCounter++;
            if (stuckCounter >= STUCK_THRESHOLD) 
            {
                EmergencyUnstuck();
                stuckCounter = 0;
            }
        } 
        else 
        {
            stuckCounter = 0;
        }
    }

    // Emergency escape
    private void EmergencyUnstuck() 
    {
        bool nearTopWall = Y > ARENA_CENTER_Y && Math.Abs(Y - ArenaHeight) < WALL_SAFETY_DISTANCE;
        bool nearBottomWall = Y < ARENA_CENTER_Y && Math.Abs(Y) < WALL_SAFETY_DISTANCE;
        bool nearRightWall = X > ARENA_CENTER_X && Math.Abs(X - ArenaWidth) < WALL_SAFETY_DISTANCE;
        bool nearLeftWall = X < ARENA_CENTER_X && Math.Abs(X) < WALL_SAFETY_DISTANCE;
        
        if ((nearTopWall && nearRightWall) || 
            (nearTopWall && nearLeftWall) || 
            (nearBottomWall && nearRightWall) || 
            (nearBottomWall && nearLeftWall))
        {
            // Corner escape
            double centerBearing = BearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
            TurnRight(centerBearing);
            Forward(200);
            
            currentZone = (currentZone + 2) % 4;
            moveClockwise = !moveClockwise;
        }
        else if (nearTopWall || nearBottomWall || nearRightWall || nearLeftWall)
        {
            // Wall escape
            double centerBearing = BearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
            TurnRight(centerBearing);
            Forward(150);
            ReverseDirection();
        }
        else 
        {
            // Ramming escape
            TurnRight(random.Next(120, 240));
            Forward(100);
            TurnLeft(90);
            Forward(100);
            ReverseDirection();
        }

        needToReposition = true;
    }

    // Set bot colors
    private void SetBotColors()
    {
        BodyColor = Color.FromArgb(25, 25, 112);    // Midnight blue/purple
        TurretColor = Color.FromArgb(128, 0, 128);  // Purple
        RadarColor = Color.White;                   // White
        BulletColor = Color.FromArgb(169, 169, 169); // Grey
        ScanColor = Color.FromArgb(75, 0, 130);     // Indigo
        GunColor = Color.Black;                     // Black
    }

    // Initialize movement
    private void InitializeMovement()
    {
        moveClockwise = random.Next(2) == 0;
        movingForward = random.Next(2) == 0;
        
        RandomizeStartPosition();
        CheckPosition();
        
        if (needToReposition)
        {
            MoveToAllowedZone();
        }

        SetForward(40000);
        movingForward = true;
    }

    // Scanning mode
    private void ScanningMode()
    {
        int iterations = GetTurnIterations();
        int halfIterations = iterations / 2;
        int gunIncrement = 3;
        
        for (int i = 0; i < iterations && IsRunning; i++)
        {   
            if (i == halfIterations)
            {
                AimGunToCenter();
            }
            else
            {
                SetTurnGunRight(gunIncrement);
            }
            
            // if (i % 3 == 0)
            // {
            //     MakeRandomTurn();
            // }
            
            if (needToReposition)
            {
                MoveToAllowedZone();
            }
            else if (IsNearWall())
            {
                AvoidWall();
            }

            Go();
        }
        gunIncrement *= -1;
    }

    // Random starting position
    private void RandomizeStartPosition()
    {
        currentZone = random.Next(4);
        needToReposition = true;
        moveClockwise = random.Next(2) == 0;
        
        if (IsRunning)
        {
            MoveToAllowedZone();
        }
    }

    // Random movement
    private void RandomizedMovement()
    {
        bool nearWall = IsNearWall();
        
        if (nearWall)
        {
            AvoidWall();
        }
        else if (IsWithinZoneSafely())
        {
            if (random.Next(20) < 3)
            {
                ReverseDirection();
            }
            
            if (random.Next(10) < 7)
            {
                MakeRandomTurn();
            }

            if (random.Next(10) < 2)
            {
                if (movingForward)
                {
                    SetForward(random.Next(40, 100));
                }
                else
                {
                    SetBack(random.Next(40, 100));
                }
            }
        }
        else
        {
            TurnTowardZoneCenter();
        }
    }

    // Check if safely in zone
    private bool IsWithinZoneSafely()
    {
        switch (currentZone)
        {
            case 0: return Y >= MIN_Y_UPPER + 70 && Y <= MAX_Y_UPPER - 70;
            case 1: return X >= MIN_X_RIGHT + 70 && X <= MAX_X_RIGHT - 70;
            case 2: return Y >= MIN_Y_LOWER + 70 && Y <= MAX_Y_LOWER - 70;
            case 3: return X >= MIN_X_LEFT + 70 && X <= MAX_X_LEFT - 70;
            default: return false;
        }
    }

    // Random turn
    private void MakeRandomTurn()
    {
        int turnAmount = random.Next(0,0);
        
        if (random.Next(2) == 0)
        {
            SetTurnRight(turnAmount);
        }
        else
        {
            SetTurnLeft(turnAmount);
        }
        
        if (random.Next(5) < 3)
        {
            if (movingForward)
            {
                SetForward(random.Next(30, 120));
            }
            else
            {
                SetBack(random.Next(30, 120));
            }
        }
        
        if (random.Next(10) < 2)
        {
            ReverseDirection();
        }
    }

    // Turn toward zone center
    private void TurnTowardZoneCenter()
    {
        double targetX = 0;
        double targetY = 0;
        
        switch (currentZone)
        {
            case 0:
                targetX = ARENA_CENTER_X;
                targetY = (MIN_Y_UPPER + MAX_Y_UPPER) / 2;
                break;
            case 1:
                targetX = (MIN_X_RIGHT + MAX_X_RIGHT) / 2;
                targetY = ARENA_CENTER_Y;
                break;
            case 2:
                targetX = ARENA_CENTER_X;
                targetY = (MIN_Y_LOWER + MAX_Y_LOWER) / 2;
                break;
            case 3:
                targetX = (MIN_X_LEFT + MAX_X_LEFT) / 2;
                targetY = ARENA_CENTER_Y;
                break;
        }

        double bearing = BearingTo(targetX, targetY);
        SetTurnRight(bearing);
        
        if (movingForward)
        {
            SetForward(60 + random.Next(40));
        }
        else
        {
            SetBack(60 + random.Next(40));
        }
    }

    // Reverse direction
    private void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(40000);
            movingForward = false;
        }
        else
        {
            SetForward(40000);
            movingForward = true;
        }
    }

    // Aim gun to center
    private void AimGunToCenter()
    {
        double bearing = GunBearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
        TurnGunLeft(bearing);
    }

    // Check if near wall
    private bool IsNearWall()
    {
        bool nearTopWall = Y > ARENA_CENTER_Y && Math.Abs(Y - ArenaHeight) < WALL_SAFETY_DISTANCE;
        bool nearBottomWall = Y < ARENA_CENTER_Y && Math.Abs(Y) < WALL_SAFETY_DISTANCE;
        bool nearRightWall = X > ARENA_CENTER_X && Math.Abs(X - ArenaWidth) < WALL_SAFETY_DISTANCE;
        bool nearLeftWall = X < ARENA_CENTER_X && Math.Abs(X) < WALL_SAFETY_DISTANCE;
        
        return nearTopWall || nearBottomWall || nearRightWall || nearLeftWall;
    }

    // Avoid wall
    private void AvoidWall()
    {
        bool nearTopWall = Y > ARENA_CENTER_Y && Math.Abs(Y - ArenaHeight) < WALL_SAFETY_DISTANCE;
        bool nearBottomWall = Y < ARENA_CENTER_Y && Math.Abs(Y) < WALL_SAFETY_DISTANCE;
        bool nearRightWall = X > ARENA_CENTER_X && Math.Abs(X - ArenaWidth) < WALL_SAFETY_DISTANCE;
        bool nearLeftWall = X < ARENA_CENTER_X && Math.Abs(X) < WALL_SAFETY_DISTANCE;
        
        if ((nearTopWall && nearRightWall) || 
            (nearTopWall && nearLeftWall) || 
            (nearBottomWall && nearRightWall) || 
            (nearBottomWall && nearLeftWall))
        {
            // Corner avoidance
            double centerBearing = BearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
            SetTurnRight(centerBearing);
            SetForward(200);
            movingForward = true;
            needToReposition = true;
            return;
        }

        if (nearTopWall)
        {
            SetTurnRight(270);
            SetForward(130);
            movingForward = true;
        }
        else if (nearBottomWall)
        {
            SetTurnRight(90);
            SetForward(130);
            movingForward = true;
        }
        else if (nearRightWall)
        {
            SetTurnRight(180);
            SetForward(130);
            movingForward = true;
        }
        else if (nearLeftWall)
        {
            SetTurnRight(0);
            SetForward(130);
            movingForward = true;
        }
    }

    // Check position
    private void CheckPosition()
    {
        needToReposition = false;
        
        bool inTopZone = (Y >= MIN_Y_UPPER && Y <= MAX_Y_UPPER);
        bool inBottomZone = (Y >= MIN_Y_LOWER && Y <= MAX_Y_LOWER);
        bool inLeftZone = (X >= MIN_X_LEFT && X <= MAX_X_LEFT);
        bool inRightZone = (X >= MIN_X_RIGHT && X <= MAX_X_RIGHT);
        
        if (!inTopZone && !inBottomZone && !inLeftZone && !inRightZone)
        {
            needToReposition = true;
        }
    }

    // Move to allowed zone
    private void MoveToAllowedZone()
    {
        double distToTop = Math.Min(Math.Abs(Y - MIN_Y_UPPER), Math.Abs(Y - MAX_Y_UPPER));
        double distToBottom = Math.Min(Math.Abs(Y - MIN_Y_LOWER), Math.Abs(Y - MAX_Y_LOWER));
        double distToLeft = Math.Min(Math.Abs(X - MIN_X_LEFT), Math.Abs(X - MAX_X_LEFT));
        double distToRight = Math.Min(Math.Abs(X - MIN_X_RIGHT), Math.Abs(X - MAX_X_RIGHT));
        
        double minDist = Math.Min(Math.Min(distToTop, distToBottom), Math.Min(distToLeft, distToRight));
        
        stopWhenSeeEnemy = true;
        
        if (minDist == distToTop)
        {
            MoveToTopZone();
        }
        else if (minDist == distToRight)
        {
            MoveToRightZone();
        }
        else if (minDist == distToBottom)
        {
            MoveToBottomZone();
        }
        else
        {
            MoveToLeftZone();
        }
        
        double centerBearing = BearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
        TurnLeft(centerBearing);
        
        if (movingForward)
            SetForward(40000);
        else
            SetBack(40000);
        
        stopWhenSeeEnemy = false;
        needToReposition = false;
    }
    
    // Zone movement methods
    private void MoveToTopZone()
    {
        double targetY = (MIN_Y_UPPER + MAX_Y_UPPER) / 2;
        double angle = CalcBearing(90);
        TurnLeft(angle);
        if (Y < targetY) {
            Forward(Math.Abs(Y - targetY));
            movingForward = true;
        } else {
            Back(Math.Abs(Y - targetY));
            movingForward = false;
        }
        currentZone = 0;
    }
    
    private void MoveToRightZone()
    {
        double targetX = (MIN_X_RIGHT + MAX_X_RIGHT) / 2;
        double angle = CalcBearing(0);
        TurnLeft(angle);
        if (X < targetX) {
            Forward(Math.Abs(X - targetX));
            movingForward = true;
        } else {
            Back(Math.Abs(X - targetX));
            movingForward = false;
        }
        currentZone = 1;
    }
    
    private void MoveToBottomZone()
    {
        double targetY = (MIN_Y_LOWER + MAX_Y_LOWER) / 2;
        double angle = CalcBearing(270);
        TurnLeft(angle);
        if (Y < targetY) {
            Forward(Math.Abs(Y - targetY));
            movingForward = true;
        } else {
            Back(Math.Abs(Y - targetY));
            movingForward = false;
        }
        currentZone = 2;
    }
    
    private void MoveToLeftZone()
    {
        double targetX = (MIN_X_LEFT + MAX_X_LEFT) / 2;
        double angle = CalcBearing(180);
        TurnLeft(angle);
        if (X < targetX) {
            Forward(Math.Abs(X - targetX));
            movingForward = true;
        } else {
            Back(Math.Abs(X - targetX));
            movingForward = false;
        }
        currentZone = 3;
    }

    // Calculate gun turn iterations
    private int GetTurnIterations()
    {
        double distFromCenterX = Math.Abs(X - ARENA_CENTER_X) / ARENA_CENTER_X;
        double distFromCenterY = Math.Abs(Y - ARENA_CENTER_Y) / ARENA_CENTER_Y;
        double cornerFactor = Math.Sqrt(distFromCenterX * distFromCenterX + distFromCenterY * distFromCenterY) / Math.Sqrt(2);
        
        int iterations = 60 - (int)(cornerFactor * 30);
        iterations = Math.Max(30, Math.Min(60, iterations));
        if (iterations % 2 != 0) iterations--;
        
        return iterations;
    }

    // Event handling methods
    public override void OnScannedBot(ScannedBotEvent e)
    {
        var distance = DistanceTo(e.X, e.Y);

        if (stopWhenSeeEnemy)
        {
            Stop();
            SmartFire(distance);
            Rescan();
            Resume();
        }
        else
            SmartFire(distance);
    }

    private void SmartFire(double distance)
    {
        if (distance > 200 || Energy < 15)
            Fire(1);
        else if (distance > 50)
            Fire(2);
        else
            Fire(3);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
        TurnRight(random.Next(120, 160) * (random.Next(2) == 0 ? 1 : -1));
        
        if (movingForward)
        {
            Forward(120 + random.Next(80));
        }
        else
        {
            Back(120 + random.Next(80));
        }
        
        bool nearTopWall = Y > ARENA_CENTER_Y && Math.Abs(Y - ArenaHeight) < 80;
        bool nearBottomWall = Y < ARENA_CENTER_Y && Math.Abs(Y) < 80;
        bool nearRightWall = X > ARENA_CENTER_X && Math.Abs(X - ArenaWidth) < 80;
        bool nearLeftWall = X < ARENA_CENTER_X && Math.Abs(X) < 80;
        
        if ((nearTopWall && nearRightWall) || 
            (nearTopWall && nearLeftWall) || 
            (nearBottomWall && nearRightWall) || 
            (nearBottomWall && nearLeftWall))
        {
            double centerBearing = BearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
            TurnRight(centerBearing);
            Forward(180);
            moveClockwise = !moveClockwise;
        }
        
        needToReposition = true;
        currentZone = DetermineCurrentZone();
        MoveToAllowedZone();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double botBearing = BearingTo(e.X, e.Y);

        if (e.IsRammed)
        {
            ReverseDirection();
            TurnRight(botBearing + 180);
            
            if (movingForward)
            {
                Forward(180 + random.Next(70));
            }
            else
            {
                Back(180 + random.Next(70));
            }
            
            currentZone = (currentZone + 2) % 4;
            needToReposition = true;
            stuckCounter = 0;
            return;
        }
        
        bool nearTopWall = Y > ARENA_CENTER_Y && Math.Abs(Y - ArenaHeight) < 80;
        bool nearBottomWall = Y < ARENA_CENTER_Y && Math.Abs(Y) < 80;
        bool nearRightWall = X > ARENA_CENTER_X && Math.Abs(X - ArenaWidth) < 80;
        bool nearLeftWall = X < ARENA_CENTER_X && Math.Abs(X) < 80;
        
        if ((nearTopWall && nearRightWall) || 
            (nearTopWall && nearLeftWall) || 
            (nearBottomWall && nearRightWall) || 
            (nearBottomWall && nearLeftWall))
        {
            double centerBearing = BearingTo(ARENA_CENTER_X, ARENA_CENTER_Y);
            TurnRight(centerBearing);
            Forward(200);
            
            moveClockwise = !moveClockwise;
            currentZone = (currentZone + 2) % 4;
            needToReposition = true;
            MoveToAllowedZone();
            return;
        }
        
        if (nearTopWall || nearBottomWall)
        {
            TurnRight(botBearing + 90 * (random.Next(2) == 0 ? 1 : -1));
        }
        else if (nearRightWall || nearLeftWall)
        {
            TurnRight(botBearing + 90 * (random.Next(2) == 0 ? 1 : -1));
        }
        else
        {
            TurnRight(botBearing + 180);
        }
        
        if (movingForward)
        {
            Forward(150 + random.Next(50));
        }
        else
        {
            Back(150 + random.Next(50));
        }

        needToReposition = true;
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        double bulletAngle = e.Bullet.Direction;
        double ourDirection = Direction;
        double relativeBulletAngle = Math.Abs(bulletAngle - ourDirection);
        
        while (relativeBulletAngle > 180)
            relativeBulletAngle = Math.Abs(relativeBulletAngle - 360);
        
        if (relativeBulletAngle > 135)
        {
            int oppositeZone = (currentZone + 2) % 4;
            currentZone = oppositeZone;
            ReverseDirection();
            moveClockwise = !moveClockwise;
            needToReposition = true;
            
            TurnRight(random.Next(-45, 46));
            Forward(100);
            
        }
        else
        {
            ReverseDirection();
            TurnRight(20 * (random.Next(2) == 0 ? 1 : -1));
        }
    }

    public override void OnBulletHitBullet(BulletHitBulletEvent e)
    {
        TurnRight(90 * (random.Next(2) == 0 ? 1 : -1));
        Back(50 + random.Next(50));
        
        if (random.Next(3) == 0)
        {
            moveClockwise = !moveClockwise;
        }
        
        needToReposition = true;
    }

    private int DetermineCurrentZone()
    {
        if (Y >= MIN_Y_UPPER && Y <= MAX_Y_UPPER) {
            return 0;
        } else if (X >= MIN_X_RIGHT && X <= MAX_X_RIGHT) {
            return 1;
        } else if (Y >= MIN_Y_LOWER && Y <= MAX_Y_LOWER) {
            return 2;
        } else if (X >= MIN_X_LEFT && X <= MAX_X_LEFT) {
            return 3;
        } else {
            double distToTop = Math.Min(Math.Abs(Y - MIN_Y_UPPER), Math.Abs(Y - MAX_Y_UPPER));
            double distToBottom = Math.Min(Math.Abs(Y - MIN_Y_LOWER), Math.Abs(Y - MAX_Y_LOWER));
            double distToLeft = Math.Min(Math.Abs(X - MIN_X_LEFT), Math.Abs(X - MAX_X_LEFT));
            double distToRight = Math.Min(Math.Abs(X - MIN_X_RIGHT), Math.Abs(X - MAX_X_RIGHT));
            
            double minDist = Math.Min(Math.Min(distToTop, distToBottom), Math.Min(distToLeft, distToRight));
            
            if (minDist == distToTop) return 0;
            if (minDist == distToRight) return 1;
            if (minDist == distToBottom) return 2;
            return 3;
        }
    }

    public override void OnDeath(DeathEvent e)
    {
        if (enemies == 0) return;
        RandomizeStartPosition();
    }
}