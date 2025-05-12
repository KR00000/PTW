//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly object stateLock = new();
        private readonly object velocityLock = new();
        private readonly Data.IBall dataBall;
        private Position currentPosition;

        private readonly double width = 382;
        private readonly double height = 402;
        private readonly double ballSize = 10.0;

    

        public event EventHandler<IPosition>? NewPositionNotification;

        internal Ball(Data.IBall ball)
        {

            dataBall = ball;

            ball.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall


        public IPosition Position
        {
            get
            {
                lock (stateLock)
                {
                    return currentPosition;
                }
            }
        }

        public double Radius => 10.0 / 2.0;

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
      
                lock (stateLock)
                {
                    currentPosition = new Position(e.x, e.y);
                }

                HandleBounce();
            
        }


        private void HandleBounce()
        {
            bool bounced = false;
            double x, y;
            Data.IVector velocity;
            double velocityX, velocityY;

            lock (stateLock)
            {
                if (currentPosition == null)
                    return;

                x = currentPosition.x;
                y = currentPosition.y;
            }


            velocity = dataBall.Velocity;
            velocityX = velocity.x;
            velocityY = velocity.y;

            // Odbicie od scianek (lewa/prawa)
            if (x < 0)
            {
                velocityX = -velocityX;
                x = 0;
                bounced = true;
            }
            else if (x > width - ballSize)
            {
                velocityX = -velocityX;
                x = width - ballSize;
                bounced = true;
            }

            // Odbicie od scianek (gora/doł)
            if (y < 0)
            {
                velocityY = -velocityY;
                y = 0;
                bounced = true;
            }
            else if (y > height - ballSize)
            {
                velocityY = -velocityY;
                y = height - ballSize;
                bounced = true;
            }

            if (bounced)
            {
                Data.IVector newVelocity = CreateNewVector(velocityX, velocityY);
                lock (velocityLock)
                {
                    dataBall.Velocity = newVelocity;
                }

                lock (stateLock)
                {
                    currentPosition = new Position(x, y);
                }
            }
            RaisePositionChangeEvent();
        }
        private Data.IVector CreateNewVector(double x, double y)
        {
            return new Data.Vector(x, y);

        }

        private void RaisePositionChangeEvent()
        {
            Position positionCopy;
            lock (stateLock)
            {
                positionCopy = new Position(currentPosition.x, currentPosition.y);
            }

            EventHandler<IPosition>? handler = NewPositionNotification;
            handler?.Invoke(this, positionCopy);
        }


        public void HandleCollision(Ball otherBall)
        {
        
            Ball firstBall = this.GetHashCode() < otherBall.GetHashCode() ? this : otherBall;
            Ball secondBall = this.GetHashCode() < otherBall.GetHashCode() ? otherBall : this;

            Position pos1, pos2;
            Data.IVector vel1, vel2;
            double radius1, radius2;

            bool shouldProcessCollision = false;
            double dx = 0, dy = 0, distance = 0;

            lock (firstBall.stateLock)
            {
                if (firstBall.currentPosition == null)
                    return;

                pos1 = new Position(firstBall.currentPosition.x, firstBall.currentPosition.y);
                vel1 = firstBall.dataBall.Velocity;
                radius1 = firstBall.Radius;
            }

            lock (secondBall.stateLock)
            {
                if (secondBall.currentPosition == null)
                    return;

                pos2 = new Position(secondBall.currentPosition.x, secondBall.currentPosition.y);
                vel2 = secondBall.dataBall.Velocity;
                radius2 = secondBall.Radius;
            }

            // Obliczenia kolizji bez blokad
            dx = pos2.x - pos1.x;
            dy = pos2.y - pos1.y;
            distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < radius1 + radius2 + 5.0)
            {
                shouldProcessCollision = true;
            }

            if (!shouldProcessCollision)
                return;

            if (distance < 0.01)
            {
                distance = 0.01;
                dx = 0.1;
                dy = 0.1;
            }

           
            double nx = dx / distance;
            double ny = dy / distance;

           
            double overlap = (radius1 + radius2) - distance;
            double correctionFactor = 1;
            double correctionX = overlap * nx * correctionFactor;
            double correctionY = overlap * ny * correctionFactor;

            // Obliczanie nowych prekosci
            double v1n = vel1.x * nx + vel1.y * ny;
            double v2n = vel2.x * nx + vel2.y * ny;

            // Sprawdzamy czy pilki sa blisko siebie
            if (v1n - v2n > 0)
            {
             
                double v1t = vel1.x * -ny + vel1.y * nx;
                double v2t = vel2.x * -ny + vel2.y * nx;

                double restitution = 1.0;

                double v1n_after = v2n * restitution;
                double v2n_after = v1n * restitution;

                double v1x_after = v1n_after * nx - v1t * ny;
                double v1y_after = v1n_after * ny + v1t * nx;
                double v2x_after = v2n_after * nx - v2t * ny;
                double v2y_after = v2n_after * ny + v2t * nx;

                lock (firstBall.stateLock)
                {
                    firstBall.currentPosition = new Position(
                        firstBall.currentPosition.x - correctionX,
                        firstBall.currentPosition.y - correctionY);
                }

                lock (secondBall.stateLock)
                {
                    secondBall.currentPosition = new Position(
                        secondBall.currentPosition.x + correctionX,
                        secondBall.currentPosition.y + correctionY);
                }

                try
                {
                    Data.IVector newVel1 = new Data.Vector(v1x_after, v1y_after);
                    Data.IVector newVel2 = new Data.Vector(v2x_after, v2y_after);

                    firstBall.dataBall.Velocity = newVel1;
                    secondBall.dataBall.Velocity = newVel2;
                }
                catch (Exception ex)
                {
 
                    Console.WriteLine($"Error during speed {ex.Message}");
                }
                Task.Run(() => firstBall.RaisePositionChangeEvent());
                Task.Run(() => secondBall.RaisePositionChangeEvent());
            }
        }

        #endregion private

    }
}