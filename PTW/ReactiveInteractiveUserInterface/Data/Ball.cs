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

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {

        private readonly object stateLock = new object();
        private Vector velocity;
        private Vector position;
        private bool isRunning = false;
        private Thread? ballThread;
        private double speedFactor = 0.0275;
        private Stopwatch stopwatch = new Stopwatch();



        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            position = initialPosition;
            velocity = initialVelocity;
        }

        #endregion ctor

        #region IBall

        internal void Start()
        {
            if (isRunning) return;

            isRunning = true;
            stopwatch.Start();
            ballThread = new Thread(() =>
            {
                while (isRunning)
                {
                    //Debug.WriteLine($"[Ball Thread] Ball {this.GetHashCode()} is running on Thread ID: {Thread.CurrentThread.ManagedThreadId}");
                    MoveSelf();
                    Thread.Sleep(1000 / 60);
                }

            });

            ballThread.IsBackground = true;
            ballThread.Start();
        }
        private void MoveSelf()
        {


            double deltaTime = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            stopwatch.Restart();

            Vector currentVelocity;
            double currentSpeedFactor;
            lock (stateLock)
            {
                currentVelocity = velocity;
                currentSpeedFactor = speedFactor;
            }

            Vector delta = new(
                currentVelocity.x * speedFactor * deltaTime * 100,
                currentVelocity.y * speedFactor * deltaTime * 100
            );

            Move(delta);
        }

        internal void Stop()
        {
            isRunning = false;
            ballThread?.Join();
        }

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity
        {
            get
            {
                lock (stateLock)
                {
                    return velocity;
                }
            }
            set
            {
                lock (stateLock)
                {
                    velocity = (Vector)value;
                }
            }
        }

        internal void SetSpeedFactor(double factor)
        {
            lock (stateLock)
            {
                speedFactor = factor;
            }
        }

        #endregion IBall

        #region private


        private void RaiseNewPositionChangeNotification()
        {
            Vector positionCopy;


            lock (stateLock)
            {
                positionCopy = new Vector(position.x, position.y);
            }



            EventHandler<IVector>? handler = NewPositionNotification;
            handler?.Invoke(this, positionCopy);
        }

        internal void Move(Vector delta)
        {

            lock (stateLock)
            {
                double newX = position.x + delta.x;
                double newY = position.y + delta.y;
                position = new Vector(newX, newY);

            }

            RaiseNewPositionChangeNotification();
        }

        #endregion private
    }
}